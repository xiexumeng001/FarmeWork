
set selfPah=%~dp0
set currPath=%cd%

rem 先切换到脚本所在的路径
cd /d %selfPah%

set BranchInvoke=%1
set ToBranchIsExist=%2
set PlatForm=%3
set BaseVer=%4
set BundleVer=%5
set ChannelId=%6

if %PlatForm%==0 ( set PlatFormName=android) else ( set PlatFormName=ios)

rem 命令行路径
rem set mergeBatPath=../../bat/git/MergeBranch.bat
set mergeBatPath=D:\JenkinsWorkSpace\bat\git\MergeBranch.bat

rem 工程路径
set clientPath=%selfPah%\StarShipClient
set gameServerPath=%selfPah%\StarShipGameServer
set battleServerPath=%selfPah%\StarShipBattleServer
set docPath=%selfPah%\StarShipDoc

rem 分支名称
set masterName=master
rem master_channel分支,用来平常打渠道包
set master_channel=master_channel/%PlatFormName%/%ChannelId%
rem release_channel分支,用来对release分支打渠道包
set release_channel=master_channel/%PlatFormName%/%ChannelId%
rem relase分支,封包时用的
set releaseName=release
rem 渠道分支,各渠道分支记录各渠道sdk包等
set channelbranchName=Channel/%PlatFormName%/%ChannelId%
rem 正式上线分支
set onlinebranchName=OnLine/%PlatFormName%/%ChannelId%/%BaseVer%/%BundleVer%

if %BranchInvoke%==1 (
	rem 合并master 与 channel分支 至 master_channel分支

	cd /d %clientPath%
	call %mergeBatPath% %channelbranchName% %master_channel% %ToBranchIsExist%
	call %mergeBatPath% %masterName% %master_channel% 1
)else if %BranchInvoke%==2 (
	rem 合并release 与 channel分支 至 release_channel分支

	cd /d %clientPath%
	call %mergeBatPath% %channelbranchName% %release_channel% %ToBranchIsExist%
	call %mergeBatPath% %releaseName% %release_channel% 1
) else if %BranchInvoke%==3 (
	rem 从 master 更新到 release

	cd /d %clientPath%
	call %mergeBatPath% %masterName% %releaseName% %ToBranchIsExist%

	cd /d %gameServerPath%
	call %mergeBatPath% %masterName% %releaseName% %ToBranchIsExist%

	cd /d %battleServerPath%
	call %mergeBatPath% %masterName% %releaseName% %ToBranchIsExist%

	cd /d %docPath%
	call %mergeBatPath% %masterName% %releaseName% %ToBranchIsExist%
) else if %BranchInvoke%==4 (
	rem 合并release与channel分支到OnLine分支

	cd /d %clientPath%
	call %mergeBatPath% %channelbranchName% %onlinebranchName% %ToBranchIsExist%
	call %mergeBatPath% %releaseName% %onlinebranchName% 1

	cd /d %gameServerPath%
	call %mergeBatPath% %releaseName% %onlinebranchName% %ToBranchIsExist%

	cd /d %battleServerPath%
	call %mergeBatPath% %releaseName% %onlinebranchName% %ToBranchIsExist%

	cd /d %docPath%
	call %mergeBatPath% %releaseName% %onlinebranchName% %ToBranchIsExist%
) else if %BranchInvoke%==5 (
	rem release合并到master

	cd /d %clientPath%
	call %mergeBatPath% %releaseName% %masterName% 1

	cd /d %gameServerPath%
	call %mergeBatPath% %releaseName% %masterName% 1

	cd /d %battleServerPath%
	call %mergeBatPath% %releaseName% %masterName% 1

	cd /d %docPath%
	call %mergeBatPath% %releaseName% %masterName% 1
)


if not %errorlevel%==0 ( goto fail ) else ( goto success )

:fail
echo fail
goto end

:success
echo success
goto end

:end


rem 切换回调用脚本时的路径
cd /d %currPath%

pause