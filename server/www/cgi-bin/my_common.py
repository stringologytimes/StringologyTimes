#!/home/lampin/local/python/bin/python3.9
#/usr/bin/python3
# -*- coding: utf-8 -*-
import MySQLdb

def remove_special_characters(text):
    text = text.replace(' ', '')
    text = text.replace("http://arxiv.org/abs/", 'https://arxiv.org/abs/')
    text = text.replace("https://doi.org/", "")

    return text
def check_correct_input(url):
    if url.find("^") == 0: 
        split_words = url.split(",")
        if split_words[0] == "^ProceedingName": 
            return "SPECIAL"
        elif split_words[0] == "^JournalURL":
            return "SPECIAL"
        else: 
            return "OTHER"
    elif url.find("https://arxiv.org/abs/") == 0: 
        return "ARXIV"
    elif url.find("10.") == 0: 
        return "DOI"
    elif url.find("http://www.stringology.org/event/") == 0: 
        return "STRINGOLOGY"
    else:
        return "OTHER"
def add_data(cursor, url): 
    url = remove_special_characters(url)
    id = cursor.execute('select * from paper_url_list')
    hit = cursor.execute(f'select * from paper_url_list where url="{url}"')
    inputType = check_correct_input(url)
    if inputType == "ARXIV" or inputType == "DOI" or inputType == "STRINGOLOGY" or inputType == "SPECIAL": 
        if hit == 0: 
            cursor.execute(f'INSERT INTO paper_url_list values ({id}, "{url}", 1)')
            return "SUCCESS"
        else:
            return "DUPLICATION"
    else:
        return "INVALID"
def listup_url(cursor): 
    cursor.execute('select * from paper_url_list')
    result = cursor.fetchall()
    return result