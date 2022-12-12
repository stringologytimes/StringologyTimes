
import * as fs from 'fs' 

const text : string = fs.readFileSync("data/test.txt", 'utf8');

//console.log(text);

var regexp = new RegExp(/http(s)?:\/\/([\S])+/, 'g'); 
var result = text.match(regexp);
if(result != null){
    for(var i=0;i<result.length;i++){
        console.log(result[i]);
    }    
}

