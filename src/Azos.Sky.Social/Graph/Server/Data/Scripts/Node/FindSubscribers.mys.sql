SELECT G_VOL AS G_SubscriberVolume
, G_SUB AS G_Subscriber
, STP AS Subs_Type
, CDT AS Create_Date
, PAR AS Parameters
FROM tbl_subscriber 
WHERE G_VOL = ?pVol 
ORDER BY G_VOL, G_SUB 
LIMIT ?pStart, ?pCount
