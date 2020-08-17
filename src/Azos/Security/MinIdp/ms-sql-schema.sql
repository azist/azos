create table tbl_user
(
);

create table tbl_login
(
);

create table tbl_role
(
);



/*
  -- LOGIN BY ID SQL:

    SELECT
       tu.*, tlg.pwd, trl.right
     FROM
       tbl_user tu join tbl_login tlg on tu.SysId = tlg.SysId && tlg.id = @USER_ID
                   join tbl_role trl on tu.role = trl.id
     WHERE
       (tu.sd < @UTC_NOW) and (tu.ed > @UTC_NOW) and
       (tlg.sd < @UTC_NOW) and (tlg.ed > @UTC_NOW)


  --  LOGIN BY SYS ENT_URI token SQL:

     SELECT
       tu.*, trl.right
     FROM
       tbl_user tu join tbl_role trl on tu.role = trl.id
     WHERE
       (tu.screenname= @URI) and
       (tu.sd < @UTC_NOW) and (tu.ed > @UTC_NOW)



  --  LOGIN BY SYS SYS AUTH token SQL:

     SELECT
       tu.*, trl.right
     FROM
       tbl_user tu join tbl_role trl on tu.role = trl.id
     WHERE
       (tu.SysId= @ID) and
       (tu.sd < @UTC_NOW) and (tu.ed > @UTC_NOW)




  */