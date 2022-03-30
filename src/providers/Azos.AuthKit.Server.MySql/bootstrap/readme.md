# Azos AuthKit MySQL Bootstrap Setup README

This document contains the current DDL and initial seed requirements for the underlying data store implementation. It details the necessary database create scripts, the inititial table create scripts, and the minimum required seed values need to operate the API.

## I. User/Login Database and Table Create Steps

### 1. Create the User/Login Database ("authkit_usr")

```sql
CREATE DATABASE `authkit_usr`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';
```

### 2. Create the User/Login Tables ("authkit_usr")

```sql
delimiter ;.
use authkit_usr;.

delimiter ;.

-- groups and roles are stored in Config tree, addressed by pass via PROPS
create table tbl_user
(
  `GDID`       BINARY(12)        not null comment 'User Account Immutable UK',
  `GUID`       BINARY(16)        not null comment 'User Account Immutable Guid Network Byte Order, used for Chronicle (co)REL(ation)',
  `REALM`      BIGINT UNSIGNED   not null comment 'Realm Atom',

  `NAME`        VARCHAR(64)      not null comment 'User name',
  `LEVEL`       CHAR(1)          not null comment 'User archetype access level: I|U|A|S',
  `DESCRIPTION` VARCHAR(128)     not null comment 'User description',

  `START_UTC`  DATETIME       not null    comment 'When user privilege takes effect',
  `END_UTC`    DATETIME       not null    comment 'When user privilege stops',
  `ORG_UNIT`   VARCHAR(256)               comment 'Tree path for org unit. So the user list may be searched by it',
  `PROPS`      MEDIUMTEXT     not null    comment 'Properties such as tree connections (e.g. roles) and claims',
  `RIGHTS`     MEDIUMTEXT                 comment 'User-specific Rights override or null for default rights',

  `NOTE`       MEDIUMTEXT                 comment 'Free form text notes associated with the account',

  `CREATE_UTC`    DATETIME         not null comment 'Creation UTC timestamp',
  `CREATE_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this record first originated from',
  `CREATE_ACTOR`  VARCHAR(256)     not null comment 'Who created the record',

  `LOCK_START_UTC` DATETIME           comment 'Lock timestamp, if set the account is inactive past that timestamp, until LOCK_END_UTC',
  `LOCK_END_UTC`   DATETIME           comment 'If present, resets lock after that point in time',
  `LOCK_ACTOR`     VARCHAR(256)       comment 'Who locked the user account',
  `LOCK_NOTE`      VARCHAR(128)       comment 'Short note explaining lock reason/status',

  `VERSION_UTC`    DATETIME       not null comment 'Version UTC timestamp',
  `VERSION_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this version originated from',
  `VERSION_ACTOR`  VARCHAR(256)   not null comment 'Who made the change',
  `VERSION_STATE`  CHAR(1)        not null comment 'The state of data: Inserted/Updated/Deleted',

  constraint `pk_tbl_user_pk` primary key (`GDID`)
)
    comment = 'Stores user accounts'
;.

delimiter ;.
  create unique index `idx_tbl_user_ukguid` on `tbl_user`(`GUID`);.

delimiter ;.
  create unique index `idx_tbl_user_ukname` on `tbl_user`(`REALM`, `NAME`);.

delimiter ;.
  create index `idx_tbl_user_org` on `tbl_user`(`REALM`, `ORG_UNIT`);.


-- delimiter ;.

-- create table tbl_tag
-- (
--   `GDID`       BINARY(12)        not null comment 'User Tag Immutable UK',
--   `G_USER`     BINARY(12)        not null comment 'FK to USER ACCOUNT table',
--   `TAG`        BIGINT UNSIGNED   not null comment 'Tag id: Atom',
--   `VALUE`      VARCHAR(64)       not null comment 'Tag value: String',
--    constraint `pk_tbl_tag_pk` primary key (`GDID`),
--    constraint `fk_tbl_tag_user` foreign key (`G_USER`) references `tbl_user`(`GDID`)
-- )
--    comment = 'Stores searchable tags for user accounts'
-- ;,



delimiter ;.

create table tbl_login
(
  `GDID`       BINARY(12)        not null comment 'Login UK',
  `REALM`      BIGINT UNSIGNED   not null comment 'Realm Atom',

  `G_USER`     BINARY(12)        not null comment 'FK to USER ACCOUNT table',
  `LEVEL_DOWN` CHAR(1)                    comment 'User level restriction: I|U|A',

  `ID`         VARCHAR(700)      not null comment 'Login ID, or provider key',
  `TID`        BIGINT UNSIGNED   not null comment 'Login Type Atom',
  `PROVIDER`   BIGINT UNSIGNED   not null comment 'Login provider, e.g.  AZOS, FBK, TWT, AD, SYSTEM X etc.. or Atom.ZERO for not eternal provider',
  `PWD`        VARCHAR(2048)              comment 'Password vector, or NULL for providers who dont need it',
  `PROVIDER_DATA`     MEDIUMTEXT          comment 'Optional extra provider -specific JSON vector',

  `START_UTC`  DATETIME       not null    comment 'When login privilege takes effect',
  `END_UTC`    DATETIME       not null    comment 'When login privilege stops',
  `PROPS`      MEDIUMTEXT                 comment 'Properties such as tree connections (e.g. roles) and claims',
  `RIGHTS`     MEDIUMTEXT                 comment 'Rights override or null',

  `CREATE_UTC`    DATETIME         not null comment 'Creation UTC timestamp',
  `CREATE_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this record first originated from',
  `CREATE_ACTOR`  VARCHAR(256)     not null comment 'Who created the record',

  `LOCK_START_UTC` DATETIME           comment 'Lock timestamp, if set the account is inactive past that timestamp, until LOCK_END_UTC',
  `LOCK_END_UTC`   DATETIME           comment 'If present, resets lock after that point in time',
  `LOCK_ACTOR`     VARCHAR(256)       comment 'Who locked the user account',
  `LOCK_NOTE`      VARCHAR(128)       comment 'Short note explaining lock reason/status',

  `VERSION_UTC`    DATETIME       not null comment 'Version UTC timestamp',
  `VERSION_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this version originated from',
  `VERSION_ACTOR`  VARCHAR(256)   not null comment 'Who made the change',
  `VERSION_STATE`  CHAR(1)        not null comment 'The state of data: Inserted/Updated/Deleted',

  constraint `pk_tbl_login_primary` primary key (`GDID`),
  constraint `fk_tbl_login_user` foreign key (`G_USER`) references `tbl_user`(`GDID`)
)
   comment = 'Stores user account login information'
;.

delimiter ;.
  create unique index `idx_tbl_login_uk1` on `tbl_login`(`REALM`, `ID`, `TID`, `PROVIDER`);.

create table tbl_loginstatus
(
  `G_LOGIN`      BINARY(12)        not null comment 'Login UK, status uses the same GDID as Login entity',

  `PWD_SET_UTC`  DATETIME     comment 'When pwd was set last time',
  `CONFIRM_UTC`  DATETIME     comment 'When account was confirmed for the last time',
  `PROPS`        MEDIUMTEXT   comment 'Optional properties such as confirmation attributes, etc.',

  `OK_UTC`    DATETIME      comment 'Last correct login timestamp or null',
  `OK_ADDR`   VARCHAR(64)   comment 'Last correct login address or null',
  `OK_AGENT`  VARCHAR(256)  comment 'Last correct login user agent or null',

  `BAD_CAUSE`  BIGINT UNSIGNED  comment 'Last bad login cause atom',
  `BAD_UTC`    DATETIME         comment 'Last bad login timestamp or null',
  `BAD_ADDR`   VARCHAR(64)      comment 'Last bad login address or null',
  `BAD_AGENT`  VARCHAR(256)     comment 'Last bad login user agent or null',
  `BAD_COUNT`  INT              comment 'Consecutive incorrect login count',

  constraint `pk_tbl_loginstatus_primary` primary key (`G_LOGIN`),
  constraint `fk_tbl_loginstatus_login` foreign key (`G_LOGIN`) references `tbl_login`(`GDID`)
)
   comment = 'LoginStatus keeps the last known snapshot of log-in volatile information to lessen the replication load Login:LoginStatus = 1:1 (same GDID)'
;.

delimiter ;.
  create index `idx_tbl_loginstatus_badutc` on `tbl_loginstatus`(`BAD_UTC`);.

delimiter ;.
  create index `idx_tbl_loginstatus_badaddr` on `tbl_loginstatus`(`BAD_ADDR`);.
```

