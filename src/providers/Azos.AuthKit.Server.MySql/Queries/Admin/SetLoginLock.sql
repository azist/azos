update tbl_login set
  LOCK_START_UTC = @start_utc,
  LOCK_END_UTC =  @end_utc,
  LOCK_ACTOR   = @actor,
  LOCK_NOTE    = @note
where GDID = @gdid
