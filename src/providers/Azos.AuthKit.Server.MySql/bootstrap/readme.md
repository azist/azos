# Azos AuthKit MySQL Bootstrap Setup README

This document contains the current DDL and initial seed requirements for the underlying data store implementation. It details the necessary database create scripts, the inititial table create scripts, and the minimum required seed values need to operate the API.

You will need at minimum 3 databases to run Authkit. For example the below script creates 1 database for AuthKit forest data, 1 for Authkit users and login data, and one or more AuthKit instance-specific forest data.

```sql

-- AuthKit system forest configuration data

CREATE DATABASE `forest_idp_kit_sky_akit`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';


-- Authkit users and login data (for all REALMs)
CREATE DATABASE `authkit_usr`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';


-- AuthKit instance-specific **REALM** forest configuration data (one or more DBs depending on you deployment needs)
CREATE DATABASE `forest_idp_kit_gdi`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';

```

## I. User/Login Database and Table Create Steps

Run DDL script from here [idp-base-create.sql](../ddl/idp-base-create.sql)

[this](/src/providers/Azos.MySql/ConfForest/ddl/tree-create.sql)

### 1. Create the User/Login Database ("authkit_usr")



### 2. Create the User/Login Tables ("authkit_usr")



### 3. Seed the User/Login Tables ("authkit_usr")




## II. User/Login Database and Table Create Steps