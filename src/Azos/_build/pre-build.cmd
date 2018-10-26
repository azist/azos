set SOLUTION_DIR=%1
set PROJECT_DIR=%2
set CONFIG=%3
set TOOL_DIR=%SOLUTION_DIR%\..\out\%CONFIG%\run-netf\

"%TOOL_DIR%buildinfo" > "%PROJECT_DIR%BUILD_INFO.txt"
