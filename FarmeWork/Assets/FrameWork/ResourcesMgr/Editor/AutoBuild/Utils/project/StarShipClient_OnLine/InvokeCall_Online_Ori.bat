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

set gitbat=D:\JenkinsWorkSpace\gitProject\StarShip\StarShipMergeGit.bat
call %gitbat% %BranchInvoke% %ToBranchIsExist% %PlatForm% %BaseVer% %BundleVer% %ChannelId%

rem 丢弃修改
git reset --hard
rem 切换分支
if %PlatForm%==0 ( set PlatFormName=android) else ( set PlatFormName=ios)
set onlinebranchName=OnLine/%PlatFormName%/%ChannelId%/%BaseVer%.%BundleVer%
git fetch
git checkout -b %onlinebranchName% origin %onlinebranchName%
git checkout %onlinebranchName%
rem 拉取
git pull

call ../bat/buildApk.bat %UnityPath% %ProjectPath% %PlatForm% %BaseVer% %BundleVer% %Server% %ChannelId% %IsReleaseModel% %CpuDefine% %IsUpdateOss%