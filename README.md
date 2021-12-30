# DisplaceGeom
 Manifold 9 addin


GeomUpdateCoord builds a new geom from `geom` where coord at `index` is replaced with `newCoord`

GeomAdjustCoord builds a new geom from `geom` where coord at `index` is moved to `newCoord` and nearby coords are adjusted according to distance from change origin. 
Line endpoints are not adjusted, unless referred by `index`.

