const request = require('sync-request');
const fs = require('fs');
import { DOMParser } from 'xmldom'
import { loadArxivXML, ArxivXMLInfo } from "./arxiv_xml"

function sleep(time) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            resolve('Success');
        }, time);
    });
}



function getArxivXML(url) {
    //var response = '';
    const response = request('GET', url);
    const p = response.body.toString('utf-8', 0, response.body.length)
    const document = new DOMParser().parseFromString(p);

    return document;
}

/*
  var nXML : HTMLDocument = new xmldom.DOMParser().parseFromString(
    '<?xml version="1.0" encoding="UTF-8" ?><articles></articles>','text/xml');

*/

function downloadArxivInfomation(ids: string[], arxivInfo: ArxivXMLInfo) {
    sleep(5000);
    //sleep.sleep(5);
    console.log("get infomation");
    console.log(ids);
    const idsStr = ids.join(",")

    const document: XMLDocument = getArxivXML(`http://export.arxiv.org/api/query?id_list=${idsStr}`);
    console.log("END");
    console.log(`get URL http://export.arxiv.org/api/query?id_list=${idsStr}`);
    console.log("END2");

    const entryCol = document.getElementsByTagName('entry');

    const articlesNode = arxivInfo.document.getElementsByTagName("articles").item(0)!;
    for (let i = entryCol.length - 1; i >= 0; i--) {
        const entry = entryCol.item(i)!;
        const idNode = entry.getElementsByTagName("id").item(0)!.textContent!;
        console.log(`${ids[i]} / ${idNode} / ${idNode.indexOf(ids[i]) != -1}`)
        if(idNode.indexOf("incorrect_id_format") != -1){
            throw new Error("incorrect_id_format");
        }

        if (idNode.indexOf(ids[i]) != -1) {
            entry.setAttribute("id", ids[i]);
            articlesNode.appendChild(entry);
            arxivInfo.dic.add(ids[i]);
        }
    }
    console.log("dicSIze" + arxivInfo.dic.size);
}
function downloadAllArxivInfomation(ids: string[], arxivInfo: ArxivXMLInfo) {
    const arr: string[] = new Array(0);
    for (let i = 0; i < ids.length; i++) {
        arr.push(ids[i]);
        if (arr.length == 10 || (arr.length > 0 && i == ids.length - 1)) {
            downloadArxivInfomation(arr, arxivInfo);
            arr.length = 0;
        }
    }
}

function load_arxiv_ids(urlPath : string){
    const text = fs.readFileSync(urlPath, 'utf8');
    const lines : string[] = text.toString().split(/\r\n|\n/);
    const id_arr: string[] = new Array(0);
    for (let line of lines) {
        const b = line.indexOf("https://arxiv.org/") == 0;
        if (b) {

            const subs = line.split("/");
            const pindex = subs.indexOf("abs");
            if(pindex != -1){
                const id = subs.slice(pindex+1).join("/");
                id_arr.push(id);

            }
            //const id: string = subs[subs.length - 1];
            
            /*
            if (!arxivXMLInfo.dic.has(id)) {
                console.log(`new ID = ${id}`)
                id_arr.push(id);
            }
            */
    
        }
    }
    return id_arr;    
}


const arxivXMLPath = "data/arxiv.xml";
const arxivXMLInfo : ArxivXMLInfo = loadArxivXML(arxivXMLPath);
const urlPath = "data/paper_list.txt";
const id_array = load_arxiv_ids(urlPath);
const new_id_array = id_array.filter((id) => !arxivXMLInfo.dic.has(id));


if (new_id_array.length > 0) {
    downloadAllArxivInfomation(new_id_array, arxivXMLInfo);
}

try {
    fs.writeFileSync(arxivXMLPath, arxivXMLInfo.document.toString());
    console.log(`Write ${arxivXMLPath}`);

} catch (e) {
    console.log(e);
}

