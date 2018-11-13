SELECT G_OWN AS G_Owner
, G_VOL AS G_SubscriberVolume
, CNT AS Count
, CDT AS Create_Date
FROM tbl_subvol
WHERE G_OWN = ?pNode
ORDER BY CDT