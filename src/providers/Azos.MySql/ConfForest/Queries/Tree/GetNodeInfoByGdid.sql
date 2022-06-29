select
  TN.GDID AS GDID,
  TL.GDID AS G_VERSION,
  TL.G_PARENT,
  TL.VERSION_UTC,
  TL.VERSION_ORIGIN,
  TL.VERSION_ACTOR,
  TL.VERSION_STATE,
  TL.PATH_SEGMENT,
  TL.START_UTC,
  TL.PROPERTIES,
  TL.CONFIG
from
  tbl_node TN inner join tbl_nodelog TL on TN.GDID = TL.G_NODE
where
  (TN.GDID = @gdid) AND
  (TL.G_NODE = @gdid) AND (TL.START_UTC <= @asof)
order by
 TL.START_UTC desc, TL.VERSION_UTC desc
limit 1
