

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
  `VERSION_ACTOR`  varchar(256)   not null comment 'Who made the change',
  `VERSION_STATE`  CHAR(1)        not null comment 'The state of data: Inserted/Updated/Deleted',

  constraint `pk_tbl_user_pk` primary key (`GDID`),
)
    comment = 'Stores user accounts'
;.

delimiter ;.
  create unique index `idx_tbl_user_ukguid` on `tbl_user`(`GUID`);.

delimiter ;.
  create index `idx_tbl_user_name` on `tbl_user`(`REALM`, `NAME`);.

delimiter ;.
  create index `idx_tbl_user_org` on `tbl_user`(`REALM`, `ORG_UNIT`);.


delimiter ;.

create table tbl_login
(
  `GDID`       BINARY(12)        not null comment 'Login UK',
  `REALM`      BIGINT UNSIGNED   not null comment 'Realm Atom',

  `G_USER`     BINARY(12)        not null comment 'FK to USER ACCOUNT table',
  `LEVEL_DOWN` CHAR(1)                    comment 'User level restriction: I|U|A',

  `ID`         VARCHAR(2048)     not null comment 'Login ID, or provider key',
  `TID`        BIGINT UNSIGNED   not null comment 'Login Type Atom',
  `PROVIDER`   BIGINT UNSIGNED   not null comment 'Login provider, e.g.  AZOS, FBK, TWT, AD, SYSTEM X etc.. or Atom.ZERO for not eternal provider',
  `PWD`        VARCHAR(2048)              comment 'Password vector, or NULL for providers who dont need it',
  `PROVIDER_DATA`     MEDIUMTEXT          comment 'Optional extra provider -specific JSON vector',

  `START_UTC`  DATETIME       not null    comment 'When login privilege takes effect',
  `END_UTC`    DATETIME       not null    comment 'When login privilege stops',
  `PROPS`      MEDIUMTEXT                 comment 'Properties such as tree connections (e.g. roles) and claims',
  `RIGHTS`     MEDIUMTEXT                 comment 'Rights override or null',

  `LOCK_START_UTC` DATETIME           comment 'Lock timestamp, if set the account is inactive past that timestamp, until LOCK_END_UTC',
  `LOCK_END_UTC`   DATETIME           comment 'If present, resets lock after that point in time',
  `LOCK_ACTOR`     VARCHAR(256)       comment 'Who locked the user account',
  `LOCK_NOTE`      VARCHAR(128)       comment 'Short note explaining lock reason/status',

  `VERSION_UTC`    DATETIME       not null comment 'Version UTC timestamp',
  `VERSION_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this version originated from',
  `VERSION_ACTOR`  VARCHAR(256)   not null comment 'Who made the change',
  `VERSION_STATE`  CHAR(1)        not null comment 'The state of data: Inserted/Updated/Deleted',

  constraint `pk_tbl_login_primary` primary key (`GDID`),
  constraint `fk_tbl_login_user` foreign key (`G_USER`) references `tbl_user`(`GDID`),
)
   comment = 'Stores user account login information'
;.

delimiter ;.
  create unique index `idx_tbl_login_uk1` on `tbl_login`(`REALM`, `ID`, `TID`, `PROVIDER`);.

create table tbl_loginstatus
(
  `GDID`      BINARY(12)        not null comment 'Login UK, status uses the same GDID as Login entity',

  `PWD_DATE_UTC`    DATETIME    not null comment 'When pwd was set last time',
  --pwd confirm data {date, method, history} etc.. used by Policy (loaded from tree)

  `LAST_OK_UTC`    DATETIME      comment 'Last correct login timestamp or null',
  `LAST_OK_ADDR`   VARCHAR(32)   comment 'Last correct login timestamp or null',
  `LAST_OK_AGENT`  VARCHAR(256)  comment 'Last correct login user agent or null',

  `LAST_BAD_UTC`    DATETIME         comment 'Last bad login timestamp or null',
  `LAST_BAD_ADDR`   VARCHAR(32)      comment 'Last bad login timestamp or null',
  `LAST_BAD_AGENT`  VARCHAR(256)     comment 'Last bad login user agent or null',
  `BAD_COUNT`       INT              comment 'Consecutive incorrect login count',
)
   comment = 'LoginStatus keeps Login volatile information to lessen the replication load Login:LoginStatus = 1:1 (same GDID)'
;.