
import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "../basic_functions/dblp_element"
import { ArxivArticle } from "../basic_functions/arxiv_xml"
import { stringToShieldsColor } from "../basic_functions/string_to_color"

function getNormalizedURL(paper : DBLPArticle) : string {
    return paper.ee[0].replace("doi.org/10.48550/arXiv.", "arxiv.org/abs/");
}
function createArxivYearMonthMD(_papers: ArxivArticle[], year : number, monthMap : Map<string, number>, doi_tag_mapper : Map<string, Set<string>>) : string[] {
    const lines: string[] = new Array();
    for(var i = 12; i >= 1 ;i--){
        const list = _papers.filter((v) => v.date.getFullYear() == year && monthMap.has(v.url ) && monthMap.get(v.url)! == i);

        if(list.length > 0){
            lines.push(`### ${year}/${i}  `);
            list.forEach((v, x) =>{
                lines.push(`  ${x+1}. [${DBLPElement.get_sanitized_title(v.title)}](${v.url})  `);
                if(doi_tag_mapper.has(v.doi)){
                    const tags = Array.from(doi_tag_mapper.get(v.doi)!);
                    const tag_str_array = new Array();
                    tags.forEach((tag) => {
                        tag_str_array.push(`![${tag}](https://img.shields.io/badge/${encodeURIComponent(tag)}-${stringToShieldsColor(tag)})`);
                    })
                    lines.push(`    ${tag_str_array.join(" ")}  `);
                }
                
            })
            lines.push(`  `);

        }
    }
    return lines;
}


export function createArxivMD(arxivArticles : ArxivArticle[], doi_tag_mapper : Map<string, Set<string>>) : string[] {

    const monthSet = new Map<string, number>();
    arxivArticles.forEach((v) =>{
        monthSet.set(v.url, v.date.getMonth() + 1);
    })


    const lines: string[] = new Array();
    lines.push(`# arXiv Papers  `)
    
    const yearList : number[] = [...new Set(arxivArticles.map((v) => v.date.getFullYear() ))];
    yearList.sort((a, b) =>{
        return a > b ? -1 : 1;
    })
    yearList.forEach((year) =>{
        lines.push(`## ${year}  `)
        //const spapers = _papers.filter((v) => v.year == year);
        createArxivYearMonthMD(arxivArticles, year, monthSet, doi_tag_mapper).forEach((v) => lines.push(v));
        lines.push(`  `);
    })
    return lines;

}