import { type } from "os";
var request = require('sync-request');
var xmldom = require('xmldom');
const fs = require('fs');

const url = "https://drops.dagstuhl.de/opus/portals/lipics/index.php?semnr=16107";
const response = request('GET', url);
const p = response.body.toString('utf-8', 0, response.body.length)

//console.log(p);

fs.writeFile('./data/local/cpm.html', p, (err, data) => {
    if (err) console.log(err);
    else console.log('write end');
});    



