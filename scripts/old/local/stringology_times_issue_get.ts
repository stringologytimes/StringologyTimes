//import { brotliDecompressSync } from "zlib";
//import { doesNotReject } from "assert";


const https = require('https');
var fs = require("fs");

enum GithubItem {
    Issues = "issues", Labels="labels"
  } 


async function httpGet(opts) : Promise<string> {
    const ret =  new Promise(((resolve,reject)=>{
        console.log('Promiseの引数の関数開始');
        let req = https.request(opts, (response) => {
            console.log('---response---');
            response.setEncoding('utf8');
            let body = '';
            response.on('data', (chunk)=>{
                //console.log('chunk:', chunk);
                body += chunk;
            });
            response.on('end', ()=>{
                //console.log('end:', body);
                resolve(body);
            });
        }).on('error', (err)=>{
            console.log('error:', err.stack);
            reject(err);
        });
        //req.write(replyData);
        req.end();
        console.log('Promiseの引数の関数終了')
    }))
    let s : string = "";
    await ret.then((v : string)=>{ 
        s = v; 
    });
    return s;
};

function mergeJsons(item1: any[], item2: any[]) {
    item2.forEach(v => item1.push(v) );
}
async function getPartialIssues(access_token : string,option :{ type : string, page : number, pageSize : number}): Promise<any[] | null> {
    const options = {
        host: 'api.github.com',
        path: `/repos/stringologytimes/StringologyTimes/${option.type}?access_token=${access_token}&page=${option.page}&per_page=${option.pageSize}`,
        method: 'GET',
        headers: { 'user-agent': 'node.js' }
    };
    const str = await httpGet(options);
    if(str == "[]"){
        return null;
    }else{
        return JSON.parse(str);
    }
}


async function getGithubJson(itemName : GithubItem, access_token : string): Promise<any[]> {    
    const result : any[] = [];
    for(let i=1;i<1000;i++){
        const tmp = await getPartialIssues(access_token,{type:itemName,page:i,pageSize:100});
        if(tmp != null){
            mergeJsons(result, tmp);
        }else{
            break;
        }
    }

    return result;
}

//const res1 = getIssues();


async function main() {
    for(var i = 0;i < process.argv.length; i++){
        console.log("argv[" + i + "] = " + process.argv[i]);
      }
    const access_token = process.argv[2];
    
    
    let s : any = "";

    const issueResult = await getGithubJson(GithubItem.Issues,access_token);
    fs.writeFile('./data/issue.json', JSON.stringify(issueResult, null, "\t"), (err, data) => {
        if (err) console.log(err);
        else console.log('write end');
    });

    const labelResult = await getGithubJson(GithubItem.Labels,access_token);
    fs.writeFile('./data/label.json', JSON.stringify(labelResult, null, "\t"), (err, data) => {
        if (err) console.log(err);
        else console.log('write end');
    });
    
    console.log("");
    //console.log(result);
    console.log("end");

    
}

main();



