#-*- coding: utf-8 -*- 
import os, sys
import json
import shutil

import oss2
from Refresh import Refresh

# 路径
configPath = sys.argv[1]
# configPath = "C:\\Users\\RBW\\Desktop\\pythonCdnSdk\\ossConfig"
# configPath = os.path.split(os.path.realpath(__file__))[0]
baseInfoPath = configPath+"\\ossInfo.json"
updateInfoFile = configPath+"\\ossUpdateInfo.txt"
cdnFile =  configPath+"\\cdnFile.txt"

#oss访问信息
endpoint = ""
bucket_name = ""
access_key_id = ""
access_key_secret = ""

#cdn信息
cdnHttp = ""
refreshCdnArgs = ""
preheatCdnArgs = ""

#更新与删除文件列表
updateFileArr = []
deleteFileArr = []

print ("startOss")

#初始化基础信息
with open(baseInfoPath, 'r') as rf:
	data = json.load(rf)
	endpoint = data["endpoint"]
	bucket_name = data["bucket_name"]
	cdnHttp = data["cdnHttp"]
	access_key_id = data["access_key_id"]
	access_key_secret = data["access_key_secret"]

refreshCdnArgs = {"-i":access_key_id,"-k":access_key_secret,"-r":cdnFile,"-t": "clear"}
preheatCdnArgs = {"-i":access_key_id,"-k":access_key_secret,"-r":cdnFile,"-t": "push"}

#获取更新文件
with open(updateInfoFile, 'r') as rf:
	data = json.load(rf)
	updateFileArr = data["UpdateRecordList"]
	deleteFileArr = data["DeleteRecordList"]

# updateFileArr = [{'LocalFilePath':'D:/JenkinsWorkSpace/bat/ddd/111.txt','OssFilePath':'Test/ddd/111.txt'},{'LocalFilePath':'D:/JenkinsWorkSpace/bat/ddd/222.txt','OssFilePath':'Test/ddd/222.txt'}]
# deleteFileArr = [{"OssFilePath":"Test/ddd/111.txt"}]

def preheatCdn():
	#写入文件
	cdnfile_handle=open(cdnFile,mode='w')
	for updateFile in updateFileArr:
		cdnfile_handle.write(cdnHttp+updateFile['OssFilePath']+'\n')
	cdnfile_handle.close()

	#刷新预热
	cdnObj = Refresh()
	cdnObj.mainPython(refreshCdnArgs)
	cdnObj = Refresh()
	cdnObj.mainPython(preheatCdnArgs)

def refreshCdn():
	#写入文件
	cdnfile_handle=open(cdnFile,mode='w')
	for deleteFile in deleteFileArr:
		cdnfile_handle.write(cdnHttp+deleteFile['OssFilePath']+'\n')
	cdnfile_handle.close()

	#刷新cdn
	cdnObj = Refresh()
	cdnObj.mainPython(refreshCdnArgs)


# 创建Bucket对象，所有Object相关的接口都可以通过Bucket对象来进行
bucket = oss2.Bucket(oss2.Auth(access_key_id, access_key_secret), endpoint, bucket_name)

if updateFileArr:
	# 把本地文件更新到OSS
	for updateFile in updateFileArr:
		print ("update:"+updateFile['LocalFilePath']+'--->'+updateFile['OssFilePath'])
		with open(oss2.to_unicode(updateFile['LocalFilePath']), 'rb') as f:
			bucket.put_object(updateFile['OssFilePath'], f)
	preheatCdn()
else:
	print ("no update")


if deleteFileArr:
	for deleteFile in deleteFileArr:
		print ("delete:"+deleteFile['OssFilePath'])
		bucket.delete_object(deleteFile['OssFilePath'])
	# bucket.batch_delete_objects(deleteFileArr)
	refreshCdn();
else:
	print ("no delete")


print ("endoss")


#1、有的玩家获取到新资源,有的玩家获取到老资源,是因为我刷新之后立即预热了么