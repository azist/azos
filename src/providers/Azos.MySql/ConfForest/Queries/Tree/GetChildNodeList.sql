select
(
  select
    json_pretty(json_object('g_ver', hex(TL.GDID),
                            'gdid', hex(TL.G_NODE),
                            'psegment', TL.PATH_SEGMENT,
                            'start_utc', TL.START_UTC,
                            'version_state', TL.VERSION_STATE))
  from
    tbl_nodelog TL
  where
    (TL.G_NODE = TN.GDID) 
    AND (TL.G_PARENT = @gparent) 
    AND (TL.START_UTC <= @asof)
    AND (TL.G_PARENT <> TL.G_NODE)
  order by
    TL.START_UTC desc, TL.VERSION_UTC desc
  limit 1
) as DATA
from
  tbl_node TN
having
  `DATA` is not null
