@echo off &setlocal
set "search=<Version>1.0.0"
set "replace=<Version>%2"
set "oldfile=%1"
set "newfile=%1.txt"
(for /f "delims=" %%i in (%oldfile%) do (
    set "line=%%i"
	setlocal enabledelayedexpansion
    set "line=!line:%search%=%replace%!"
    echo(!line!
    endlocal
))>"%newfile%"
del %oldfile%
rename %newfile% %~nx1