'use strict';
//モジュールの読み込み
const fs = require('fs');
const readline = require('readline');
const crypter = require('crypto');

class ArxivMetadata{
    id : string;
    submitter : string;
    authors : string;
    title : string;
    comments : string;
    doi : string;
    categories : string;
    licence :any;
    abstract : string;
    versions : any[];
    update_date : string;
    authors_parsed : any[];

    
}

//readstreamを作成
const rs = fs.createReadStream("E:/stringology/arxiv-metadata-oai-snapshot.json/arxiv-metadata-oai-snapshot.json");
//writestreamを作成
const ws = fs.createWriteStream('./output.csv');

//インターフェースの設定
const rl = readline.createInterface({
//読み込みたいストリームの設定
  input: rs,
//書き出したいストリームの設定
  output: ws
});


function category_check(category : string) : boolean {
    return category.indexOf("cs.") != -1 || category.indexOf("math.") != -1;
}

let k = 0;
//1行ずつ読み込む設定
rl.on('line', (lineString) => {

    const p : ArxivMetadata = JSON.parse(lineString);
    k++;
    const ango = crypter.createHash('md5').update(p.title).digest('hex');
    //console.log('md5:    ' + );
    ws.write(p.id + "/" +  ango + '\n');
    if(k % 100000 == 0){
        console.log(k);
    }
    /*
    if(category_check(p.categories)){
        console.log(`${k} : ${p.categories}`);
    }
    */
    
});
rl.on('close', () => {
  console.log("END!" + k);
});