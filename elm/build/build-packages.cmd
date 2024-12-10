@echo on

set VER=3.10.0.7
set HEADLINE=Evaluator unit tests and svc update
call build-all Release %VER%

if errorlevel 1 goto BUILD_ERROR

set PROJECT_HOME=%AZIST_HOME%
SET LAST=%PROJECT_HOME:~-1%
set %LAST% NEQ \ (SET PROJECT_HOME=%PROJECT_HOME%\)

set AZOS_HOME=%PROJECT_HOME%azos\
set OUT=%AZOS_HOME%out\nuget

set ICON=https://raw.githubusercontent.com/azist/azos/master/elm/design/logo/azos-logo-320x320.png


set WARNING=-NoPackageAnalysis

nuget pack Azos.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.Sky.nuspec -Version %VER% %WARNING%  -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.Sky.Server.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.Wave.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.MongoDb.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.Sky.Server.MongoDb.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.MySql.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.MsSql.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.Oracle.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.Media.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.WinForms.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.AuthKit.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.AuthKit.Server.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"
nuget pack Azos.AuthKit.Server.MySql.nuspec -Version %VER% %WARNING% -OutputDirectory "%OUT%" -Properties icon="%ICON%";headline="%HEADLINE%"

rem goto :FINISH
rem --------------------------

nuget push "%OUT%\Azos.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.Sky.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.Sky.Server.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.Wave.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.MongoDb.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.Sky.Server.MongoDb.%VER%.nupkg" %AZIST_NUGET_API_KEY%
nuget push "%OUT%\Azos.MySql.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.MsSql.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.Oracle.%VER%.nupkg" %AZIST_NUGET_API_KEY%
nuget push "%OUT%\Azos.Media.%VER%.nupkg" %AZIST_NUGET_API_KEY% 
nuget push "%OUT%\Azos.WinForms.%VER%.nupkg" %AZIST_NUGET_API_KEY%
nuget push "%OUT%\Azos.AuthKit.%VER%.nupkg" %AZIST_NUGET_API_KEY%
nuget push "%OUT%\Azos.AuthKit.Server.%VER%.nupkg" %AZIST_NUGET_API_KEY%
nuget push "%OUT%\Azos.AuthKit.Server.MySql.%VER%.nupkg" %AZIST_NUGET_API_KEY%
 
 echo Nuget Published!
 goto :FINISH
 
:BUILD_ERROR
 echo Build Error happened and packaging was aborted!!!!!!!!!!!!!!!!!!!!
:FINISH