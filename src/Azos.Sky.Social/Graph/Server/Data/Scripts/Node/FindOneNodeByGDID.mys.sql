SELECT GDID AS GDID
, TYP AS Node_Type
, G_OSH AS G_OriginShard
, G_ORI AS G_Origin
, ONM AS Origin_Name
, ODT AS Origin_Data
, CDT AS Create_Date
, FVI AS Friend_Visibility
, IN_USE AS In_Use
FROM tbl_node 
WHERE GDID = ?pgnode AND IN_USE = 'T'