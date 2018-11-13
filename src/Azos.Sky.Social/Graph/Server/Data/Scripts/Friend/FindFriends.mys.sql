SELECT *
FROM tbl_friends
WHERE G_OWN = ?pNode 
AND LST LIKE ?pList
AND STS LIKE ?pStatus
LIMIT ?pFetchStart, ?pFetchCount
