
import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "../basic_functions/dblp_element"
import { ArxivArticle } from "../basic_functions/arxiv_xml"

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


export function createArxivMD(_papers: DBLPArticle[], arxivArticles : ArxivArticle[]) : string[] {

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