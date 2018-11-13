SELECT G_VOL AS G_SubscriberVolume
, G_SUB AS G_Subscriber
, STP AS Subs_Type
, CDT AS Create_Date
, PAR AS Parameters
FROM tbl_subscriber
WHERE G_VOL = ?pVol
AND G_SUB = ?pSub