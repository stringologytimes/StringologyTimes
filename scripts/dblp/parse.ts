
import { type } from "os";
import { DOMParser } from 'xmldom'
import { PaperArticle } from "../article"
const fs = require('fs');

function TrimRightmostDot(title : string) : string{
    const p = title.indexOf(".");
    if(p != -1){
        return title.substring(0, p);
    }else{
        return title;
    }
}

const dblpFolderPath = "./data/dblp";

function loadJson(conference : string, year : number) : PaperArticle[]{
    const filepath = `data/dblp/${conference}_${year}.json`;
    const text = fs.readFileSync(filepath, 'utf8');
    
    const document = JSON.parse(text);
    
    const outputArticles : PaperArticle[] = [];
    
    const collection : any[] = document["result"]["hits"]["hit"];
    collection.forEach((v) =>{
        const p = new PaperArticle();
        const info = v["info"];
        p.title = TrimRightmostDot(info["title"]);
        p.conferencePaperURL = info["ee"];
        p.conference = conference;
        p.year = year;
        if(info["authors"] != undefined){
            const authorsInfo : any = info["authors"]["author"]; 
    
            if(Array.isArray(authorsInfo)){
                p.authors = authorsInfo.map((inf) => inf["text"]);    
            }else{
                p.authors = [authorsInfo["text"]];
            }
            outputArticles.push(p);
            //console.log(p.toString());
        
        }
    
    })
    return outputArticles;
    
}

const jsonList : [string, number][] = [];
const files = fs.readdirSync(dblpFolderPath)
files.filter((v) =>{
    return fs.statSync(dblpFolderPath + "/" + v).isFile() && /.*\.json$/.test(dblpFolderPath + "/" + v);
}).forEach((filename) =>{
    const p1 = filename.split(".")[0];
    const conferenceName = p1.split("_")[0];
    const year = Number.parseInt(p1.split("_")[1]);
    console.log([conferenceName, year]);

    jsonList.push([conferenceName, year]);
})

const hList : PaperArticle[] = [];
jsonList.forEach(([conf, year]) =>{
    loadJson(conf, year).forEach((v) =>{
        hList.push(v);
    })
})
hList.forEach((v) =>{
    console.log(v.toCSSString());
})


/*
//console.log(document);

*/