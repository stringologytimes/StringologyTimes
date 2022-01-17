
import { ConferenceArticle, PaperArticle } from "./article"

//const request = require('sync-request');
const fs = require('fs');

let conferenceUrl = "";
let paperUrl = "";
let outputFolder = "";

if(process.argv.length != 5){
    throw new Error("Arguments Error");
}
conferenceUrl = process.argv[2];
paperUrl = process.argv[3];
outputFolder = process.argv[4];



function createMD(_papers: PaperArticle[], _conferences: ConferenceArticle[], year: number) {
    const thisYearConferences = _conferences.filter((v) => v.year == year);
    thisYearConferences.sort((a, b) => {
        if (a.getDeadlineString() < b.getDeadlineString()) {
            return -1;
        } else {
            return 1;
        }
    }
    )
    const lines: string[] = new Array();
    lines.push(`# Conference List for Stringologist (${year})`);
    thisYearConferences.forEach((v) => {
        let s = "";
        if(v.conferenceURL.length > 0){
            s += `+ ${v.getDeadlineString()}: [${v.conference} ${v.year}](${v.conferenceURL})`;
        }else{
            s += `+ ${v.getDeadlineString()}: ${v.conference} ${v.year}`;
        }

        if(v.acceptedPaperURL.length > 0){
            s += ` [Accepted Papers](${v.acceptedPaperURL})`;
        }
        lines.push(s)
    })



    const thisYearPapers = _papers.filter((v) => v.year == year);
    lines.push(`# Conference Paper List for Stringologist`);


    thisYearConferences.forEach((conf) => {
        lines.push(`## ${conf.conference}`);
        const paperList = thisYearPapers.filter((v) => v.conference == conf.conference);
        paperList.sort((a, b) => {
            if (a.title < b.title) {
                return -1;
            } else {
                return 1;
            }
        })
        paperList.forEach((v) => {
            if(v.conferencePaperURL.length > 0){
                lines.push(`+ [${v.title}](${v.conferencePaperURL})  `);
            }else{
                lines.push(`+ ${v.title}  `);
            }
            lines.push(`${v.authors.join(", ")}  `);
            if (v.arxivURL.length > 0) {
                lines.push(`[Online Repository](${v.arxivURL})  `);
            }
            if (v.slideURL.length > 0) {
                lines.push(`[Slides](${v.slideURL})  `);
            }
            if (v.videoURL.length > 0) {
                lines.push(`[Video](${v.videoURL})  `);
            }
        })
        //paperList
    })


    return lines.join("\r\n");
}



//console.log(`${conferenceUrl}`)
//console.log(`${paperUrl}`)




const paperInfoRawText: string = fs.readFileSync(paperUrl, 'utf8');
const papers = PaperArticle.parseList(paperInfoRawText);

const conferenceInfoRawText: string = fs.readFileSync(conferenceUrl, 'utf8');
const conferences = ConferenceArticle.parseList(conferenceInfoRawText);


/*
papers.forEach((v) => {
    console.log(v.toCSSString());
})
*/

/*
conferences.forEach((v) => {
    console.log(v.toString());
})
*/

const yearSet = new Set<number>();
papers.forEach((v) => {
    if (!isNaN(v.year)) {
        yearSet.add(v.year)
    }
}
);

yearSet.forEach((key) => {
    const text = createMD(papers, conferences, key);

    try {
        fs.writeFileSync(`${outputFolder}/conference_list_${key}.md`, text);
        console.log(`Outputted markdown file for ${key}`);

    } catch (e) {
        console.log(e);
    }
})

