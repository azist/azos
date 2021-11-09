﻿insert into tbl_nodelog
(
  `GDID`,
  `VERSION_UTC`,
  `VERSION_ORIGIN`,
  `VERSION_ACTOR`,
  `VERSION_STATE`,
  `G_NODE`,
  `G_PARENT`,
  `PATH_SEGMENT`,
  `START_UTC`,
  `PROPERTIES`,
  `CONFIG`
)
values
(
  @gdid,
  @version_utc,
  @version_origin,
  @version_actor,
  @version_state,
  @gnode,
  @gparent,
  @psegment,
  @start_utc,
  @properties,
  @config
)
