echo +++++ [Azos.Wave] Pre Build

set SOLUTION_DIR=%1
set PROJECT_DIR=%2
set CONFIG=%3
set TOOL_DIR=%SOLUTION_DIR%../out/%CONFIG%/

echo +++ Solution  -  %SOLUTION_DIR%
echo +++ Project   -  %PROJECT_DIR%
echo +++ Config    -  %CONFIG%
echo +++ Tool      -  %TOOL_DIR%

rem ---------------------------------------------

dotnet "%TOOL_DIR%buildinfo.dll" > "%PROJECT_DIR%BUILD_INFO.txt"
dotnet "%TOOL_DIR%sky.dll" ntc "%PROJECT_DIR%Templatization\StockContent\*.nht" -sub -r -ext ".auto.cs" -src -c "Azos.Templatization.NHTCompiler, Azos"

rem ---------------------------------------------
echo +++++ [Azos.Wave] Pre Build