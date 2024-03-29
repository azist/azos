﻿select
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
  TL.GDID = @gver
