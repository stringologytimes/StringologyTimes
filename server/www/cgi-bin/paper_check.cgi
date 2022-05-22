#!/home/lampin/local/python/bin/python3.9
#/usr/bin/python3
# -*- coding: utf-8 -*-

import cgi
import MySQLdb
import my_common
import os
import sys
import io
import json
import urllib.parse

os.chdir(os.path.dirname(os.path.abspath(__file__)))

f = open('../../pass.txt', 'r')
pass_data = f.read()
f.close()
passlines = pass_data.splitlines()
_host = passlines[0]
_db = passlines[1]
_user = passlines[2]
_passwd = passlines[3]
conn = MySQLdb.connect(host=_host, db=_db,user=_user,passwd=_passwd,charset='utf8mb4')
cursor = conn.cursor()


sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
getVal = os.environ.get('QUERY_STRING')
grpList = getVal.split('&')
grpDict = {}
for grp in grpList:
    tmpList = grp.split('=')
    grpDict[tmpList[0]] = tmpList[1]


url = urllib.parse.unquote(grpDict["url"])

mode = grpDict["mode"]

result = ""
if mode == "register" : 
    result = my_common.add_data(cursor, url)
else:    
    result = my_common.check_data(cursor, url)
conn.commit()
conn.close()
    
    
result_json = {'result': result, 'url' : url}
print('Content-Type: application/json; charset=utf-8\n')
print(json.dumps(result_json))

#print("[Python CGI Test 03]",file=sys.stderr)
