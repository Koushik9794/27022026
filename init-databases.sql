-- Initialize all service databases in a single PostgreSQL instance
-- This script runs automatically when the container starts for the first time

CREATE DATABASE admin_service;
CREATE DATABASE catalog_service;
CREATE DATABASE rule_service;
CREATE DATABASE file_service;
CREATE DATABASE configuration_service;
CREATE DATABASE bom_service;

-- Grant all privileges to postgres user
GRANT ALL PRIVILEGES ON DATABASE admin_service TO postgres;
GRANT ALL PRIVILEGES ON DATABASE catalog_service TO postgres;
GRANT ALL PRIVILEGES ON DATABASE rule_service TO postgres;
GRANT ALL PRIVILEGES ON DATABASE file_service TO postgres;
GRANT ALL PRIVILEGES ON DATABASE configuration_service TO postgres;
GRANT ALL PRIVILEGES ON DATABASE bom_service TO postgres;
