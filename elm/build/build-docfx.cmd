@echo off

set PROJECT_HOME=%AZIST_HOME%\
set LAST=%PROJECT_HOME:~-1%
if %LAST% NEQ \ (SET PROJECT_HOME=%PROJECT_HOME%\)

set DOCFX_PATH=%DOCFX_PATH%\
set LAST=%DOCFX_PATH:~-1%
if %LAST% NEQ \ (SET DOCFX_PATH=%DOCFX_PATH%\)


set AZOS_HOME=%PROJECT_HOME%AZOS\
set AZOS_DOCS_PROJ=%AZOS_HOME%elm\help\azoslib\
set AZOS_DOCS_OUTPUT=%AZOS_HOME%out\help\

set DOCFX_EXE="%DOCFX_PATH%docfx.exe"

echo Removing previously generated YAML files ---------------------
for %%i in (%AZOS_DOCS_PROJ%docs\*.yml) do if not "%%~nxi"=="toc.yml" del "%%i"

echo Building AZOS Library/API Documentation ---------------------
%DOCFX_EXE% %AZOS_DOCS_PROJ%docfx.json -o %AZOS_DOCS_OUTPUT%

echo Removing generated YAML files ---------------------
for %%i in (%AZOS_DOCS_PROJ%docs\*.yml) do if not "%%~nxi"=="toc.yml" del "%%i"

if errorlevel 1 goto ERROR

echo Done Building AZOS Documentation!
goto :FINISH

:ERROR
 echo Error happened!
:FINISH
 pause




