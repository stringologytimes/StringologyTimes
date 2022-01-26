
import { type } from "os";
import { DOMParser } from 'xmldom'
const fs = require('fs');

class DagstuhlPaperArticle{
    
    public title : string;
    public authors : string[];
    public url : string;

    public toString() : string {
        return `${this.title}\t${this.authors.join(", ")}\t${this.url}`;
    }
}
function cleaning(name : string){
    const p = name.indexOf("{");
    if(p != -1){
        let newName = name;
        const repArr : [RegExp, string][] = [[/\{\\\\\\\'i\}/g, "í"],  
        [/\{\\\\\\\'e\}/g,"é"], 
        [/\{\\\\\\\"e\}/g,"ë"], 
        [/\{\\\\\\\"o\}/g,"ö"],
        [/\{\\\\\\\"a\}/g,"ä"], 
        [/\{\\\\\\\'y\}/g,"ý"]];
        repArr.forEach((v) =>{
            newName = newName.replace(v[0], v[1]);
        })
        if(newName.indexOf("{") != -1){
            console.log("Error!: " + newName);
        }
        return newName;
    }else{
        return name;
    }
}

const filepath = "data/local/cpm.html";
const text = fs.readFileSync(filepath, 'utf8');

const document = new DOMParser().parseFromString(text);
const entries = document.getElementsByTagName("a");

const outputArticles : DagstuhlPaperArticle[] = [];
for (let i = 0; i < entries.length; i++) {
    const entry = entries.item(i);    
    const hrefText : string | null = entry.getAttribute("href");
    if(hrefText != null){

        const prefix = `javascript:msg_win('@InProceedings`;
        const hrefPrefix = hrefText.substring(0, prefix.length);
        if(prefix == hrefPrefix){
            const article = new DagstuhlPaperArticle();
            const lines = hrefText.split("\n");
            lines.forEach((v) =>{
                const v2 = v.replace("<br />", "");
                const equalPosition = v2.indexOf("=");
                if(equalPosition != -1){
                    const leftStr = v2.substring(0, equalPosition).trim();
                    const rightStr = v2.substring(equalPosition+1).trim();
                    if(leftStr == "title"){
                        const text = rightStr.replace("{{", "").replace("}},", "");
                        article.title = text;
                    }
                    if(leftStr == "author"){
                        const p1 = rightStr.indexOf("{");
                        const p2 = rightStr.lastIndexOf("}");
                        const text1 = rightStr.substring(p1+1, p2).split(" and ").map((v) => cleaning(v));
                        article.authors = text1;
                    }
                    if(leftStr == "URL"){
                        const p1 = rightStr.indexOf("{");
                        const p2 = rightStr.lastIndexOf("}");
                        const text = rightStr.substring(p1+1, p2);
                        article.url = text;
                    }
                    //console.log([leftStr, rightStr]);
                }
            })
            outputArticles.push(article);
        }
    }
}

const outputPath = filepath + ".tsv";
const outputText = outputArticles.map((v) => v.toString()).join("\n");

fs.writeFile(outputPath, outputText, (err, data) => {
    if (err) console.log(err);
    else console.log('write end');
});   

//console.log(document);

