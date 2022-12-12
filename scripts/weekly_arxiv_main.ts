import * as fs from 'fs'

type ArxivSimpleArticle = {
    title: string,
    id: string;
    year: number;
    month: number;
    day: number;
}


class WeekArticleList {
    year: number;
    month: number;
    weekNumber: number;
    articles: ArxivSimpleArticle[] = new Array();

    constructor() {
        this.year = 0;
        this.month = 0;
        this.weekNumber = 0;
    }

    public getHashKey(): string {
        return WeekArticleList.createHashKey(this.year, this.month, (this.weekNumber + 1) * 7);
    }

    public static createHashKey(year: number, month: number, day: number): string {
        const _month = month < 10 ? `0${month}` : month.toString();
        const _weekNumber = Math.floor((day - 1) / 7);

        return `${year}${_month}${_weekNumber}`;
    }
    public static getWeekNumber(day : number) : number{
        const _weekNumber = Math.floor((day - 1) / 7);
        return _weekNumber;

    }
    public static getDaysString(weekNumber: number) : string{
        var maxDay = Math.min(31, ((weekNumber+1)*7));
        return `${(weekNumber*7)+1}-${maxDay}`;
    }
    public static build(year : number, month : number, weekNumber : number, articles: ArxivSimpleArticle[]) : WeekArticleList{
        const list = new WeekArticleList();
        list.year = year;
        list.month = month;
        list.weekNumber = weekNumber;
        articles.forEach((v) =>{
            if(list.year == v.year && list.month == v.month && list.weekNumber == WeekArticleList.getWeekNumber(v.day)){
                list.articles.push(v);
            }
        })
        return list;
    }

}
class WeekArticleListByMonth{
    year: number;
    month: number;
    lists : WeekArticleList[];
    constructor() {
        this.year = 0;
        this.month = 0;
        this.lists = new Array();
    }
    public static build(year : number, month : number, articles: ArxivSimpleArticle[]) : WeekArticleListByMonth{
        const list = new WeekArticleListByMonth();
        list.year = year;
        list.month = month;
        for(let i = 0;i<5;i++){
            const weekList = WeekArticleList.build(year, month, i, articles);
            list.lists.push(weekList);
        }
        return list;
        
    }

}
class WeekArticleListByYear{
    year: number;
    lists: WeekArticleListByMonth[];
    constructor(){
        this.year = 0;
        this.lists = new Array();
    }
    public static build(year : number, minMonth : number, maxMonth : number, articles: ArxivSimpleArticle[]) : WeekArticleListByYear{
        const list = new WeekArticleListByYear();
        list.year = year;
        for(let i = minMonth;i<=maxMonth;i++){
            const monthArticles = articles.filter((v) => v.month == i);
            const monthList = WeekArticleListByMonth.build(year, i, monthArticles);
            list.lists.push(monthList);
        }
        return list;
        
    }
}
class WeekArticleSuperList {
    lists : WeekArticleListByYear[] = new Array();

    public static build(articles: ArxivSimpleArticle[]) : WeekArticleSuperList{
        const r = new WeekArticleSuperList();
        //const map: Map<string, WeekArticleList> = new Map();
        let minYear = 2022;
        let minMonth = 12;
        let maxYear = 0;
        let maxMonth = 0;
        
        articles.forEach((v) => {
            if (v.year < minYear) {
                minYear = v.year;
            }
            if (v.year == minYear && v.month < minMonth) {
                minMonth = v.month;
            }
            if (v.year > maxYear) {
                maxYear = v.year;
            }
            if (v.year == maxYear && v.month > maxMonth) {
                maxMonth = v.month;
            }
        })
        for (let y = minYear; y <= maxYear; y++) {
            const monthEnd = y == maxYear ? maxMonth : 12;
            const monthStart = y == minYear ? minMonth : 1;
            const yearArticles = articles.filter((v) => v.year == y);
            const yearList = WeekArticleListByYear.build(y, monthStart, monthEnd, yearArticles);
            r.lists.push(yearList);
        }

        return r;

    }
}


function write_weekly_arxiv_list_sub(list : WeekArticleList, outputFolderPath: string) {
    const lines : string[] = new Array();
    lines.push(`# Data Structures and Algorithms: ${list.year}/${list.month}/${WeekArticleList.getDaysString(list.weekNumber)}  `);

    list.articles.forEach((v, i) =>{
        lines.push(`${i+1}: [${v.title}](https://doi.org/10.48550/arXiv.${v.id})  `)
    })

    const page = lines.join("\n");



    try {
        const mdPath = `${outputFolderPath}/${list.getHashKey()}.md`;
        fs.writeFileSync(mdPath, page );
        console.log(`Outputted ${mdPath}`);

    } catch (e) {
        console.log(e);
    }
}

function write_weekly_arxiv_list(superList : WeekArticleSuperList, outputFolderPath: string, toppagePath : string) {
    if (!fs.existsSync(outputFolderPath)) {
        fs.mkdirSync(outputFolderPath);
    }

    const lines : string[] = new Array();
    lines.push(`# Data Structures and Algorithms  `);

    for(let y = superList.lists.length - 1; y >= 0;y--){
        const yearList = superList.lists[y];
        lines.push(`## ${yearList.year}  `);
        yearList.lists.forEach((monthList) =>{            
            monthList.lists.forEach((weekList) =>{
                const monthStr = monthList.month < 10 ? `0${monthList.month}` : `${monthList.month}`;
                const str = `[${monthStr}/${WeekArticleList.getDaysString(weekList.weekNumber)}](./weekly_arxiv/${weekList.getHashKey()}.md) (${weekList.articles.length} articles)  `
                lines.push(`- ${str}  `);
                write_weekly_arxiv_list_sub(weekList, outputFolderPath); 
            }
            )
        })

    }
    const page = lines.join("\n")
    try {
        const mdPath = `${toppagePath}`;
        fs.writeFileSync(mdPath, page);
        console.log(`Outputted ${mdPath}`);

    } catch (e) {
        console.log(e);
    }


    /*
    weeklyArticleListArray.forEach((list) => {
        write_weekly_arxiv_list_sub(list, outputFolderPath);    
    })
    */
    
}


const tsv: string = fs.readFileSync("data/cs.DS_arxiv_articles.tsv", 'utf8');
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



/*
articles.forEach((v) => {
    const key = WeekArticleList.createHashKey(v.year, v.month, v.day);
    const list = map.get(key);
    if(list == null){
        throw new Error(`Error: ${key}`);
    }else{
        list.articles.push(v);
    }
})

const weeklyArticleListArray : WeekArticleList[] = new Array();

map.forEach((value, key) => {
    weeklyArticleListArray.push(value);
})
weeklyArticleListArray.sort((a,b) => 
{
    if(a.getHashKey() <= b.getHashKey()){
        return -1;
    }else{
        return 1;
    }
}
);
*/

write_weekly_arxiv_list(superList, "docs/output/weekly_arxiv", "docs/output/weekly_arxiv_top.md")



