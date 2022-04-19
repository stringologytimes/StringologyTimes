// tsvに記された学会のDBLP情報をダウンロード

import { ConferenceArticle } from "../article"

var request = require('sync-request');
const fs = require('fs');

function sleep(time) {
    return new Promise((resolve, reject) => {
      setTimeout(() => {
        resolve('Success');
      }, time);
    });
  }

const conferenceUrl = `data/spreadsheets/Stringology Conference - Conference Site.tsv`;
const outputFolder = `data/dblp`;


const conferenceInfoRawText: string = fs.readFileSync(conferenceUrl, 'utf8');
const conferences = ConferenceArticle.parseList(conferenceInfoRawText);

conferences.forEach((v) => {
    if (v.dblpName != null && v.dblpName!.length > 0) {
        const outputFileName = outputFolder + "/" + v.conference + "_" + v.year + ".json";
        if (!fs.existsSync(outputFileName)) {
            const pURL = v.getDBLPJSONURL();

            //console.log("Download " + pURL);
            const response = request('GET', pURL);
            const p : string = response.body.toString('utf-8', 0, response.body.length);

            const p1 = p.indexOf("(");
            const p2 = p.lastIndexOf(")");
            const jsonText = p.substring(p1+1, p2);

            fs.writeFile(outputFileName, jsonText, (err, data) => {
                if (err) console.log(err);
                else console.log(`Downloaded ${v.conference} ${v.year}`);
            });
            sleep(5000);

        }


    }
})