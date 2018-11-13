SELECT G_OWN AS G_Owner 
, G_VOL AS G_CommentVolume
, DIM AS Dimension
, CNT AS Count
, CDT AS Create_Date
FROM tbl_commentvol 
WHERE G_OWN = ?pNode AND DIM = ?pDim AND CNT < ?pMaxCount 
ORDER BY CDT DESC 
LIMIT 0,1