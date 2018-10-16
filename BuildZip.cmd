@echo off
rem ************************************************************
rem
rem Script to create a ZIP deployment package.
rem
rem ************************************************************
setlocal enabledelayedexpansion

:handle_help_request
if /i "%~1" == "-?"     call :usage & exit /b 0
if /i "%~1" == "--help" call :usage & exit /b 0

:check_args
if /i "%~1" == "" call :usage & exit /b 1

set configuration=Release

if /i "%1" == "debug" set configuration=Debug
if /i "%1" == "release" set configuration=Release

:build
if not exist "Packages" mkdir Packages
if errorlevel 1 exit /b 1

set zipfile=Packages\VaultCmd-%configuration%.zip
set filelist=PkgList.%configuration%.txt

if exist "%zipfile%" del "%zipfile%"
if errorlevel 1 popd & exit /b 1

7za a -tzip -bd %zipfile% @%filelist%
if errorlevel 1 popd & exit /b 1

:success
exit /b 0

:usage
echo.
echo Usage: %~n0 [debug ^| release]
goto :eof
