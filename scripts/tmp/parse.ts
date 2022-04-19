
import { type } from "os";
import { DOMParser } from 'xmldom'
import { PaperArticle } from "../article"
import { DBLPArticle } from "../dblp"

const fs = require('fs');

class DOIArticle{
    public doi : string;
    public title : string;
}



const dblpFolderPath = "./data/dblp";

const hList = DBLPArticle.loadPaperArticlesInDBLPFolder(dblpFolderPath);

//const logTxt = hList.map((v) => v.uniqueName()).join("\n");

const hListMap : Map<string, number> = new Map();
const doiListMap : Map<string, number> = new Map();
const focusedDoiListMap : Map<string, number> = new Map();

hList.forEach((v, i) =>{
    hListMap.set(v.uniqueName(), i);
    if(v.doi != undefined){
        doiListMap.set(v.doi, i);
    }
    if(v.conference == "CPM" || v.conference == "SPIRE" || v.conference == "PSC"){
        if(v.doi != undefined){
            focusedDoiListMap.set(v.doi, i);
        }else if(v.conferencePaperURL != undefined){
            focusedDoiListMap.set(v.conferencePaperURL, i);
        }else{
            console.log(`Unhit: ${v.conference} ${v.title}`);
        }
    }
})

const paperUrl = "./data/spreadsheets/Stringology Conference - Old papers.tsv";
const paperInfoRawText: string = fs.readFileSync(paperUrl, 'utf8');
const papers = PaperArticle.parseList(paperInfoRawText);

const doiArticleArray : DOIArticle[] = new Array();

//const doiArticleMap : Map<string, DOIArticle> = new Map();
const notFoundDOIArticleArray : string[] = new Array();

papers.forEach((v) =>{
    const i = hListMap.get(v.uniqueName());

    if(i != undefined){
        let doiOrURL : string | undefined = undefined;
        if(hList[i].doi != undefined){
            doiOrURL = hList[i].doi;
        }else{
            doiOrURL = hList[i].conferencePaperURL;
        }
        if(doiOrURL == undefined){
            console.log(hList[i]);
        }{
            focusedDoiListMap.set(doiOrURL, i);
        }
    }
})
papers.filter((v) => v.doi.length > 0).forEach((v) =>{
    const i = doiListMap.get(v.doi);
    if(i != undefined){
        focusedDoiListMap.set(v.doi, i);
    }
})

focusedDoiListMap.forEach((i, key) =>{
    const p = new DOIArticle();
    if(hList[i].doi != undefined){
        p.doi = hList[i].doi;
    }else{
        p.doi = hList[i].conferencePaperURL;
    }
    if(p.doi == undefined){
        console.log(`Unhit: ${key}`);
    }
    p.title = hList[i].title;
    doiArticleArray.push(p);

})

/*
papers.forEach((v) =>{
    const i = hListMap.get(v.uniqueName());

    if(i != undefined){
        const p = new DOIArticle();
        if(hList[i].doi != undefined){
            p.doi = hList[i].doi;
        }else{
            p.doi = hList[i].conferencePaperURL;
        }
        if(p.doi == undefined){
            console.log(p);
        }

        p.title = hList[i].title;
        doiArticleArray.push(p);
    }else{
        if(v.doi.length > 0 && hList.find((w) => w.doi == v.doi) != undefined){
            const res = hList.find((w) => w.doi == v.doi);
            if(res != undefined){
                const p = new DOIArticle();
                p.doi = v.doi;
                p.title = res.title;
                doiArticleArray.push(p);            
            }
    
        }
        else{
            console.log("Unhit: " + v.uniqueName());
            notFoundDOIArticleArray.push(v.uniqueName());
        }
    }
})
*/


const logTxt = doiArticleArray.map((v) => `${v.doi}`).join("\n");
fs.writeFile("./data/doi.txt", logTxt, (err, data) => {
    if (err) console.log(err);
    else console.log(`Wrote: ./data/doi.txt`);
});

if(notFoundDOIArticleArray.length > 0){
    const logTxt2 = notFoundDOIArticleArray.map((v) => `${v}`).join("\n");
    fs.writeFile("./data/hList_unhit.log", logTxt2, (err, data) => {
        if (err) console.log(err);
        else console.log(`Wrote: ./data/hList_unhit.log`);
    });    
}


/*
//console.log(document);

*/