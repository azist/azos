select
 TL.GDID,
 TL.VERSION_UTC,
 TL.VERSION_ORIGIN,
 TL.VERSION_ACTOR,
 TL.VERSION_STATE
from
 tbl_node TN inner join tbl_nodelog TL on TN.GDID = TL.G_NODE
where
 (TN.GDID = @gnode) AND (TL.G_NODE = @gnode)
order by
 TL.VERSION_UTC ASC
