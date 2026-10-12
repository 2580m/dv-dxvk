@echo off
cd /d "%~dp0"
echo disable>pending.txt
DVVulkan.Worker.exe "%~dp0..\.." 0 0
type deployment.log
pause
