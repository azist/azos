﻿select
  TN.GDID AS GDID,
  TL.GDID AS G_VERSION,
  TL.G_PARENT,
  TL.VERSION_UTC,
  TL.VERSION_ORIGIN,
  TL.VERSION_ACTOR,
  TL.VERSION_STATE,
  TL.MNEMONIC,
  TL.CAPTION,
  TL.START_UTC,
  TL.PROPERTIES,
  TL.CONFIG
from
  tbl_hnode TN inner join tbl_hnodelog TL on TN.GDID = TL.G_NODE
where
  (TL.MNEMONIC = @mnemonic) AND (TL.START_UTC <= @asof) AND (TN.ETYPE = @etp)
order by
 TL.START_UTC desc, TL.VERSION_UTC desc
limit 1