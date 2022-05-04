#!/home/lampin/local/python/bin/python3.9
#/usr/bin/python3
# -*- coding: utf-8 -*-
import cgi
import MySQLdb
import my_common
import os

os.chdir(os.path.dirname(os.path.abspath(__file__)))

#path = os.getcwd()
#print("Content-Type: text/plain\n")
#print(path)

f = open('../../pass.txt', 'r')
pass_data = f.read()
f.close()
passlines = pass_data.splitlines()
_host = passlines[0]
_db = passlines[1]
_user = passlines[2]
_passwd = passlines[3]


form = cgi.FieldStorage()
url_form_value = form.getfirst('url_area')
conn = MySQLdb.connect(host=_host, db=_db,user=_user,passwd=_passwd,charset='utf8mb4')
cursor = conn.cursor()

result_list = [] 
url_lines = url_form_value.splitlines()
for url in url_lines:
    result = my_common.add_data(cursor, url)
    result_list.append([url, result])
    

conn.commit()
conn.close()

print("Content-Type: text/html")
#print("Content-Type: text/plain\n")

prefhtmlText = f'''
<!DOCTYPE html>
<html>
    <head><meta charset="utf-8" /></head>
<body bgcolor="lightyellow">
    <h1>Result</h1>'''
sufhtmlText = f'''
    <hr/>
    <a href="../url_register.html">戻る</a>　
</body>
</html>
'''
htmlText = prefhtmlText
for [url, result] in result_list: 
    if result == "SUCCESS": 
        htmlText += f'''<p>SUCESS: "{url}"</p>'''
    elif result == "DUPLICATION": 
        htmlText += f'''<p>DUPLICATION: "{url}" is already registered in the table.</p>'''
    else:
        htmlText += f'''<p>INVALID: "{url}" is an invalid string.</p>'''

htmlText += sufhtmlText
print(htmlText.encode("cp932", 'ignore').decode('cp932') )
