@echo off
chcp 65001 >nul
setlocal
set "COH3_FOW_EXE=%~dp0CoH3FowTool.exe"
if not exist "%COH3_FOW_EXE%" set "COH3_FOW_EXE=%~dp0publish\CoH3FowTool.exe"
if not exist "%COH3_FOW_EXE%" set "COH3_FOW_EXE=%~dp0bin\Release\net9.0-windows\CoH3FowTool.exe"
if not exist "%COH3_FOW_EXE%" (
  echo [ERROR] Tool not found. Run build.ps1 -Publish first.
  pause
  exit /b 1
)
echo Launching CoH3FowTool with administrator privileges ...
powershell -NoProfile -Command "Start-Process -FilePath $env:COH3_FOW_EXE -Verb RunAs"
endlocal
