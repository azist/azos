select
 TL.GDID,
 TL.VERSION_UTC,
 TL.VERSION_ORIGIN,
 TL.VERSION_ACTOR,
 TL.VERSION_STATE
from
 tbl_hnode TN inner join tbl_hnodelog TL on TN.GDID = TL.G_NODE
where
 (TN.GDID = @gnode) AND (TN.ETYPE = @etp) AND (TL.G_NODE = @gnode)
order by
 TL.VERSION_UTC ASC
