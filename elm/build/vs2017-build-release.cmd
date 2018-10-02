@echo off

SET PROJECT_HOME=%AZIST_HOME%
SET LAST=%PROJECT_HOME:~-1%
IF %LAST% NEQ \ (SET PROJECT_HOME=%PROJECT_HOME%\)


set AZOS_HOME=%PROJECT_HOME%AZOS\

set DOTNET_FRAMEWORK_DIR=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin

set MSBUILD_EXE="%DOTNET_FRAMEWORK_DIR%\MSBuild.exe"

set BUILD_ARGS=/t:Restore;Rebuild /p:Configuration=Release /p:Platform="Any CPU" /p:DefineConstants="DEBUG;TRACE" /p:METABASE=prod /verbosity:normal /maxcpucount:1

echo Building Release AZOS ---------------------------------------
echo todo: future include nuget pub
%MSBUILD_EXE% "%AZOS_HOME%src\AZOS.sln" %BUILD_ARGS%
if errorlevel 1 goto ERROR

echo Done!
goto :FINISH

:ERROR
 echo Error happened!
:FINISH
 pause