### 3. Seed the User/Login Tables ("authkit_usr")

```sql
delimiter ;.

INSERT INTO `tbl_user`
(`GDID`,
`GUID`,
`REALM`,
`NAME`,
`LEVEL`,
`DESCRIPTION`,
`START_UTC`,
`END_UTC`,
`ORG_UNIT`,
`PROPS`,
`RIGHTS`,
`NOTE`,
`CREATE_UTC`,
`CREATE_ORIGIN`,
`CREATE_ACTOR`,
`LOCK_START_UTC`,
`LOCK_END_UTC`,
`LOCK_ACTOR`,
`LOCK_NOTE`,
`VERSION_UTC`,
`VERSION_ORIGIN`,
`VERSION_ACTOR`,
`VERSION_STATE`)
VALUES
(
0x000000000000000000000001,
0xd1433bf1ed3c425ca9ad82a0e9e75512,
6906983, -- REALM hex version
'root', -- NAME
'S', -- LEVEL system
'Root system account for GDI realm', -- Description
'1000-01-01 00:00:00', -- START_UTC
'2100-01-01 00:00:00', -- END_UTC
null, -- ORG_UNIT
'{ p: { } }', -- PROPS
null, -- RIGHTS
'DO NOT REMOVE', -- NOTE
utc_timestamp(), -- CREATE_UTC
7567731,          -- CREATE_ORIGIN - 0x737973 = `sys`
'usrn@idp::root', -- CREATE_ACTOR
null, -- LOCK_START_UTC
null, -- LOCK_END_UTC
null, -- LOCK_ACTOR
null, -- LOCK_NOTE
'1000-01-01 00:00:00', -- VERSION_UTC
7567731,          -- VERSION_ORIGIN - 0x737973 = `sys`
'usrn@idp::root', -- VERSION_ACTOR
'C' -- VERSION_STATE
);.



delimiter ;.

INSERT INTO `tbl_login`
(`GDID`,
`REALM`,
`G_USER`,
`LEVEL_DOWN`,
`ID`,
`TID`,
`PROVIDER`,
`PWD`,
`PROVIDER_DATA`,
`START_UTC`,
`END_UTC`,
`PROPS`,
`RIGHTS`,
`CREATE_UTC`,
`CREATE_ORIGIN`,
`CREATE_ACTOR`,
`LOCK_START_UTC`,
`LOCK_END_UTC`,
`LOCK_ACTOR`,
`LOCK_NOTE`,
`VERSION_UTC`,
`VERSION_ORIGIN`,
`VERSION_ACTOR`,
`VERSION_STATE`)
VALUES
(
0x000000000000000000000001, -- GDID
6906983, -- GDI realm hex version
0x000000000000000000000001, -- G_USER
null, -- LEVEL_DOWN
'root', -- ID
25705, -- "ID" TID
7956003944985229683, -- syslogin
'{"alg":"KDF","fam":"Text","h":"8kinx_bcL0xz9q0viOe0Ro0Gcly7WXJtjT117cplLRw","s":"SjsLUw1-d_7ya-YR12atOm0Tr0Sjezg1XhIRv9dNR9o"}', -- thejake
null, --  PROVIDER_DATA
'1000-01-01 00:00:00', -- START_UTC
'2100-01-01 00:00:00', -- END_UTC
null, -- PROPS
null, -- RIGHTS
utc_timestamp(), -- CREATE_UTC
7567731,          -- CREATE_ORIGIN - 0x737973 = `sys`
'usrn@idp::root', -- CREATE_ACTOR
null, -- LOCK_START_UTC
null, -- LOCK_END_UTC
null, -- LOCK_ACTOR
null, -- LOCK_NOTE
'1000-01-01 00:00:00', -- VERSION_UTC
7567731,          -- VERSION_ORIGIN - 0x737973 = `sys`
'usrn@idp::root', -- VERSION_ACTOR
'C'
);.

```