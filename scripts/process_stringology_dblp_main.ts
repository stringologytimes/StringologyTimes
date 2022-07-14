import { DOMParser } from 'xmldom'
import * as fs from 'fs'
import { DBLPArticle, DBLPElement, DBLPInproceedings, DBLPElementClass } from "./basic_functions/dblp_element"
import { ArxivArticle } from "./basic_functions/arxiv_xml"

import {write_list_year_md, write_complete_md, write_arxiv_list_md, append_registered_papers_info, writeGenrePaperNumberPerYearFile} from "./output_functions/md_output_functions"


const stringology_dblp_raw_text = fs.readFileSync("data/stringology_dblp.xml", 'utf8');
const doc = new DOMParser().parseFromString(stringology_dblp_raw_text, 'text/xml');
const dblpElements = DBLPElement.parseFromXML(doc);
const arxivArticles = ArxivArticle.loadArxivArticles("data/arxiv.xml");

write_list_year_md([2019, 2020, 2021, 2022], dblpElements, "docs/output/list");
write_complete_md(dblpElements, `docs/output/complete_list.md`);
write_arxiv_list_md(dblpElements, arxivArticles, `docs/output/arxiv_list.md`);
append_registered_papers_info(dblpElements, arxivArticles, `data/stringology_times_history.csv`);
writeGenrePaperNumberPerYearFile(dblpElements, arxivArticles, `data/paper_statistics_for_each_year.csv`);