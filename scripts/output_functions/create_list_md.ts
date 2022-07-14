import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "../basic_functions/dblp_element"

type ProceedingsInfo = {
    name: string,
    inproceedings: DBLPInproceedings[];
    dblp_url: string;
}
type JournalInfo = {
    name: string,
    articles: DBLPArticle[];
    dblp_url: string;
}
type YearPaperCollection = {
    year: number,
    papers: DBLPElementClass[];
}

function createInproceedingsMD(_proceedings: ProceedingsInfo[], year: number): string[] {
    const lines: string[] = new Array();
    //lines.push(`### Proceedings  `);
    //lines.push(`  `);

    _proceedings.forEach((v) => {
        lines.push(`#### [${v.name} ${year}](${v.dblp_url})`);
        v.inproceedings.forEach((w, i) => {
            if (w.ee.length > 0) {
                lines.push(`  ${i + 1}. [${w.title}](${w.ee[0]})  `);
            } else {
                lines.push(`  ${i + 1}. ${w.title}  `);
            }
        })
        lines.push(`  `);
    })
    return lines;
}
function createArticleMD(journals: JournalInfo[], year: number): string[] {
    const lines: string[] = new Array();
    //lines.push(`### Journals  `);
    //lines.push(`  `);

    journals.forEach((v) => {
        lines.push(`#### ${v.name} ${year}  `);
        v.articles.forEach((w, i) => {
            if (w.ee.length > 0) {
                lines.push(`  ${i + 1}. [${DBLPElement.get_sanitized_title(w.title)}](${w.ee[0]})  `);
            } else {
                lines.push(`  ${i + 1}. ${DBLPElement.get_sanitized_title(w.title)}  `);
            }
        })
        lines.push(`  `);

    })
    return lines;
}


function get_sorted_proceedings(papers: DBLPInproceedings[], year: number): ProceedingsInfo[] {
    const inproceedingPapers: DBLPInproceedings[] = papers.filter((v) => v.year == year);
    const mapper = new Map<string, ProceedingsInfo>();
    inproceedingPapers.forEach((v) => {
        const x = mapper.get(v.booktitle);
        if (x == null) {
            mapper.set(v.booktitle, { name: v.booktitle, inproceedings: [v], dblp_url: v.proceedingsURL });
        } else {
            x.inproceedings.push(v);
        }
    })
    const list: ProceedingsInfo[] = [];
    mapper.forEach((a, b) => list.push(a));
    list.sort((a, b) => {
        return a.name < b.name ? -1 : 1;
    })

    list.forEach((v) =>{
        v.inproceedings.sort((a, b) =>{
            return a.title < b.title ? -1 : 1;
        })
    })
    return list;
}
function get_sorted_journals(papers: DBLPArticle[], year: number): JournalInfo[] {
    const articlePapers: DBLPArticle[] = papers.filter((v) => v.year == year);
    const mapper = new Map<string, JournalInfo>();
    articlePapers.forEach((v) => {
        const x = mapper.get(v.journal);
        if (x == null) {
            mapper.set(v.journal, { name: v.journal, articles: [v], dblp_url: v.journalURL });
        } else {
            x.articles.push(v);
        }
    })
    const list: JournalInfo[] = [];
    mapper.forEach((a, b) => list.push(a));
    list.sort((a, b) => {
        return a.name < b.name ? -1 : 1;
    })
    list.forEach((v) =>{
        v.articles.sort((a, b) =>{
            return a.title < b.title ? -1 : 1;
        })
    })

    return list;
}
function get_sorted_papers_by_year(papers: DBLPElementClass[]): YearPaperCollection[] {
    const mapper = new Map<number, YearPaperCollection>();
    papers.forEach((v) => {
        const x = mapper.get(v.year);
        if (x == null) {
            mapper.set(v.year, { year: v.year, papers: [v] });
        } else {
            x.papers.push(v);
        }
    })
    const list: YearPaperCollection[] = [];
    mapper.forEach((a, b) => list.push(a));
    list.sort((a, b) => {
        return a.year > b.year ? -1 : 1;
    })
    return list;

}



