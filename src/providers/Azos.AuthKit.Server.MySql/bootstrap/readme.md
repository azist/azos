# Azos AuthKit MySQL Bootstrap Setup README

This document contains the current DDL and initial seed requirements for the underlying data store implementation. It details the necessary database create scripts, the inititial table create scripts, and the minimum required seed values need to operate the API.

You will need at minimum 3 databases to run Authkit. For example the below script creates 1 database for AuthKit forest data, 1 for Authkit users and login data, and one or more AuthKit instance-specific forest data.

```sql

-- AuthKit system forest configuration data

-- Authkit users and login data (for all REALMs)
CREATE DATABASE `authkit_usr`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';


CREATE DATABASE `forest_idp_kit_sky_akit`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';


-- AuthKit instance-specific **REALM** forest configuration data (one or more DBs depending on you deployment needs)
CREATE DATABASE `forest_idp_kit_gdi`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';

```

## I. AuthKit Databases

### 1. Create the User/Login database ("authkit_usr")

Run DDL script from here [idp-base-create.sql](/src/providers/Azos.MySql/ConfForest/ddl/idp-base-create.sql)

### 2. Create AuthKit system forest configuration data database ("forest_idp_kit_sky_akit")

Run DDL script from here [tree-create.sql](/src/providers/Azos.MySql/ConfForest/ddl/tree-create.sql)

### 3. Create AuthKit instance-specific **REALM** forest configuration databases (e.g. "forest_idp_kit_gdi")

Run DDL script from here [tree-create.sql](/src/providers/Azos.MySql/ConfForest/ddl/tree-create.sql)


## II. Table Create Steps

### 1. Create User/Login tables ("authkit_usr")

Run DDL script from here [akit-create-idp-tables.sql](/src/providers/Azos.AuthKit.Server.MySql/bootstrap/akit-create-idp-tables.sql)


### 2. 

