
import {PaperArticle} from "../article"
enum PaperArticleProcessorMode {
    None, Title, Authors
}

class PaperArticleProcessor{
    paperArticle : PaperArticle = new PaperArticle();
    mode : PaperArticleProcessorMode = PaperArticleProcessorMode.None;

    public constructor() {
    }
    public read(line : string) : PaperArticle | null{
        let r : PaperArticle | null = null;
        if(line.substr(0, 3) == "## "){
            this.mode = PaperArticleProcessorMode.None;
            if(!this.paperArticle.isEmpty()){
                r = this.paperArticle.copy();
            }
            this.paperArticle.clear();

            const conf = line.substr(3);
            this.paperArticle.conference = conf;
        }else if(line.substr(0, 2) == "+ "){
            this.mode = PaperArticleProcessorMode.Title;
            if(!this.paperArticle.isEmpty()){
                r = this.paperArticle.copy();
            }
            this.paperArticle.clear();

            const leftKakko = line.indexOf("[");
            const rightKakko = line.indexOf("]");

            if(leftKakko == -1){
                const title = line.substr(2).trim();
                this.paperArticle.title = title;
            }else{
                const lineSuffix = line.substr(rightKakko+1);
                const leftBracket = lineSuffix.indexOf("(");
                const rightBracket = lineSuffix.indexOf(")");
    
                const title = line.substr(leftKakko+1, (rightKakko - leftKakko - 1)).trim();
                const url = lineSuffix.substr(leftBracket+1, (rightBracket - leftBracket - 1)).trim();
                this.paperArticle.title = title;
                this.paperArticle.conferencePaperURL = url;
            }

            //console.log(line.substr(2));
        }else if(line.substr(0, 7) == "[arXiv]" || line.substr(0, 5) == "[PDF]"){
            const leftBracket = line.indexOf("(");
            const rightBracket = line.indexOf(")");
            const url = line.substr(leftBracket+1, (rightBracket - leftBracket - 1)).trim();
            this.paperArticle.arxivURL = url;
        }else if(line.substr(0, 9) == "[SIGPORT]" || line.substr(0, 9) == "[YouTube]"){
            const leftBracket = line.indexOf("(");
            const rightBracket = line.indexOf(")");
            const url = line.substr(leftBracket+1, (rightBracket - leftBracket - 1)).trim();
            this.paperArticle.videoURL = url;
        }else if(line.substr(0, 7) == "[Slide]"){
            const leftBracket = line.indexOf("(");
            const rightBracket = line.indexOf(")");
            const url = line.substr(leftBracket+1, (rightBracket - leftBracket - 1)).trim();
            this.paperArticle.slideURL = url;
        }
        else if(this.mode == PaperArticleProcessorMode.Title){
            this.mode = PaperArticleProcessorMode.Authors;
            this.paperArticle.authors = new Array(0);
            this.paperArticle.authors.push(line);
        }else if(this.mode == PaperArticleProcessorMode.Authors){

        }

        return r;

    }

}

const xmdName = "x2020";

function ABCprocess() {
    var request = require('sync-request');
    const fs = require('fs');

    const text: string = fs.readFileSync(`data/md/${xmdName}.md`, 'utf8');

    const lines = text.split("\r\n");

    const r: PaperArticle[] = new Array(0);

    const pap = new PaperArticleProcessor();


    lines.forEach((v) => {
        const result = pap.read(v);
        if(result != null){
            r.push(result)
        }
    })

    const texts = r.map((v) =>{
        //console.log(v.toCSSString());
        return v.toCSSString();
    })

    
    const outputText = texts.join("\n");


    try {
        fs.writeFileSync(`data/md/${xmdName}.tsv`, outputText);
        console.log("Write text");

    } catch (e) {
        console.log(e);
    }
    

}
ABCprocess();

