@echo off
REM Migration management script for Rule Service

set SERVICE_DIR=Services\rule-service

if "%1"=="create" (
    if "%2"=="" (
        echo Usage: migrate.bat create migration_name
        exit /b 1
    )
    
    REM In a real scenario, would generate timestamp
    echo Creating migration: %2
    echo TODO: Add migration file to src\infrastructure\migrations\
    
) else if "%1"=="up" || "%1"=="migrate" (
    echo Running migrations...
    cd %SERVICE_DIR%
    dotnet run
    cd ..\..
    
) else if "%1"=="down" || "%1"=="rollback" (
    echo Rollback: Update migrations manually
    
) else (
    echo Usage: migrate.bat {create^|up^|migrate^|down^|rollback} [args]
    echo.
    echo Examples:
    echo   migrate.bat create AddNewColumn
    echo   migrate.bat up
    exit /b 1
)
