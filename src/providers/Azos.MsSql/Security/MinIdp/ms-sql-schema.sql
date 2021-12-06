
-- ILLUSTRATION PURPOSES ONLY
-- MS SQL MINIDP is not supported at this time

 *  *  *  D E M O  *  *  *

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
