import { type } from "os";

//import xmldom from "xmldom";
//var sleep = require('sleep');
//var request = require('request');
var request = require('sync-request');
var xmldom = require('xmldom');

const fs = require('fs');

function sleep(time) {
  return new Promise((resolve, reject) => {
    setTimeout(() => {
      resolve('Success');
    }, time);
  });
}

type ArxivXMLInfo = {
  document: XMLDocument,
  dic: Set<string>
}

function loadArxivXML(): ArxivXMLInfo {
  const xml = fs.readFileSync("data/arxiv.xml", 'utf8');
  const document = new xmldom.DOMParser().parseFromString(xml);
  const dic = new Set<string>();
  const info: ArxivXMLInfo = { document: document, dic: dic };
  const entries = document.getElementsByTagName("entry");
  for (let i = 0; i < entries.length; i++) {
    const entry = entries.item(i);
    const id = entry.getAttribute("id");
    if(!dic.has(id)){
      dic.add(id);
    }else{
      document.removeChild(entry);
      console.log(`Remove: /${id}/`);
    }
  }
  return info;
}

class ArxivArticle {
  node: Element;
  date: Date;
  id: string;
  title: string;
  url: string;
  public constructor(_node: Element) {
    this.node = _node;
    const dateStr: string = this.node.getElementsByTagName("published").item(0)!.textContent!;
    this.date = new Date(dateStr);

    this.id = this.node.getAttribute("id")!;
    this.title = this.node.getElementsByTagName("title").item(0)!.textContent!;
    this.url = `https://arxiv.org/abs/${this.id}`;
  }
}



function getArxivXML(url) {
  //var response = '';
  const response = request('GET', url);
  const p = response.body.toString('utf-8', 0, response.body.length)
  const document = new xmldom.DOMParser().parseFromString(p);

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
//http://export.arxiv.org/api/query?id_list=2003.10069

function createPaperHTML(arxivInfo: ArxivXMLInfo): string {
  const outputArr: string[] = new Array(0);
  const entryCol = arxivInfo.document.getElementsByTagName('entry');
  const arr: ArxivArticle[] = new Array(0);
  for (let i = 0; i < entryCol.length; i++) {
    arr.push(new ArxivArticle(entryCol.item(i)!));
  }
  arr.sort((a, b) => {
    return b.date.getTime() - a.date.getTime();
  })

  outputArr.push("# Arxiv Papers");
  arr.forEach((v, i) => {
    const title = v.title.replace(/\r?\n/g, '');
    if (i == 0 || arr[i - 1].date.getMonth() != arr[i].date.getMonth()) {
      outputArr.push(`## ${v.date.getFullYear()}/${v.date.getMonth() + 1}/`)

    }
    outputArr.push(`- [${title}](${v.url})`)
    //console.log(v.id + "/" + v.date.getMonth());
  })
  return outputArr.join("\n");
}



const arxivXMLInfo = loadArxivXML();

const text = fs.readFileSync("data/arxiv_url.txt", 'utf8');
const lines = text.toString().split('\n');
const id_arr: string[] = new Array(0);
for (let line of lines) {
  const subs = line.split("/");
  const id: string = subs[subs.length - 1];
  if (!arxivXMLInfo.dic.has(id)) {
    console.log(`new ID = ${id}`)
    if (id.indexOf('\r') != -1) {
      throw new Error("The arXiv URL contains \\r character! Please remove the character from the URL.");
    }
    id_arr.push(id);
  }
}
console.log(id_arr);



if (id_arr.length > 0) {
  downloadAllArxivInfomation(id_arr, arxivXMLInfo);
}

try {
  fs.writeFileSync("data/arxiv.xml", arxivXMLInfo.document.toString());
  console.log("Write data/arxiv.xml");

} catch (e) {
  console.log(e);
}


const paperHTML = createPaperHTML(arxivXMLInfo);

try {
  fs.writeFileSync("docs/arxiv_list.md", paperHTML);
  console.log("Write: docs/arxiv_list.md");

} catch (e) {
  console.log(e);
}

