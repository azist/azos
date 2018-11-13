set SOLUTION_DIR=%1
set CONFIG=%2
set PROJECT_DIR=%~dp0
set TOOL_DIR=%SOLUTION_DIR%\lib\nfx\run-netf\


"%TOOL_DIR%buildinfo" > "%PROJECT_DIR%BUILD_INFO.txt"
