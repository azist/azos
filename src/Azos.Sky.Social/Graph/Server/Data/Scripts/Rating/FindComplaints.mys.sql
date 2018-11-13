SELECT
  G_CMT AS G_Comment,
  GDID,
  G_ATH AS G_AuthorNode,
  KND AS Kind,
  MSG AS Message,
  CDT AS Create_Date,
  In_Use
FROM tbl_complaint
WHERE G_CMT = ?pComment