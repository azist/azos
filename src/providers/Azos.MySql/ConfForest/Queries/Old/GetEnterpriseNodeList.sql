select
(
  select
    json_pretty(json_object('g_ver', hex(TL.GDID),
                            'gdid', hex(TL.G_NODE),
                            'mnemonic', TL.MNEMONIC,
                            'caption', TL.CAPTION,
                            'start_utc', TL.START_UTC,
                            'version_state', TL.VERSION_STATE))
  from
    tbl_hnodelog TL
  where
    (TL.G_NODE = TN.GDID) AND (TL.G_PARENT is NULL) AND (TL.START_UTC <= @asof)
  order by
    TL.START_UTC desc, TL.VERSION_UTC desc
  limit 1
) as DATA
from
  tbl_hnode TN
where
  TN.ETYPE = 'EN'
having
  `DATA` is not null
