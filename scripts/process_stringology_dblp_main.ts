import { DOMParser } from 'xmldom'
import * as fs from 'fs'
import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "./dblp_element"
import { ArxivArticle } from "./arxiv_xml"
import { createArxivMD } from "./create_arxiv_md"
import { createMD } from "./create_list_md"

//const root = doc.getElementsByTagName("dblp").item[0];





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
const arxivPapers = (<DBLPArticle[]>dblpElements.filter((v) => v instanceof DBLPArticle)).filter((v) => v.journal == "CoRR");
const arxivMDText = createArxivMD(arxivPapers, arxivArticles).join("\r\n");
try {
    fs.writeFileSync(`docs/output/arxiv_list.md`, arxivMDText);
    console.log(`Outputted markdown file for arXiv`);

} catch (e) {
    console.log(e);
}
