-- Creates the MiCoffee database from scratch.
-- Running it drops the existing local database.

DROP DATABASE IF EXISTS MiCoffee;

CREATE DATABASE MiCoffee
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE MiCoffee;

-- Tables are added here from the data model defined by the DBA.
