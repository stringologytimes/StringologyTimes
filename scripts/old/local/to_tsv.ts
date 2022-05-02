

var request = require('sync-request');
const fs = require('fs');

const text = fs.readFileSync("data/test.txt", 'utf8');
const lines = text.split("\n");
let output_text = "";
let pstr = "";
lines.forEach((v, i) =>{
    if(i % 2 == 0){
        pstr += v;
    }else{
        pstr += "\t" + v;
        output_text += pstr + "\n";
        pstr = "";
    }
})
fs.writeFile('./data/test.tsv', output_text, (err, data) => {
    if (err) console.log(err);
    else console.log('write end');
});    
//console.log(text);

/*
var regexp = new RegExp(/http(s)?:\/\/([\S])+/, 'g'); 
var result : any[] = text.match(regexp);

for(var i=0;i<result.length;i++){
    console.log(result[i]);
}
*/
