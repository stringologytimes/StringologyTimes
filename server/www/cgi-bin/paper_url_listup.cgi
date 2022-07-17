#!/home/lampin/local/python/bin/python3.9
#/usr/bin/python3

# -*- coding: utf-8 -*-
import cgi
import MySQLdb
import my_common
import os

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

results = my_common.listup_url(cursor)

conn.commit()
conn.close()

print("Content-Type: text/plain\n")
for item in results:
    print(item[1])
