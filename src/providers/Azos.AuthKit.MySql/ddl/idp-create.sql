

-- groups and roles are stored in Config tree, addressed by pass via PROPS
create table tbl_user
(
  `GDID`       BINARY(12)        not null comment 'User Account Immutable UK',
  `GUID`       BINARY(16)        not null comment 'User Account Immutable Guid Network Byte Order, used for Chronicle REL',
  `REALM`      BIGINT UNSIGNED   not null comment 'Realm Atom',

  //name, description etc...
  //where is LOCKED information?
  `LEVEL`      CHAR(1)                    comment 'User level: I|U|A|S',

  `START_UTC`  datetime       not null    comment 'When user privilege takes effect',
  `END_UTC`    datetime       not null    comment 'When user privilege stops',
  `RIGHTS`            MEDIUMTEXT          comment 'Rights override or null',


  `VERSION_UTC`    DATETIME       not null comment 'Version UTC timestamp',
  `VERSION_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this version originated from',
  `VERSION_ACTOR`  varchar(256)   not null comment 'Who made the change',
  `VERSION_STATE`  CHAR(1)        not null comment 'The state of data: Inserted/Updated/Deleted',

  constraint `pk_tbl_user_pk` primary key (`GDID`),
)
    comment = 'Stores user accounts'
;.

delimiter ;.
  create unique index `idx_tbl_user_guid` on `tbl_user`(`GUID`);.


delimiter ;.

create table tbl_login
(
  `GDID`       BINARY(12)        not null comment 'Login UK',
  `REALM`      BIGINT UNSIGNED   not null comment 'Realm Atom',

  `G_USER`     BINARY(12)        not null comment 'FK to USER ACCOUNT table',
  `LEVEL_DOWN` CHAR(1)                    comment 'User level restriction: I|U|A',
  `ID`         VARCHAR(2048)     not null comment 'Login ID, or provider key',
  `TID`        CHAR(1)           not null comment 'Login Type Atom',
  `PROVIDER`   BIGINT UNSIGNED   not null comment 'Login provider, e.g.  AZOS, FBK, TWT, AD, SYSTEM X etc.. or Atom.ZERO for not eternal provider',
  `PWD`        VARCHAR(2048)     not null comment 'Password vector, or empty for providers',
  `PROVIDER_DATA`     MEDIUMTEXT          comment 'Optional extra provider -specific JSON vector',
  `START_UTC`  datetime       not null    comment 'When login privilege takes effect',
  `END_UTC`    datetime       not null    comment 'When login privilege stops',
  `RIGHTS`            MEDIUMTEXT          comment 'Rights override or null',

  `VERSION_UTC`    DATETIME       not null comment 'Version UTC timestamp',
  `VERSION_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this version originated from',
  `VERSION_ACTOR`  varchar(256)   not null comment 'Who made the change',
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

  --pwd change utc
  --pwd confirm data {date, method, history} etc.. used by Policy (loaded from tree)

  -- last incorrect utc
  -- incorrect count
  -- incorrect log {date, agent, address}

  -- last correct utc
  -- correct count
)
   comment = 'LoginStatus keeps Login volatile information to lessen the replication load Login:LoginStatus = 1:1 (same GDID)'
;.