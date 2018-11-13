SELECT GDID 
, G_VOL AS G_CommentVolume
, G_ATR AS G_AuthorNode
, G_TRG AS G_TargetNode
, DIM AS Dimension
, ROOT AS IsRoot
, G_PAR AS G_Parent
, MSG AS Message
, DAT AS Data
, CDT AS Create_Date
, LKE AS "Like"
, DIS AS Dislike
, CMP AS ComplaintCount
, PST AS PublicationState
, RTG AS Rating
, In_Use 
, RCNT as ResponseCount
FROM tbl_comment 
WHERE (G_TRG = ?pNode) AND (DIM = ?pDim) AND (ROOT = ?pRoot) AND (In_Use = 'T' OR RCNT > 0)