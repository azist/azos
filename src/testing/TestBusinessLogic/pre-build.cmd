set SOLUTION_DIR=%1
set CONFIG=%2
set PROJECT_DIR=%~dp0
set TOOL_DIR=%SOLUTION_DIR%\..\out\%CONFIG%\run-netf\

"%TOOL_DIR%ntc" "%PROJECT_DIR%WaveTestSite\Pages\*.htm" -sub -r -ext ".auto.cs" -src -c "Azos.Templatization.NHTCompiler, Azos"
