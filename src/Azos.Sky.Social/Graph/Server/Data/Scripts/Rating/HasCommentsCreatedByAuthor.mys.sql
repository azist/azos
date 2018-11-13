SELECT *
  FROM tbl_comment 
WHERE
  DIM = ?pDim AND G_ATR = ?pAuthor AND G_TRG = ?pNode AND ROOT = 'T' AND In_Use = 'T'
LIMIT 1
