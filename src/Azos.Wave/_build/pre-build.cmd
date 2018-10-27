echo +++++ [Azos.Wave] Pre Build

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
dotnet "%TOOL_DIR%ntc.dll" "%PROJECT_DIR%Templatization\StockContent\*.htm" -sub -r -ext ".auto.cs" -src -c "Azos.Templatization.NHTCompiler, Azos"
dotnet "%TOOL_DIR%ntc.dll" "%PROJECT_DIR%Templatization\StockContent\Embedded\script\ljs\*.ntc.js" -ext ".js" -replace ".ntc.js" -dest "%PROJECT_DIR%Templatization\StockContent\Embedded\script" -src -c "Azos.Templatization.TextJSTemplateCompiler, Azos" -o dom-gen="cmp{pretty=1}"

rem ---------------------------------------------
echo +++++ [Azos.Wave] Pre Build
