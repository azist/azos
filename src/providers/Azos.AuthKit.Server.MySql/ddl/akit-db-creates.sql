
-- Authkit users and login data (for all REALMs)
CREATE DATABASE `authkit_usr`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';


-- AuthKit system forest configuration data
CREATE DATABASE `forest_idp_kit_sky_akit`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';


-- AuthKit instance-specific **REALM** forest configuration data (one or more DBs depending on you deployment needs)
CREATE DATABASE `forest_idp_kit_gdi`
CHARACTER SET 'utf8mb4'
COLLATE 'utf8mb4_unicode_ci';

-- Add additional **REALM** databases as needed