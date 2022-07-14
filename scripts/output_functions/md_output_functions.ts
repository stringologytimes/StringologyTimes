import * as fs from 'fs'
import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "../basic_functions/dblp_element"
import { ArxivArticle } from "../basic_functions/arxiv_xml"
import { createArxivMD } from "./create_arxiv_md"
import { createCompleteMD, createMD } from "./create_list_md"

export function write_list_year_md(yearList: number[], dblpElements: DBLPElementClass[], outputFilePathPrefix: string): void {
    yearList.forEach((year) => {
        const mdlines = createMD(dblpElements, year);
        const text = mdlines.join("\r\n");

        try {
            fs.writeFileSync(`${outputFilePathPrefix}_${year}.md`, text);
            console.log(`Outputted markdown file for ${year}`);

        } catch (e) {
            console.log(e);
        }
    })
}
export function write_complete_md(dblpElements: DBLPElementClass[], outputFilePath: string): void {
    const complete_MD_lines = createCompleteMD(dblpElements);
    const complete_MD_text = complete_MD_lines.join("\r\n");
    try {
        fs.writeFileSync(outputFilePath, complete_MD_text);
        console.log(`Outputted markdown file for the complete list`);

    } catch (e) {
        console.log(e);
    }
}

export function write_arxiv_list_md(dblpElements: DBLPElementClass[], arxivArticles: ArxivArticle[], outputFilePath: string): void {
    const arxivPapers = (<DBLPArticle[]>dblpElements.filter((v) => v instanceof DBLPArticle)).filter((v) => v.journal == "CoRR");
    const arxivMDText = createArxivMD(arxivPapers, arxivArticles).join("\r\n");
    try {
        fs.writeFileSync(outputFilePath, arxivMDText);
        console.log(`Outputted markdown file for arXiv`);

    } catch (e) {
        console.log(e);
    }
}

export function append_registered_papers_info(dblpElements: DBLPElementClass[], arxivArticles: ArxivArticle[], outputFilePath: string): void {
    const inproceedings = <DBLPInproceedings[]>dblpElements.filter((v) => v instanceof DBLPInproceedings);
    const articlesExcludingArxiv = (<DBLPArticle[]>dblpElements.filter((v) => v instanceof DBLPArticle)).filter((v) => v.journal != "CoRR");
    const arxivPapers = (<DBLPArticle[]>dblpElements.filter((v) => v instanceof DBLPArticle)).filter((v) => v.journal == "CoRR");

    console.log("The number of dblp elements: " + dblpElements.length);
    console.log("The number of arXiv papers: " + arxivPapers.length);
    console.log("The number of proceedings papers: " + inproceedings.length);
    console.log("The number of journal papers: " + articlesExcludingArxiv.length);
    const totalPaperNumber = arxivPapers.length + inproceedings.length + articlesExcludingArxiv.length;

    const date = new Date();
    const Y = date.getFullYear()
    const M = ("00" + (date.getMonth() + 1)).slice(-2)
    const D = ("00" + date.getDate()).slice(-2)

    const history_line = `${Y}${M}${D}, ${totalPaperNumber}, ${inproceedings.length}, ${arxivPapers.length}, ${articlesExcludingArxiv.length} \n`;

    const history_file_exist = fs.existsSync(outputFilePath);
    const firstLine = "Date, papers, inproceedings papers, arxiv papers, journal papers \n";

    fs.appendFile(outputFilePath, history_file_exist ? history_line : (firstLine + history_line), (err) => {
        if (err) throw err;
        console.log('docs/output/stringology_times_history.csvに追記されました');
    });

}

export function writeGenrePaperNumberPerYearFile(dblpElements: DBLPElementClass[], arxivArticles: ArxivArticle[], outputFilePath: string) {
    let dblpYearMin = 9999;
    let dblpYearMax = 0;

    dblpElements.forEach((v) => { dblpYearMin = Math.min(v.year, dblpYearMin); dblpYearMax = Math.max(v.year, dblpYearMax) })
    const dblpYearList: number[] = new Array(dblpYearMax - dblpYearMin + 1);
    const inproceedingsYearList: number[] = new Array(dblpYearMax - dblpYearMin + 1);
    const journalPaperYearList: number[] = new Array(dblpYearMax - dblpYearMin + 1);
    const arxivPaperYearList: number[] = new Array(dblpYearMax - dblpYearMin + 1);


    for (let i = 0; i < dblpYearList.length; i++) {
        dblpYearList[i] = 0;
        inproceedingsYearList[i] = 0;
        journalPaperYearList[i] = 0;
        arxivPaperYearList[i] = 0;
    }
    dblpElements.forEach((v) => dblpYearList[v.year - dblpYearMin]++);

    const inproceedings = <DBLPInproceedings[]>dblpElements.filter((v) => v instanceof DBLPInproceedings);
    inproceedings.forEach((v) => inproceedingsYearList[v.year - dblpYearMin]++);

    const journalPapers = (<DBLPArticle[]>dblpElements.filter((v) => v instanceof DBLPArticle)).filter((v) => v.journal != "CoRR");
    journalPapers.forEach((v) => journalPaperYearList[v.year - dblpYearMin]++);

    const arxivPapers = (<DBLPArticle[]>dblpElements.filter((v) => v instanceof DBLPArticle)).filter((v) => v.journal == "CoRR");
    arxivPapers.forEach((v) => arxivPaperYearList[v.year - dblpYearMin]++);

    const outputLineStrings: string[][] = new Array();
    outputLineStrings.push(["Year", "papers", "proceedings papers", "arxiv papers", "journal papers"]);
    for (let i = 0; i < dblpYearList.length; i++) {
        outputLineStrings.push([(i + dblpYearMin).toString(), dblpYearList[i].toString(), inproceedingsYearList[i].toString(), arxivPaperYearList[i].toString(), journalPaperYearList[i].toString()]);
    }

    let outputString = "";
    outputLineStrings.forEach((v) => {
        outputString += v.join(", ");
        outputString += "\n";
    })

    try {
        fs.writeFileSync(outputFilePath, outputString);
        console.log(`Outputted GenrePaperNumberPerYearFile`);

    } catch (e) {
        console.log(e);
    }
}