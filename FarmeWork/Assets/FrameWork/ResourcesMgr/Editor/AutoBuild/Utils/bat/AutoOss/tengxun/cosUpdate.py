#-*- coding: utf-8 -*- 
import os, sys
import json
import shutil
import logging

from qcloud_cos import CosConfig
from qcloud_cos import CosS3Client

from tencentcloud.common import credential
from tencentcloud.common.profile.client_profile import ClientProfile
from tencentcloud.common.profile.http_profile import HttpProfile
from tencentcloud.common.exception.tencent_cloud_sdk_exception import TencentCloudSDKException
from tencentcloud.cdn.v20180606 import cdn_client, models

# 路径
configPath = sys.argv[1]
# configPath = "C:\\Users\\RBW\\Desktop\\pythonCdnSdk\\ossConfig"
# configPath = os.path.split(os.path.realpath(__file__))[0]
baseInfoPath = configPath+"\\oss.json"
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

print ("startCos")

#初始化基础信息
with open(baseInfoPath, 'r') as rf:
    data = json.load(rf)
    endpoint = data["endpoint"]
    bucket_name = data["bucket_name"]
    cdnHttp = data["cdnHttp"]
    access_key_id = data["access_key_id"]
    access_key_secret = data["access_key_secret"]

#获取更新文件
with open(updateInfoFile, 'r') as rf:
    data = json.load(rf)
    updateFileArr = data["UpdateRecordList"]
    deleteFileArr = data["DeleteRecordList"]

# {
#     "UpdateRecordList":[{"LocalFilePath":"C:\\Users\\RBW\\Desktop\\cdn\\2012.txt","OssFilePath":"Overseas/2012.txt"},{"LocalFilePath":"C:\\Users\\RBW\\Desktop\\cdn\\2013.txt","OssFilePath":"Overseas/2013.txt"}],
#     "DeleteRecordList":["Overseas/dddddd.txt","Overseas/v.txt","Overseas/2222.txt","Overseas/1111.txt"]
# }

# updateFileArr = [{"LocalFilePath":"C:\\Users\\RBW\\Desktop\\cdn\\v.txt","OssFilePath":"Overseas/v.txt"}]
# deleteFileArr = ["Overseas/dddddd.txt"]

def refreshCdn():
    req = models.PurgeUrlsCacheRequest()

    params = {"Urls": []}
    # 更新
    if updateFileArr:
        for updateFile in updateFileArr:
            params["Urls"].append(cdnHttp+updateFile['OssFilePath'])
    # 删除
    if deleteFileArr:
        for deleteFile in deleteFileArr:
            params["Urls"].append(cdnHttp+deleteFile['OssFilePath'])

    if params["Urls"]:
        req.from_json_string(json.dumps(params))
        resp = cdnClient.PurgeUrlsCache(req) 
        print(resp.to_json_string())
    else:
        print("no updateFile")

def preheatCdn():
    req = models.PushUrlsCacheRequest()
    params = {"Urls": []}
    if updateFileArr:
        for updateFile in updateFileArr:
            params["Urls"].append(cdnHttp+updateFile['OssFilePath'])

    if params["Urls"]:
        req.from_json_string(json.dumps(params))
        resp = cdnClient.PushUrlsCache(req) 
        print(resp.to_json_string()) 
    else:
        print("no deleteFile")


try: 
    # COS操作
    logging.basicConfig(level=logging.INFO, stream=sys.stdout)
    secret_id = access_key_id      # 替换为用户的 secretId
    secret_key = access_key_secret  # 替换为用户的 secretKey
    region = 'ap-beijing'     # 替换为用户的 Region
    token = None                # 使用临时密钥需要传入 Token，默认为空，可不填
    scheme = 'https'            # 指定使用 http/https 协议来访问 COS，默认为 https，可不填
    config = CosConfig(Region=region, SecretId=secret_id, SecretKey=secret_key, Token=token, Scheme=scheme)
    # 2. 获取客户端对象
    cosClient = CosS3Client(config)

    # 3.上传
    if updateFileArr:
        for updateFile in updateFileArr:
            response = cosClient.upload_file(
               Bucket=bucket_name,
               LocalFilePath=updateFile['LocalFilePath'],
               Key=updateFile['OssFilePath'],
               PartSize=1,
               MAXThread=10,
               EnableMD5=False
            )

    # 4.删除
    if deleteFileArr:
        for deleteFile in deleteFileArr:
            response = cosClient.delete_object(
                Bucket=bucket_name,
                Key=deleteFile['OssFilePath']
            )

    # CDN操作
    cred = credential.Credential(access_key_id, access_key_secret) 
    httpProfile = HttpProfile()
    httpProfile.endpoint = endpoint

    clientProfile = ClientProfile()
    clientProfile.httpProfile = httpProfile
    cdnClient = cdn_client.CdnClient(cred, "", clientProfile) 

    refreshCdn()
    preheatCdn()

except TencentCloudSDKException as err: 
    print(err) 

print ("endCos")