import { DOMParser } from 'xmldom'
import * as fs from 'fs'
import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "./basic_functions/dblp_element"
import { ArxivArticle } from "./basic_functions/arxiv_xml"

import {write_list_year_md, write_complete_md, write_arxiv_list_md, append_registered_papers_info, writeGenrePaperNumberPerYearFile, write_list_by_book} from "./output_functions/md_output_functions"


const stringology_dblp_raw_text = fs.readFileSync("data/stringology_dblp.xml", 'utf8');
const doc = new DOMParser().parseFromString(stringology_dblp_raw_text, 'text/xml');
const dblpElements = DBLPElement.parseFromXML(doc);
const arxivArticles = ArxivArticle.loadArxivArticles("data/arxiv.xml");

const yearList : number[] = [];
for(let i= 2010;i<=2022;i++){
    yearList.push(i);
}


write_list_year_md(yearList, dblpElements, "docs/output/list");
write_complete_md(dblpElements, `docs/output/complete_list.md`);

write_list_by_book(dblpElements, `docs/output/proceedings`, `docs/output/list_of_proceedings.md`);

write_arxiv_list_md(arxivArticles, `docs/output/arxiv_list.md`);
append_registered_papers_info(dblpElements, arxivArticles, `data/stringology_times_history.csv`);
writeGenrePaperNumberPerYearFile(dblpElements, arxivArticles, `data/paper_statistics_for_each_year.csv`);