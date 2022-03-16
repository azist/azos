
/* forest_idp_kit_gdi SEED */

insert into `tbl_node` (`GDID`, `CREATE_UTC`) values (0x000000000000000000000002, utc_timestamp())

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
          0x000000000000000000000002, -- GDID
          utc_timestamp(),  -- VERSION_UTC
          7567731,          -- VERSION_ORIGIN - 0x737973 = `sys`
          'usrn@idp::root', -- VERSION_ACTOR
          'C',-- VERSION_STATE - 'C' = created
          0x000000000000000000000002, -- G_NODE
          0x000000000000000000000001, -- G_PARENT
          'sys', -- PATH_SEGMENT
          '1000-01-01 00:00:00', -- START_UTC
          '{"r": {}}', -- PROPERTIES
          '{"r": {}}'-- CONFIG
        )