import * as fs from 'fs'
import { ArxivSimpleArticle, WeekArticleSuperList, write_weekly_arxiv_list } from './weekly_arxiv/week_article_list'

/**
 * This script is used to generate the weekly arxiv list.
 */

const tsv: string = fs.readFileSync("data/auto_generated/cs.DS_arxiv_articles.tsv", 'utf8');
const articles: ArxivSimpleArticle[] = new Array();
tsv.split("\n").forEach((line) => {
    const line2 = line.split("\t");
    const title = line2[2];
    const id = line2[1];
    const timeArr = line2[0].split("-");
    const year = Number.parseInt(timeArr[0]);
    const month = Number.parseInt(timeArr[1]);
    const day = Number.parseInt(timeArr[2]);
    articles.push({ title: title, id: id, year: year, month: month, day: day });
})

const superList = WeekArticleSuperList.build(articles);


write_weekly_arxiv_list(superList, "docs/output/weekly_arxiv", "docs/output/weekly_arxiv_top.md")



