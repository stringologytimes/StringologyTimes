

var request = require('sync-request');
const fs = require('fs');

const text : string = fs.readFileSync("docs/conference/2021.md", 'utf8');

const lines = text.split("\n");

const r : string[] = new Array(0);
lines.forEach((v) =>{
    const regexp = new RegExp(/http(s)?:\/\/arxiv([\S])+/, 'g'); 
    const result : any = v.match(regexp);
    
    const regexp_sigport = new RegExp(/http(s)?:\/\/sigport([\S])+/, 'g'); 
    const result_sigport : any = v.match(regexp_sigport);

    const regexp_youtube = new RegExp(/http(s)?:\/\/www\.youtube([\S])+/, 'g'); 
    const result_youtube : any = v.match(regexp_youtube);


    if(v[0] != "[" && result != null){
        const resultArr : string[] = result;
        r.push(`[arXiv](${resultArr[0]})  `);
    }else if(v[0] != "[" && result_sigport != null){
        const resultArr : string[] = result_sigport;
        r.push(`[SIGPORT](${resultArr[0]})  `);
    }else if(v[0] != "[" && result_youtube != null){
        const resultArr : string[] = result_youtube;
        r.push(`[YouTube](${resultArr[0]})  `);
    }
    else{
        r.push(v);
    }

})

const outputText = r.join("\n");


try {
    fs.writeFileSync("docs/conference/2021__.md", outputText);
    console.log("Write text");
  
  } catch (e) {
    console.log(e);
  }
  
