SELECT G_NOD AS G_Node
, CDT AS Create_Date
, DIM AS Dimension
, CNT AS Cnt
, LCD AS Last_Change_Date
, RTG1 AS Rating1
, RTG2 AS Rating2
, RTG3 AS Rating3
, RTG4 AS Rating4
, RTG5 AS Rating5
FROM tbl_noderating 
WHERE G_NOD = ?pNode 
  AND DIM = ?pDim  
ORDER BY CDT DESC 
LIMIT 1