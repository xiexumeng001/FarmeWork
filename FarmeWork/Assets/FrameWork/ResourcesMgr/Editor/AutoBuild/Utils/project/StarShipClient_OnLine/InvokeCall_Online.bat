rem 启用延迟变量
SETLOCAL ENABLEDELAYEDEXPANSION

REM 本地使用的参数
set UnityPath=%1
set ProjectPath=%2
set PlatForm=%3
set BaseVer=%4
set BundleVer=%5
set Server=%6
set ChannelId=%7
set IsReleaseModel=%8
set CpuDefine=%9

REM 更新oss
shift
set IsUpdateOss=%9

REM 分支合并参数
shift
set BranchInvoke=%9
shift
set ToBranchIsExist=%9
shift
set OutType=%9

set ChannelId=%ChannelId:-=,%
set isBundle=1
set gitbat=D:\JenkinsWorkSpace\gitProject\StarShip\StarShipMergeGit_Only.bat
if %PlatForm%==0 ( set PlatFormName=android) else ( set PlatFormName=ios)
for  %%I in (%ChannelId%) do (

	set cId=%%I

	call %gitbat% %BranchInvoke% %ToBranchIsExist% %PlatForm% %BaseVer% %BundleVer% !cId!

	rem 更新到分支
	git reset --hard
	set onlinebranchName=OnLine/%PlatFormName%/!cId!/%BaseVer%/%BundleVer%
	git fetch
	git checkout -b !onlinebranchName! origin/!onlinebranchName!
	git checkout !onlinebranchName!
	git pull
	echo checkout to !onlinebranchName!

	call ../bat/buildApk.bat %UnityPath% %ProjectPath% %PlatForm% %BaseVer% %BundleVer% %Server% !cId! %IsReleaseModel% %CpuDefine% !IsUpdateOss! !isBundle! %OutType%

	rem 只有第一次打AB包与更新oss
	set isBundle=0
	set IsUpdateOss=0
)

rem git reset --hard
rem call ../bat/buildApk.bat %UnityPath% %ProjectPath% %PlatForm% %BaseVer% %BundleVer% %Server% %ChannelId% %IsReleaseModel% %CpuDefine% %IsUpdateOss% 1