

var request = require('sync-request');
const fs = require('fs');

const text = fs.readFileSync("data/test.txt", 'utf8');

//console.log(text);

var users = 'user-0, user1, users-d, myuser'; 
var regexp = new RegExp(/http(s)?:\/\/([\S])+/, 'g'); 
var result : any[] = text.match(regexp);

for(var i=0;i<result.length;i++){
    console.log(result[i]);
}