function create_overview_year(item: YearPaperCollection): string[] {
    const lines: string[] = new Array();

    const inproceedings = <DBLPInproceedings[]>item.papers.filter((w) => w instanceof DBLPInproceedings);
    const sorted_inproceedings = get_sorted_proceedings(inproceedings, item.year);
    const journals = <DBLPArticle[]>item.papers.filter((w) => w instanceof DBLPArticle);
    const sorted_journals = get_sorted_journals(journals, item.year);

    //lines.push(`+ ${item.year}  `);
    //lines.push(`### Proceedings (${item.year})`);
    lines.push(`  `);
    sorted_inproceedings.forEach((w) => {
        let s = `- [${w.name}(${item.year})](#${w.name.toLocaleLowerCase()}-${item.year}) [\[dblp\]](${w.dblp_url})  `;
        lines.push(s)
    })
    //lines.push(`### Journals (${item.year})`);
    lines.push(`  `);
    sorted_journals.forEach((w) => {
        let s = `- [${w.name}(${item.year})](#${w.name.toLocaleLowerCase()}-${item.year}) [\[dblp\]](${w.dblp_url})  `;
        lines.push(s)
    })
    lines.push(`  `);
    return lines;

}




function create_overview(_papers: DBLPElementClass[]): string[] {
    const lines: string[] = new Array();
    lines.push("## Table of Contents")

    const sorted_papers_by_year = get_sorted_papers_by_year(_papers);
    sorted_papers_by_year.forEach((v) => {
        const sub_lines = create_overview_year(v);
        sub_lines.forEach((w) => lines.push(w));
    })
    return lines;
}


export function createCompleteMD(_papers: DBLPElementClass[]): string[] {
    const lines: string[] = new Array();
    lines.push(`# Papers for stringologist`)

    const overview_lines = create_overview(_papers);
    overview_lines.forEach((v) => lines.push(v));

    const sorted_papers_by_year = get_sorted_papers_by_year(_papers);
    lines.push("## Contents")
    sorted_papers_by_year.forEach((v) => {
        const inproceedings = <DBLPInproceedings[]>v.papers.filter((w) => w instanceof DBLPInproceedings);
        const sorted_inproceedings = get_sorted_proceedings(inproceedings, v.year);
        const journals = <DBLPArticle[]>v.papers.filter((w) => w instanceof DBLPArticle);
        const sorted_journals = get_sorted_journals(journals, v.year);

        createInproceedingsMD(sorted_inproceedings, v.year).forEach((v) => lines.push(v));
        createArticleMD(sorted_journals, v.year).forEach((v) => lines.push(v));
    })

    return lines;

}


export function createMD(papers: DBLPElementClass[], year : number): string[] {
    const spapers = get_sorted_papers_by_year(papers);
    const xp = spapers.filter((w) => w.year == year); 
    const lines: string[] = new Array();
    lines.push(`# Papers for stringologist (${year})`)

    if(xp.length > 0){
        const item = xp[0];

        const overview_lines = create_overview_year(item);
        overview_lines.forEach((v) => lines.push(v));
    
        const inproceedings = <DBLPInproceedings[]>item.papers.filter((w) => w instanceof DBLPInproceedings);
        const sorted_inproceedings = get_sorted_proceedings(inproceedings, item.year);
        const journals = <DBLPArticle[]>item.papers.filter((w) => w instanceof DBLPArticle);
        const sorted_journals = get_sorted_journals(journals, item.year);

        lines.push("## Contents")
        createInproceedingsMD(sorted_inproceedings, item.year).forEach((v) => lines.push(v));
        createArticleMD(sorted_journals, item.year).forEach((v) => lines.push(v));
    
    }


    return lines;
}
