using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplaceGeomTest
{
    class Program
    {

        [STAThread] // important
        static void Main(string[] args)
        {

            String extdll = @"C:\Program Files\Manifold\v9.0\bin64\ext.dll";
            using (Manifold.Root root = new Manifold.Root(extdll))
            {
                Manifold.Application app = root.Application;
                String mapfile = Path.GetFullPath(@"m9_DisplaceGeomTest.map");

                using (Manifold.Database db = app.CreateDatabaseForFile(mapfile, true))
                {
                    Script.CreateQueries(app, db);
                    db.Save();
                 
                }
            }
        }
    }
}
