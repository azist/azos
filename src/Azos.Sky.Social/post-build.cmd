set SOLUTION_DIR=%1
set CONFIG=%2
set PROJECT_DIR=%~dp0
set TOOL_DIR=%SOLUTION_DIR%\lib\nfx\run-netf\
set OUT_DIR=%SOLUTION_DIR%..\out\%CONFIG%\

copy /Y "%TOOL_DIR%rsc.exe" "%OUT_DIR%"
copy /Y "%TOOL_DIR%NFX.PAL.NetFramework.dll" "%OUT_DIR%"
copy /Y "%TOOL_DIR%NFX.Tools.dll" "%OUT_DIR%"


"%OUT_DIR%rsc" "%PROJECT_DIR%\Graph\Server\Data\Schema\graph-node.rschema" -o out-name-prefix="Graph.Node." domain-search-paths="Azos.Sky.Social.Graph.Server.Data.Schema.*, Azos.Sky.Social"
"%OUT_DIR%rsc" "%PROJECT_DIR%\Graph\Server\Data\Schema\graph-comment.rschema" -o out-name-prefix="Graph.Comment." domain-search-paths="Azos.Sky.Social.Graph.Server.Data.Schema.*, Azos.Sky.Social"
