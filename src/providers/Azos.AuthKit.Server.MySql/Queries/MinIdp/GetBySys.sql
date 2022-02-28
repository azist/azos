﻿select
  TU.GDID
  , TU.REALM
  , TU.LEVEL
  , TL.LEVEL_DOWN
  , TU.CREATE_UTC
  , TU.START_UTC
  , TU.END_UTC
  , TL.GDID as G_Login
  , TL.ID
  , TL.PWD
  , TL.START_UTC AS LOGIN_START_UTC
  , TL.END_UTC AS LOGIN_END_UTC
  , TU.NAME
  , TU.DESCRIPTION
  , TU.PROPS
  , TU.RIGHTS
  , TL.PROPS AS LOGIN_PROPS
  , TL.RIGHTS AS LOGIN_RIGHTS
  , TU.NOTE
  , TU.VERSION_STATE
  , TL.VERSION_STATE AS LOGIN_VERSION_STATE
from
  tbl_login TL INNER JOIN tbl_user TU
    ON TL.G_USER = TU.GDID
where
  TL.GDID = @glogin
  AND TL.REALM = @realm
  AND TU.REALM = @realm