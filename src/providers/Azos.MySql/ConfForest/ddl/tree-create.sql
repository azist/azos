delimiter ;.
-- Table tbl_tnode
create table `tbl_node`
(
 `GDID`           BINARY(12)     not null comment 'Global Distributed Id - Immutable Primary Key aka G_NODE',
 `CREATE_UTC`     DATETIME       not null comment 'Creation UTC timestamp',
  constraint `pk_tbl_node_primary` primary key (`GDID`)
)
    comment = 'Forest tree node master'
;.

delimiter ;.
-- Table tbl_nodelog
create table `tbl_nodelog`
(
 `GDID`           BINARY(12)     not null comment 'Global Distributed Id - Immutable Primary Key aka G_VERSION',
 `VERSION_UTC`    DATETIME       not null comment 'Version UTC timestamp',
 `VERSION_ORIGIN` BIGINT UNSIGNED  not null comment 'Cloud partition where this version originated from',
 `VERSION_ACTOR`  varchar(256)   not null comment 'Who made the change',
 `VERSION_STATE`  CHAR(1)        not null comment 'The state of data: Inserted/Updated/Deleted',
 `G_NODE`         BINARY(12)     not null comment 'Node master reference',
 `G_PARENT`       BINARY(12)     not null comment 'Parent Node reference, for very root node it is `0:0:1`',
 `PATH_SEGMENT`   varchar(64)    not null comment 'Path Segment see Constaints.MAX_LEN = 64',
 `START_UTC`      DATETIME       not null comment 'As of which point in time the node takes effect',
 `PROPERTIES`     MEDIUMTEXT     not null comment 'Property configuration data',
 `CONFIG`         MEDIUMTEXT     not null comment 'Tree node configuration which gets applied down the tree hierarchy path',
  constraint `pk_tbl_nodelog_primary` primary key (`GDID`),
  constraint `fk_tbl_nodelog_node` foreign key (`G_NODE`) references `tbl_node`(`GDID`),
  constraint `fk_tbl_nodelog_parent` foreign key (`G_PARENT`) references `tbl_node`(`GDID`)
)
    comment = 'Forest tree node version log'
;.

delimiter ;.
  create  index `idx_tbl_nodelog_gnsd` on `tbl_nodelog`(`G_NODE`, `START_UTC`);.
delimiter ;.
  create  index `idx_tbl_nodelog_gpsd` on `tbl_nodelog`(`G_PARENT`, `PATH_SEGMENT`, `START_UTC`);.

-- Create tree root node
delimiter ;.
insert into `tbl_node` (`GDID`, `CREATE_UTC`)
values (0x000000000000000000000001, utc_timestamp());.

delimiter ;.
insert into `tbl_nodelog`
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
  0x000000000000000000000001, -- GDID
  utc_timestamp(),  -- VERSION_UTC
  7567731,          -- VERSION_ORIGIN - 0x737973 = `sys`
  'usrn@idp::root', -- VERSION_ACTOR
  'C',-- VERSION_STATE - 'C' = created
  0x000000000000000000000001, -- G_NODE
  0x000000000000000000000001, -- G_PARENT
  '/', -- PATH_SEGMENT
  '1000-01-01 00:00:00', -- START_UTC
  '{"r": {}}', -- PROPERTIES
  '{"r": {}}'  -- CONFIG
);.