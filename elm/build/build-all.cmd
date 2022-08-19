@echo off

set PROJECT_HOME=%AZIST_HOME%
set LAST=%PROJECT_HOME:~-1%
if %LAST% NEQ \ (SET PROJECT_HOME=%PROJECT_HOME%\)


set AZOS_HOME=%PROJECT_HOME%azos-net6\


set BUILD_ARGS=-c %1
set BUILD_ARGS_WIN=%BUILD_ARGS% -r win-x64
set BUILD_ARGS_LIN=%BUILD_ARGS% -r linux-x64

rem https://andrewlock.net/version-vs-versionsuffix-vs-packageversion-what-do-they-all-mean/
if "%~2" NEQ "" (SET BUILD_ARGS=%BUILD_ARGS% /p:Version=%2)

echo Building Azos ---------------------------------------

dotnet build "%AZOS_HOME%src\AZOS.sln" %BUILD_ARGS%
if errorlevel 1 goto ERROR
dotnet build "%AZOS_HOME%src\runtimes\core\buildinfo\buildinfo.csproj" %BUILD_ARGS_WIN%
if errorlevel 1 goto ERROR
dotnet build "%AZOS_HOME%src\runtimes\core\buildinfo\buildinfo.csproj" %BUILD_ARGS_LIN%
if errorlevel 1 goto ERROR
dotnet build "%AZOS_HOME%src\runtimes\core\sky\sky.csproj" %BUILD_ARGS_WIN% 
if errorlevel 1 goto ERROR
dotnet build "%AZOS_HOME%src\runtimes\core\sky\sky.csproj" %BUILD_ARGS_LIN%
if errorlevel 1 goto ERROR

echo DONE: Success!
goto :FINISH

:ERROR
 echo Error happened!
:FINISH
