@echo off
cd /d "%~dp0"
DVVulkan.Worker.exe "%~dp0..\.." 0 0
type deployment.log
pause
