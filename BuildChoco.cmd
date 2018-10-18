@echo off
rem ************************************************************
rem
rem Build a Chocolatey package.
rem
rem ************************************************************
setlocal enabledelayedexpansion

:handle_help_request
if /i "%~1" == "-?"     call :usage & exit /b 0
if /i "%~1" == "--help" call :usage & exit /b 0

:build
set workingFolder=%~dp0Chocolatey
pushd "%workingFolder%" 2>nul
if !errorlevel! neq 0 (
	echo ERROR: "%workingFolder%" folder missing
	exit /b 1
)

set nuspecFile=vaultcmd.nuspec
if not exist "%nuspecFile%" (
	echo ERROR: "%nuspecFile%" missing
	popd & exit /b 1
)

if not exist "bin" (
	mkdir "bin"
	if !errorlevel! neq 0 popd & exit /b 1
)

for /f %%f in (PkgList.txt) do (
	if not exist "%%f" (
		echo ERROR: Package artefact "%%f" not found
		popd & exit /b 1
	)
	echo Copying artefact "%%f"
	copy /y "%%f" "bin\." 1>nul
)

set packageFolder=..\Packages
choco pack --out "%packageFolder%"
if %errorlevel% neq 0 popd & exit /b 1

:success
exit /b 0

rem ************************************************************
rem Functions
rem ************************************************************

:usage
echo.
echo Usage: %~n0
goto :eof
