@echo off
setlocal

set "ROOT=%~dp0"
set "HOST=127.0.0.1"
set "PORT=5005"

if exist "%ROOT%AIBackend.exe" (
    "%ROOT%AIBackend.exe"
    exit /b %ERRORLEVEL%
)

if exist "%ROOT%python\python.exe" (
    "%ROOT%python\python.exe" "%ROOT%server.py" --server --host %HOST% --port %PORT%
    exit /b %ERRORLEVEL%
)

where py >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    py -3 "%ROOT%server.py" --server --host %HOST% --port %PORT%
    exit /b %ERRORLEVEL%
)

where python >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    python "%ROOT%server.py" --server --host %HOST% --port %PORT%
    exit /b %ERRORLEVEL%
)

echo AI backend could not start. No AIBackend.exe, embedded python, py launcher, or python.exe was found.
exit /b 1
