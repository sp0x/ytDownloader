@echo off
SET PATH=%~dp0;%PATH%
"%~dp0node" "%~dp0..\..\..\..\asp.net\musicowl\packages\NoGit.0.0.8\node_modules\nogit\bin\git.js" %*
