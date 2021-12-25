@echo off
setlocal


set M9=C:\Program Files\Manifold\v9.0\shared

set ADDINNAME=DisplaceGeom

if exist "%M9%\%ADDINNAME%\" GOTO PROMPT
GOTO NOTHINGTOUNINSTALL

:PROMPT
SET /P AREYOUSURE=Are you sure you want to delete %ADDINNAME% ([Y]/N)?
IF /I "%AREYOUSURE%" NEQ "N" GOTO DOUNINST
GOTO DONOTWANT


:DOUNINST
cd..
del "%M9%\%ADDINNAME%\%ADDINNAME%.dll"
if exist "%M9%\%ADDINNAME%\%ADDINNAME%.dll" GOTO NOLUCK
echo %ADDINNAME%.dll deleted. 
rmdir /S /Q "%M9%\%ADDINNAME%\"
if exist "%M9%\%ADDINNAME%\" GOTO NOLUCK
goto END

:NOLUCK
echo Error: Cannot delete %ADDINNAME%.dll. Perhaps you have Manifold running.
echo %M9%\%ADDINNAME%\
GOTO END


:DONOTWANT
echo Uninstall skipped.
GOTO END

:NOTHINGTOUNINSTALL
echo Nothing to uninstall
echo %ADDINNAME% directory does not exist
GOTO END

:END
endlocal
pause
