@echo off
rem This script should clean up what's in the .gitignore file.
rem Note: use "git status -s --ignored" to see what's been missed.
setlocal

if /i "%1" == "--help" call :usage & exit /b 0
if /i "%1" == "/?"     call :usage & exit /b 0

:shallow_clean
rmdir /s /q Packages 1>nul 2>nul
for /r %%d in (obj) do if exist "%%d" rmdir /s /q "%%d"
for /r %%d in (bin) do if exist "%%d" rmdir /s /q "%%d"
del /s *.sln.cache 1>nul 2>nul

:deep_clean
if /i "%1" == "--all" (
    del /s  *.user 1>nul 2>nul
    del /s /ah *.suo 1>nul 2>nul
)

:success
exit /b 0

:usage
echo.
echo Usage: %~n0 [--all]
goto :EOF
