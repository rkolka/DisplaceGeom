-- $manifold$

-- GeomUpdateCoord builds a new geom from `geom` where coord at `index` is replaced with `newCoord`
FUNCTION GeomUpdateCoord(@geom GEOM, @index INT32, @newCoord FLOAT64x2) GEOM AS SCRIPT FILE 'DisplaceGeom.dll' ENTRY 'Script.GeomUpdateCoord';

-- GeomAdjustCoord builds a new geom from `geom` where coord at `index` is moved to `newCoord` and nearby coords are adjusted according to distance from change origin. 
-- Line endpoints are not adjusted, unless referred by `index`.
FUNCTION GeomAdjustCoord(@geom GEOM, @index INT32, @newCoord FLOAT64x2) GEOM AS SCRIPT FILE 'DisplaceGeom.dll' ENTRY 'Script.GeomAdjustCoord';

