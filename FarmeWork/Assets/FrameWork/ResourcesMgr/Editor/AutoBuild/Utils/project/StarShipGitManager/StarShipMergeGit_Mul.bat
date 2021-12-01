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

set ChannelId=%ChannelId:-=,%
for %%i in (%ChannelId%) do (
	call StarShipMergeGit_Only.bat %BranchInvoke% %ToBranchIsExist% %PlatForm% %BaseVer% %BundleVer% %%i
)

rem 切换回调用脚本时的路径
cd /d %currPath%