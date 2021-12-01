@echo off

set selfPah=%~dp0
set currPath=%cd%

rem 先切换到脚本所在的路径
cd /d %selfPah%

REM set UnityPath=D:\UnityAll\Version\Unity2018.3.14\Editor\Unity.exe
REM set ProjectPath=D:\Jenkins\Jenkins\workspace\StarShip_New\Client\Project
REM set PlatForm=0
REM set BaseVer=1.0.2
REM set BundleVer=1
REM set Server=Server

set UnityPath=%1
set ProjectPath=%2
set PlatForm=%3
set BaseVer=%4
set BundleVer=%5
set Server=%6
set ChannelId=%7
set IsReleaseModel=%8
set CpuDefine=%9

shift
set IsUpdateOss=%9
shift
set IsBundle=%9
shift
set OutType=%9

rem 参数设置
set argsFileName=Args.txt
set argsPath=%ProjectPath%\AutoBuild\buildArgs
set args=%PlatForm%-%BaseVer%-%BundleVer%-%Server%-%ChannelId%-%IsReleaseModel%-%CpuDefine%-%IsUpdateOss%-%OutType%
if not exist %argsPath% (
	mkdir %argsPath%
)
echo %args%>%argsPath%\%argsFileName%

rem 日志设置
set logfileDir=%ProjectPath%\AutoBuild\buildLog
set time=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set filename=%time: =0%
if not exist %logfileDir% (
	mkdir %logfileDir%
)

rem 提前删除下Xlua的Warp文件,防止老代码的Warp报错,工程都不能运行
set xLuaGenDir=%ProjectPath%\Assets\XLua\Gen\
if exist %xLuaGenDir% (
	rd /Q /S %xLuaGenDir%
)

REM 设置宏定义
echo Start SetConfig
%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoBuild.SetConfig -logFile %logfileDir%\%filename%_SetConfig.log
REM timeout 30
echo End SetConfig

REM 热更
echo Open XLuaHot
%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoBuild.OpenXluaHot -logFile %logfileDir%\%filename%_OpenXluaHot.log
REM timeout 30
echo End XLuaHot

echo Start ClearXLuaWrap
%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoBuild.ClearXLuaWrap -logFile %logfileDir%\%filename%_ClearXLuaWrap.log
REM timeout 30
echo End ClearXLuaWrap

echo Start BuildXLuaWrap
%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoBuild.BuildXLuaWrap -logFile %logfileDir%\%filename%_BuildXLuaWrap.log
REM timeout 30
echo End BuildXLuaWrap

if %IsBundle%==1 (
	echo Start Build AssetBundle
	REM BuildAssetBundle
	%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoBuild.BuildBundle -logFile %logfileDir%\%filename%_buildbundle.log
	echo Build AssetBundle Finished
)

echo Start Build APK
REM Build APK
%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoBuild.BuildApk -logFile %logfileDir%\%filename%_buildApk.log
REM %1 -projectPath %2 -quit -batchmode -executeMethod APKBuild.Build -logFile build.log
echo End Build APK

echo Start ClearXLuaWrapOnBuildEnd
%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoBuild.ClearXLuaWrap -logFile %logfileDir%\%filename%_ClearXLuaWrapOnBuildEnd.log
REM timeout 30
echo End ClearXLuaWrapOnBuildEnd

if not %IsUpdateOss%==0 (
	echo Start OssUpdate
	%UnityPath% -projectPath %ProjectPath% -quit -batchmode -executeMethod AutoOss.UpAbToOss -logFile %logfileDir%\%filename%_AutoOss.log
	if %IsUpdateOss%==1 (
		python AutoOss/ali/ossUpdate.py %ProjectPath%/AutoBuild/ossConfig
	)else (
		python AutoOss/tengxun/cosUpdate.py %ProjectPath%/AutoBuild/ossConfig
	)

	echo EndOssUpdate
)
if not %errorlevel%==0 ( goto fail ) else ( goto success )

:success
echo Build APK OK
REM Copr Dir
goto end

:fail
echo Build APK Fail
goto end
 
:end

rem 切换回调用脚本时的路径
cd /d %currPath%

pause