# Azos AuthKit MySQL Bootstrap Setup README

This document contains the current DDL and initial seed requirements for the underlying data store implementation. It details the necessary database create scripts, the inititial table create scripts, and the minimum required seed values need to operate the API.

You will need at minimum 3 databases to run Authkit. For example the below script creates 1 database for Authkit users and login data, 1 database for AuthKit forest data, and one or more AuthKit instance-specific forest databases.

```sql
delimiter ;.

-- Authkit users and login data (for all REALMs)
CREATE DATABASE `authkit_usr`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';.

delimiter ;.

-- AuthKit system forest configuration data
CREATE DATABASE `forest_idp_kit_sky_akit`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';.

delimiter ;.

-- AuthKit instance-specific **REALM** forest configuration data (one or more DBs depending on you deployment needs)
-- Name this database to meet your naming needs (this is for example only, `_gdi` is used as a specific instance name!)
CREATE DATABASE `forest_idp_kit_gdi`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';.

-- Add additional **REALM** databases as needed
```

---

## Create AuthKit Databases


### 1. Create the databases (`authkit_usr`, `forest_idp_kit_sky_akit`, `forest_idp_kit_gdi`, etc.)

Run DDL script from here [akit-db-creates.sql](/src/providers/Azos.AuthKit.Server.MySql/ddl/akit-db-creates.sql)

---

## Create and Seed User/Login tables (`authkit_usr`)

### 2. Create tables 

Run DDL script from here [akit-table-creates.sql](/src/providers/Azos.AuthKit.Server.MySql/ddl/akit-table-creates.sql)

### 3. SEED tables

Run SEED script from here [akit-seed-users-logins.sql](/src/providers/Azos.AuthKit.Server.MySql/bootstrap/akit-seed-users-logins.sql)

---

## Create and Seed AuthKit system forest configuration data tables (`forest_idp_kit_sky_akit`)

### 4. Create tables 

Run DDL script from here [tree-create.sql](/src/providers/Azos.MySql/ConfForest/ddl/tree-create.sql)


### 5. SEED tables

Run SEED script from here [akit-seed-tree-tables.sql](/src/providers/Azos.AuthKit.Server.MySql/bootstrap/akit-seed-tree-tables.sql)

---

## Create and Seed instance-specific **REALM** forest configuration data tables (`forest_idp_kit_gdi`, etc.)

> Name this database to meet your naming needs (this is for example only, `_gdi` is used as a specific instance name!)

### 6. Create tables 

Run DDL script from here [tree-create.sql](/src/providers/Azos.MySql/ConfForest/ddl/tree-create.sql)

### 7. SEED tables

Run SEED script from here [akit-seed-tree-tables.sql](/src/providers/Azos.AuthKit.Server.MySql/bootstrap/akit-seed-tree-tables.sql)

---
