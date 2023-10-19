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

where /q 7z
if !errorlevel! neq 0 goto :7zip_missing

:build
if not exist "Packages" mkdir Packages
if errorlevel 1 exit /b 1

set getVersion=^
Get-ChildItem .\Source\AnsVaultCmd\bin\%configuration%\AnsVaultCmd.exe ^|^
 ForEach { $_.VersionInfo.FileVersion } ^|^
 ForEach { $_ -replace '\.[0-9]+$','' } ^|^
 ForEach { $_ -replace '\.','' }
for /f "usebackq" %%v in (`PowerShell "%getVersion%"`) do set version=%%v

set zipdir=Packages
set zipfile=%zipdir%\ansvaultcmd-%version%%suffix%.zip
set pkglist=PkgList.txt
set filelist=%zipdir%\FileList.txt

if exist "%filelist%" del "%filelist%" || exit /b 1
if exist "%zipfile%" del "%zipfile%" || exit /b 1

for /f %%l in (%pkglist%) do (
    set "file=%%l"
    set "file=!file:${BUILD}=%configuration%!"
    set "file=!file:${PLATFORM}=%platform%!"
    if not exist "!file!" (
        echo ERROR: Package file missing: "!file!"
        exit /b 1
    )
    echo !file!>>"%filelist%" || exit /b
)

7z a -tzip -bd %zipfile% @%filelist% || exit /b 1

:success
exit /b 0

:usage
echo.
echo Usage: %~n0 [debug ^| release]
goto :eof

:7zip_missing
echo ERROR: 7z not installed or on the PATH.
echo You can install it with 'choco install -y 7zip'.
exit /b !errorlevel!
