import { DOMParser } from 'xmldom'
import { DBLPElement } from "./basic_functions/dblp_element"
import { ArxivArticle } from "./basic_functions/arxiv_xml"
import {write_list_year_md, write_complete_md, write_arxiv_list_md, append_registered_papers_info, writeGenrePaperNumberPerYearFile, write_list_by_book} from "./output_functions/md_output_functions"
import * as fs from 'fs' 

const stringology_dblp_raw_text = fs.readFileSync("data/auto_generated/stringology_dblp.xml", 'utf8');
const doc = new DOMParser().parseFromString(stringology_dblp_raw_text, 'text/xml');
const dblpElements = DBLPElement.parseFromXML(doc);
const arxivArticles = ArxivArticle.loadArxivArticles("data/auto_generated/filtered_arxiv.xml");

const yearList : number[] = [];
for(let i= 2010;i<=2035;i++){
    yearList.push(i);
}

const doi_tag_mapper = new Map<string, Set<string>>();
const tag_csv_raw_text = fs.readFileSync("data/auto_generated/tag.csv", 'utf8');
const tag_csv_lines = tag_csv_raw_text.split("\n");
for(let i = 0; i < tag_csv_lines.length; i++){
    const line = tag_csv_lines[i];
    const cols = line.split(",");
    const doi = cols[0].trim();
    const tag = cols[1].trim();
    if(doi_tag_mapper.has(doi)){
        doi_tag_mapper.get(doi)!.add(tag);
    }else{
        doi_tag_mapper.set(doi, new Set<string>([tag]));
    }
}



write_list_year_md(yearList, dblpElements, "docs/output/lists");
write_complete_md(dblpElements, `docs/output/lists`, "complete_list.md");

write_list_by_book(dblpElements, `docs/output/proceedings`, `docs/output/list_of_proceedings.md`);

write_arxiv_list_md(arxivArticles, doi_tag_mapper, `docs/output/arxiv_list.md`);
append_registered_papers_info(dblpElements, arxivArticles, `data/auto_generated/stringology_times_history.csv`);
writeGenrePaperNumberPerYearFile(dblpElements, arxivArticles, `data/auto_generated/paper_statistics_for_each_year.csv`);