CREATE TABLE [MIDP_ROLE]
(
  [ID]      nvarchar(25)    NOT NULL,
  [REALM]   bigint          NOT NULL,
  [DESCR]   nvarchar(96)    NOT NULL,
  [RIGHTS]  nvarchar(MAX)   NOT NULL,
  [SUTC]    datetime        NOT NULL,
  [EUTC]    datetime        NOT NULL,
  [NOTE]    nvarchar(4000),
  constraint PK_MIDP_ROLE primary key clustered ([ID], [REALM]),
  constraint CHK_MIDP_ROLE_STARTEND check (SUTC < EUTC)
);

CREATE TABLE [MIDP_USER]
(
  [SYSID]   bigint         NOT NULL identity(5000, 1) constraint PK_MINIDP_USER primary key clustered,
  [REALM]   bigint         NOT NULL,
  [ROLE]    nvarchar(25)   NOT NULL,
  [STAT]    tinyint        NOT NULL,
  [CUTC]    datetime       NOT NULL,
  [SUTC]    datetime       NOT NULL,
  [EUTC]    datetime       NOT NULL,
  [NAME]    nvarchar(36)   NOT NULL,
  [DESCR]   nvarchar(96)   NOT NULL,
  [SNAME]   nvarchar(36)   NOT NULL,
  [NOTE]    nvarchar(4000),
  constraint FK_MIDP_USER_RL foreign key ([ROLE], [REALM]) references [MIDP_ROLE]([ID], [REALM]),
  constraint CHK_MIDP_USER_CREATE check (CUTC <= SUTC),
  constraint CHK_MIDP_USER_STARTEND check (SUTC < EUTC)
);

create unique index [IDX_MIDP_USER_UKSN] on [MIDP_USER]([SNAME], [REALM]);


CREATE TABLE [MIDP_LOGIN]
(
  [ID]      nvarchar(36)    NOT NULL,
  [REALM]   bigint          NOT NULL,
  [SYSID]   bigint          NOT NULL constraint FK_MINIDP_LOGIN_USR foreign key references [MIDP_USER]([SYSID]),
  [PWD]     nvarchar(2048)  NOT NULL,
  [SUTC]    datetime        NOT NULL,
  [EUTC]    datetime        NOT NULL,
  constraint PK_MINIDP_LOGIN primary key clustered ([ID], [REALM]),
  constraint CHK_MIDP_LOGIN_STARTEND check (SUTC < EUTC)
);

create unique index [IDX_MIDP_LOGIN_UK] on [MIDP_LOGIN]([SYSID], [ID]);









/*
  -- LOGIN BY ID SQL:

    SELECT
       tu.*, tlg.pwd, trl.right
     FROM
       tbl_user tu join tbl_login tlg on tu.SysId = tlg.SysId && tlg.id = @USER_ID
                   join tbl_role trl on tu.role = trl.id
     WHERE
       (tu.sd < @UTC_NOW) and (tu.ed > @UTC_NOW) and
       (tlg.sd < @UTC_NOW) and (tlg.ed > @UTC_NOW) and
       (trl.sd < @UTC_NOW) and (trl.ed > @UTC_NOW)


  --  LOGIN BY SYS ENT_URI token SQL:

     SELECT
       tu.*, trl.right
     FROM
       tbl_user tu join tbl_role trl on tu.role = trl.id
     WHERE
       (tu.screenname= @URI) and
       (tu.sd < @UTC_NOW) and (tu.ed > @UTC_NOW) and
       (trl.sd < @UTC_NOW) and (trl.ed > @UTC_NOW)


  --  LOGIN BY SYS SYS AUTH token SQL:

     SELECT
       tu.*, trl.right
     FROM
       tbl_user tu join tbl_role trl on tu.role = trl.id
     WHERE
       (tu.SysId= @ID) and
       (tu.sd < @UTC_NOW) and (tu.ed > @UTC_NOW) and
       (trl.sd < @UTC_NOW) and (trl.ed > @UTC_NOW)




  */