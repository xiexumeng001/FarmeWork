
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
shift
set OutType=%9

call ../bat/buildApk.bat %UnityPath% %ProjectPath% %PlatForm% %BaseVer% %BundleVer% %Server% %ChannelId% %IsReleaseModel% %CpuDefine% %IsUpdateOss% 1 %OutType%