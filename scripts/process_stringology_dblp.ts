import { DOMParser } from 'xmldom'
import * as fs from 'fs'
import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "./dblp_element"
import { ArxivArticle } from "./arxiv_xml"

//const root = doc.getElementsByTagName("dblp").item[0];

function createInproceedingsMD(_inproceedings_papers: DBLPInproceedings[], inproceedingNames : string[], year : number) : string[] {
    const lines: string[] = new Array();
    lines.push(`## Proceedings  `);
    lines.push(`  `);

    inproceedingNames.forEach((v) =>{

        lines.push(`### ${v} ${year}  `);
        _inproceedings_papers.filter((w) => w.booktitle == v).forEach((w, i) =>{
            if(w.ee.length > 0){
                lines.push(`  ${i+1}. [${w.title}](${w.ee[0]})  `);
            }else{
                lines.push(`  ${i+1}. ${w.title}  `);
            }
        })
        lines.push(`  `);
    })
    return lines;
}
function createArticleMD(_article_papers: DBLPArticle[], articleNames : string[], year : number) : string[] {
    const lines: string[] = new Array();
    lines.push(`## Journals  `);
    lines.push(`  `);

    articleNames.forEach((v) =>{
        lines.push(`### ${v} ${year}  `);
        _article_papers.filter((w) => w.booktitle == v).forEach((w, i) =>{
            if(w.ee.length > 0){
                lines.push(`  ${i+1}. [${DBLPElement.get_sanitized_title(w.title)}](${w.ee[0]})  `);
            }else{
                lines.push(`  ${i+1}. ${DBLPElement.get_sanitized_title(w.title)}  `);
            }
        })
        lines.push(`  `);

    })
    return lines;
}


function createMD(_papers: DBLPElementClass[], year: number) : string[] {

    const lines: string[] = new Array();

    const thisYearPapers : DBLPElementClass[] = _papers.filter((v) => v.year == year);
    const articlePapers : DBLPArticle[] = <DBLPArticle[]>thisYearPapers.filter((v) => v instanceof DBLPArticle);
    const inproceedingPapers : DBLPInproceedings[] = <DBLPInproceedings[]>thisYearPapers.filter((v) => v instanceof DBLPInproceedings);

    const articleNames : string[] = [...new Set(articlePapers.map((v) => v.booktitle))];
    const inproceedingNames : string[] = [...new Set(inproceedingPapers.map((v) => v.booktitle))];

    articleNames.sort((a,b) => {
        return a < b ? -1 : 1;
    })
    inproceedingNames.sort((a,b) => {
        return a < b ? -1 : 1;
    })

    lines.push(`# Conference List for Stringologist (${year})`);
    lines.push(`  `);
    inproceedingNames.forEach((v) => {
        let s = `[${v}](#${v.toLocaleLowerCase()}-${year})  `;
        lines.push(s)
    })
    lines.push(`# Journal List for Stringologist (${year})`);
    lines.push(`  `);
    articleNames.forEach((v) => {
        let s = `[${v}](#${v.toLocaleLowerCase()}-${year})  `;
        lines.push(s)
    })

    createInproceedingsMD(inproceedingPapers, inproceedingNames, year).forEach((v) => lines.push(v));
    createArticleMD(articlePapers, articleNames, year).forEach((v) => lines.push(v));


    return lines;
}

function createArxivYearMonthMD(_papers: DBLPArticle[], year : number, monthMap : Map<string, number>) : string[] {
    const lines: string[] = new Array();
    for(var i = 12; i >= 1 ;i--){
        const list = _papers.filter((v) => v.year == year && monthMap.has(v.ee[0]) && monthMap.get(v.ee[0])! == i);
        if(list.length > 0){
            lines.push(`### ${year}/${i}  `);
            list.forEach((v, x) =>{
                lines.push(`  ${x+1}. [${DBLPElement.get_sanitized_title(v.title)}](${v.ee[0]})  `);
            })
            lines.push(`  `);

        }
    }
    return lines;
}


function createArxivMD(_papers: DBLPArticle[], arxivArticles : ArxivArticle[]) : string[] {

    const monthSet = new Map<string, number>();
    arxivArticles.forEach((v) =>{
        monthSet.set(v.url, v.date.getMonth() + 1);
    })

    const lines: string[] = new Array();
    lines.push(`# arXiv Papers  `)
    
    const yearList : number[] = [...new Set(_papers.map((v) => v.year ))];
    yearList.sort((a, b) =>{
        return a > b ? -1 : 1;
    })
    yearList.forEach((year) =>{
        lines.push(`## ${year}  `)
        const spapers = _papers.filter((v) => v.year == year);
        createArxivYearMonthMD(spapers, year, monthSet).forEach((v) => lines.push(v));
        lines.push(`  `);
    })
    return lines;

}


const stringology_dblp_raw_text = fs.readFileSync("data/stringology_dblp.xml", 'utf8');
const doc = new DOMParser().parseFromString(stringology_dblp_raw_text,'text/xml');
const dblpElements = DBLPElement.parseFromXML(doc);

//dblpElements.forEach((v) => console.log(v));

const yearList : number[] = [...new Set(dblpElements.map((v) => v.year))];

yearList.forEach((year) =>{
    const mdlines = createMD(dblpElements, year);
    const text = mdlines.join("\r\n");

    try {
        fs.writeFileSync(`docs/output/list_${year}.md`, text);
        console.log(`Outputted markdown file for ${year}`);

    } catch (e) {
        console.log(e);
    }
})

const arxivArticles = ArxivArticle.loadArxivArticles("data/arxiv.xml");
const arxivPapers = <DBLPArticle[]>dblpElements.filter((v) => v.booktitle == "CoRR");
const arxivMDText = createArxivMD(arxivPapers, arxivArticles).join("\r\n");
try {
    fs.writeFileSync(`docs/output/arxiv_list.md`, arxivMDText);
    console.log(`Outputted markdown file for arXiv`);

} catch (e) {
    console.log(e);
}
