SELECT G_OWN AS G_Owner
, G_VOL AS G_SubscriberVolume
, CNT AS Count
, CDTCreate_Date
FROM tbl_subvol
WHERE G_OWN = ?pNode
ORDER BY CDT
LIMIT ?pStart, 1