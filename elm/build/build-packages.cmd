@echo on

SET PROJECT_HOME=%AZIST_HOME%
SET LAST=%PROJECT_HOME:~-1%
IF %LAST% NEQ \ (SET PROJECT_HOME=%PROJECT_HOME%\)

set AZOS_HOME=%PROJECT_HOME%AZOS\
set OUT=%AZOS_HOME%out\nuget

set VER=1.0.1

nuget pack Azos.nuspec -Version %VER% -OutputDirectory "%OUT%"

nuget push "%OUT%\Azos.%VER%.nupkg" %AZIST_NUGET_API_KEY% 