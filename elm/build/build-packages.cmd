@echo on

call vs2017-build-release

if errorlevel 1 goto BUILD_ERROR

set PROJECT_HOME=%AZIST_HOME%
SET LAST=%PROJECT_HOME:~-1%
set %LAST% NEQ \ (SET PROJECT_HOME=%PROJECT_HOME%\)

set AZOS_HOME=%PROJECT_HOME%AZOS\
set OUT=%AZOS_HOME%out\nuget

set ICON=https://raw.githubusercontent.com/azist/azos/master/elm/design/logo/azos-logo-320x320.png

set VER=1.0.210

nuget pack Azos.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.Web.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.Wave.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.Media.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.MongoDb.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.MySql.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.MsSql.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.Oracle.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.Sky.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.Sky.MongoDb.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"
nuget pack Azos.WinForms.nuspec -Version %VER% -OutputDirectory "%OUT%" -Properties icon="%ICON%"


 nuget push "%OUT%\Azos.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.Web.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.Wave.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.Media.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.MongoDb.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.MySql.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.MsSql.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.Oracle.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.Sky.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
 nuget push "%OUT%\Azos.Sky.MongoDb.%VER%.nupkg" %AZIST_NUGET_API_KEY%
 nuget push "%OUT%\Azos.WinForms.%VER%.nupkg" %AZIST_NUGET_API_KEY%
 
 echo Nuget Published!
 goto :FINISH
 
:BUILD_ERROR
 echo Build Error happened and packaging was aborted!!!!!!!!!!!!!!!!!!!!
:FINISH