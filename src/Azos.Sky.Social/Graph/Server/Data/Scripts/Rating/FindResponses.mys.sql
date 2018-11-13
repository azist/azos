SELECT G_VOL AS G_CommentVolume
, GDID
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
, CMP AS Complaint
, PST AS PublicationState
, RTG AS Rating
, In_Use
, RCNT as ResponseCount
FROM tbl_comment WHERE G_PAR = ?pComment AND G_VOL = ?pVolume AND (In_Use = 'T' OR (In_Use = 'F' AND RCNT > 0))