insert into tbl_hnodelog
(
  `GDID`,
  `VERSION_UTC`,
  `VERSION_ORIGIN`,
  `VERSION_ACTOR`,
  `VERSION_STATE`,
  `G_NODE`,
  `G_PARENT`,
  `MNEMONIC`,
  `CAPTION`,
  `START_UTC`,
  `PROPERTIES`,
  `CONFIG`
)
select
  @gdid,
  @version_utc,
  @version_origin,
  @version_actor,
  @version_state,
  @g_node,
  @g_parent,
  @mnemonic,
  @caption,
  @start_utc,
  @properties,
  @config
from
  tbl_hnode TN
where
  TN.GDID = @g_node AND TN.ETYPE = @etype
