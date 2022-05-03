import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "./dblp_element"

function createInproceedingsMD(_inproceedings_papers: DBLPInproceedings[], inproceedingNames : string[], 
    year : number, map_from_inproceedings_name_to_dblp : Map<string, string>) : string[] {
    const lines: string[] = new Array();
    lines.push(`## Proceedings  `);
    lines.push(`  `);

    inproceedingNames.forEach((v) =>{

        lines.push(`### [${v} ${year}](${map_from_inproceedings_name_to_dblp.get(v)})`);
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
        _article_papers.filter((w) => w.journal == v).forEach((w, i) =>{
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


export function createMD(_papers: DBLPElementClass[], year: number) : string[] {

    const lines: string[] = new Array();

    const thisYearPapers : DBLPElementClass[] = _papers.filter((v) => v.year == year);
    const articlePapers : DBLPArticle[] = <DBLPArticle[]>thisYearPapers.filter((v) => v instanceof DBLPArticle);
    const inproceedingPapers : DBLPInproceedings[] = <DBLPInproceedings[]>thisYearPapers.filter((v) => v instanceof DBLPInproceedings);

    const articleNames : string[] = [...new Set(articlePapers.map((v) => v.journal))];
    const inproceedingNames : string[] = [...new Set(inproceedingPapers.map((v) => v.booktitle))];

    const map_from_inproceedings_name_to_dblp = new Map<string, string>();
    inproceedingPapers.forEach((v) => {
        map_from_inproceedings_name_to_dblp.set(v.booktitle, v.proceedingsURL);
    })

    /*
    const map_from_journal_to_dblp = new Map<string, string>();
    articlePapers.forEach((v) => {

    })
    */


    articleNames.sort((a,b) => {
        return a < b ? -1 : 1;
    })
    inproceedingNames.sort((a,b) => {
        return a < b ? -1 : 1;
    })

    lines.push(`# Conference List for Stringologist (${year})`);
    lines.push(`  `);
    inproceedingNames.forEach((v) => {
        let s = `[${v}](#${v.toLocaleLowerCase()}-${year}) [\[dblp\]](${map_from_inproceedings_name_to_dblp.get(v)})  `;
        lines.push(s)
    })
    lines.push(`# Journal List for Stringologist (${year})`);
    lines.push(`  `);
    articleNames.forEach((v) => {
        let s = `[${v}](#${v.toLocaleLowerCase()}-${year})  `;
        lines.push(s)
    })

    createInproceedingsMD(inproceedingPapers, inproceedingNames, year, map_from_inproceedings_name_to_dblp).forEach((v) => lines.push(v));
    createArticleMD(articlePapers, articleNames, year).forEach((v) => lines.push(v));


    return lines;
}
