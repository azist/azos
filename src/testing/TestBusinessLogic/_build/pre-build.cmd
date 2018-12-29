echo +++++ [TestBusinessLogic] Pre Build

set SOLUTION_DIR=%1
set PROJECT_DIR=%2
set CONFIG=%3
set TOOL_DIR=%SOLUTION_DIR%../out/%CONFIG%/run-core/

echo +++ Solution  -  %SOLUTION_DIR%
echo +++ Project   -  %PROJECT_DIR%
echo +++ Config    -  %CONFIG%
echo +++ Tool      -  %TOOL_DIR%

rem ---------------------------------------------

dotnet "%TOOL_DIR%ntc.dll" "%PROJECT_DIR%WaveTestSite\Pages\*.htm" -sub -r -ext ".auto.cs" -src -c "Azos.Templatization.NHTCompiler, Azos"

rem ---------------------------------------------
echo +++++ [TestBusinessLogic] Pre Build