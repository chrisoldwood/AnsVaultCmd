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

set configuration=Release
set suffix=r

if /i "%1" == "debug" (
set configuration=Debug
set suffix=d
)
if /i "%1" == "release" (
set configuration=Release
set suffix=r
)

:check_built
if not exist "Source\AnsVaultCmd\bin\%configuration%\AnsVaultCmd.exe" (
	echo ERROR: '%configuration%' configuration not built.
	exit /b 1
)

:build
if not exist "Packages" mkdir Packages
if errorlevel 1 exit /b 1

set getVersion=^
Get-ChildItem .\Source\AnsVaultCmd\bin\%configuration%\AnsVaultCmd.exe ^|^
 ForEach { $_.VersionInfo.FileVersion } ^|^
 ForEach { $_ -replace '\.[0-9]+$','' } ^|^
 ForEach { $_ -replace '\.','' }
for /f "usebackq" %%v in (`PowerShell "%getVersion%"`) do set version=%%v

set zipfile=Packages\ansvaultcmd-%version%%suffix%.zip
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
