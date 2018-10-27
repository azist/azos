echo +++++ [Azos.Web] Pre Build

set SOLUTION_DIR=%1
set PROJECT_DIR=%2
set CONFIG=%3
set TOOL_DIR=%SOLUTION_DIR%../out/%CONFIG%/run-core/

echo +++ Solution  -  %SOLUTION_DIR%
echo +++ Project   -  %PROJECT_DIR%
echo +++ Config    -  %CONFIG%
echo +++ Tool      -  %TOOL_DIR%

rem ---------------------------------------------

dotnet "%TOOL_DIR%buildinfo.dll" > "%PROJECT_DIR%BUILD_INFO.txt"

rem ---------------------------------------------
echo +++++ [Azos.Web] Pre Build