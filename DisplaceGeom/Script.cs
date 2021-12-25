using Manifold;
using System;
using System.IO;


public class Script
{
    private static readonly string AddinName = "DisplaceGeom";
    private static readonly string AddinCodeFolder = "Code\\DisplaceGeom";

    private static readonly string[] CodeFiles = { "DisplaceGeom.sql" };


    private static Context Manifold;

    public static void Main()
    {
        // The current application context 
        Application app = Manifold.Application;

        using (Database db = app.GetDatabaseRoot())
        {
            CreateQueries(app, db);
        }

        app.Log(DisplayHelp());
        app.OpenLog();

    }



    private static string DisplayHelp()
    {
        return "Use include directive:\r\n-- $include$ [DisplaceGeom.sql]";
    }




    public static bool IndexIsInRange(Manifold.Geom geom, int index)
    {
        return (index >= 0 && index <= geom.Coords.Count - 1);
    }

    public static bool IndexIsEndPoint(Manifold.Geom geom, int index)
    {
        return (index == 0 || index == geom.Coords.Count - 1);
    }


    /// <summary>
    /// Builds a new geom from ´geom´ where coord at `index´ is replaced with `newCoord`
    /// </summary>
    /// <param name="geom">a geom</param>
    /// <param name="index">an integer</param>
    /// <param name="newCoord">a float64x2</param>
    /// <returns>New geom with one coordinate changed, or geom if index out of range.</returns>
    public static Manifold.Geom GeomUpdateCoord(Manifold.Geom geom, int index, Manifold.Point<double> newCoord)
    {
        if (!IndexIsInRange(geom, index))
        {
            return geom;
        }

        Manifold.GeomBuilder builder = Manifold.Application.CreateGeomBuilder();

        switch (geom.Type)
        {
            case "point":
                builder.StartGeomPoint();
                break;
            case "line":
                builder.StartGeomLine();
                break;
            case "area":
                builder.StartGeomArea();
                break;
            default:
                return geom;
        }

        // 
        int i = 0;

        for (int bi = 0; bi < geom.Branches.Count; bi++)
        {
            var gbranch = geom.Branches[bi];
            builder.AddBranch();

            for (int ci = 0; ci < gbranch.Coords.Count; ci++)
            {
                if (i != index)
                {
                    builder.AddCoord(gbranch.Coords[ci]);
                }
                else
                {
                    builder.AddCoord(newCoord);
                }
                i++;
            }
            builder.EndBranch();
        }


        return builder.EndGeom();

    }


    public static double PointDistance(Manifold.Point<double> a, Manifold.Point<double> b)
    {
        return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
    }
    /// <summary>
    /// Builds a new geom from ´geom´ where coord at `index´ is moved to `newCoord` and nearby coords are adjusted according to distance from change origin. 
    /// Endpoints are not adjusted, unless intended by ´index´.
    /// </summary>
    /// <param name="geom">a geom</param>
    /// <param name="index">an integer</param>
    /// <param name="newCoord">a float64x2</param>
    /// <returns>New geom with coordinates adjusted, or geom if index out of range.</returns>
    public static Manifold.Geom GeomAdjustCoord(Manifold.Geom geom, int index, Manifold.Point<double> newCoord)
    {
        if (!IndexIsInRange(geom, index))
        {
            return geom;
        }

        double displacementRatioCutoff = 0.1;
        Manifold.Point<double> changeOrigin = geom.Coords[index];
        Manifold.Point<double> displacementVector = new Manifold.Point<double>(newCoord.X - changeOrigin.X, newCoord.Y - changeOrigin.Y);

        double displacementLength = PointDistance(changeOrigin, newCoord);

        if (displacementLength == 0)
        {
            return geom;
        }


        Manifold.GeomBuilder builder = Manifold.Application.CreateGeomBuilder();

        switch (geom.Type)
        {
            case "point":
                builder.StartGeomPoint();
                break;
            case "line":
                builder.StartGeomLine();
                break;
            case "area":
                builder.StartGeomArea();
                break;
            default:
                return geom;
        }

        int i = 0;

        for (int bi = 0; bi < geom.Branches.Count; bi++)
        {
            var gbranch = geom.Branches[bi];
            builder.AddBranch();

            for (int ci = 0; ci < gbranch.Coords.Count; ci++)
            {
                Manifold.Point<double> currentPoint = geom.Coords[i];

                // Find, how much to displace  1 / ( 1 + x )
                double displacementRatio = displacementLength / (displacementLength + PointDistance(currentPoint, changeOrigin));

                // do not bother with too small changes.
                if (displacementRatio < displacementRatioCutoff)
                {
                    displacementRatio = 0;
                }

                // For lines, do not displace endpoints, unless i==index (it's the origin of change)
                if ((i != index) && (geom.Type == "line") && IndexIsEndPoint(geom, i))
                {
                    displacementRatio = 0;
                }

                if (displacementRatio == 0)
                {
                    builder.AddCoord(gbranch.Coords[ci]);
                }
                else
                {
                    // displacement-vector scaled with displacement-ratio is added to current coord.
                    builder.AddCoord(new Manifold.Point<double>(gbranch.Coords[ci].X + displacementRatio * displacementVector.X, gbranch.Coords[ci].Y + displacementRatio * displacementVector.Y));
                }
                i++;
            }
            builder.EndBranch();
        }
        return builder.EndGeom();

    }


    public static void CreateQuery(Application app, Database db, string name, string text, string folder = "")
    {
        PropertySet propertyset = app.CreatePropertySet();
        propertyset.SetProperty("Text", text);
        if (folder != "")
        {
            propertyset.SetProperty("Folder", folder);

        }
        db.Insert(name, "query", null, propertyset);

    }

    public static void CreateQueries(Application app, Database db)
    {
        string AddinDir = System.IO.Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);

        foreach (string fname in CodeFiles)
        {
            bool rewrite = true;

            if (db.GetComponentType(fname) == "")
            {
                rewrite = true;
            }
            else
            {
                rewrite = false;

                string message = $"{db.GetComponentType(fname).ToUpper()} {fname} already exists. DROP?";

                System.Windows.Forms.MessageBoxButtons buttons = System.Windows.Forms.MessageBoxButtons.YesNo;
                System.Windows.Forms.DialogResult result = System.Windows.Forms.MessageBox.Show(message, AddinName, buttons);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    db.Delete(fname);
                    rewrite = true;
                }
            }

            if (rewrite)
            {
                string text = File.ReadAllText(AddinDir + "\\" + fname);
                // TODO? check hash?

                // insert
                CreateQuery(app, db, fname, text, AddinCodeFolder);
            }
        }
    }


}
