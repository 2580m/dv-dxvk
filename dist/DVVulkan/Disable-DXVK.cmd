@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo disable>pending.txt
DVVulkan.Worker.exe "%~dp0..\.." 0 0
type deployment.log
pause
