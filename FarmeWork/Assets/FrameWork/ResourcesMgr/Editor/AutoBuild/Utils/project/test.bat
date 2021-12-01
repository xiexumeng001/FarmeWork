rem git config -l

rem git config --global user.name "jenkins"
rem git config --global user.email "jenkins@jenkins.com"

rem git config -l

rem git checkout master
rem git pull

rem git checkout release
rem git pull

rem git merge master --no-edit
rem git push

@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

rem set ChannelId="aa_bb_cc"
rem set channelId=aa
rem echo channelid %channelid%
rem echo ChannelId %ChannelId%

rem set isBundle=1
rem set dd=0
rem for /f "delims=_" %%I in (%ChannelId%) do (

rem 	set cid=%%I
rem 	echo cid !cid!
rem 	rem echo I %%I
rem 	rem set /a dd=!dd!+1
rem 	rem echo dd %dd%
rem 	rem echo dd !dd!
rem 	call test_1.bat !cid!

rem 	echo isBundle !isBundle!
rem 	set isBundle=0
rem )

rem set dd=aa_bb_cc
rem set str="%dd%"
rem for /f "delims=_, tokens=1,2,3*" %%i in (%str%) do (
rem 	echo %%i %%j %%k
rem 	rem set str="%%j"
rem )

rem set ChannelId=aa-bb-cc
rem set allChannel=
rem :GOON
rem for /f "delims=-, tokens=1,*" %%i in ("%ChannelId%") do (
rem 	set allChannel=%allChannel%,%%i
rem 	set ChannelId=%%j
rem 	goto GOON
rem )
rem echo %allChannel%
rem for %%I in (%allChannel%) do (
rem 	echo %%I
rem )

set channelid=aa_dd-bb_dd-cc_ff
set channelid=%channelid:-=,%
for %%I in (%channelid%) do echo %%I

pause