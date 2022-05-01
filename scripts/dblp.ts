
import { PaperArticle } from "./article"
import { DOMParser } from 'xmldom'
const fs = require('fs');


function TrimRightmostDot(title: string): string {
    const p = title.lastIndexOf(".");
    if (p != -1) {
        return title.substring(0, p);
    } else {
        return title;
    }
}
function getCorrectTitle(title: string): string {
    title = title.replace(/ \- /g, ': ');
    return TrimRightmostDot(title);
}

type DBLPElementType = 'article' | 'inproceedings' | 'proceedings'
export class DBLPElement {
    public authors: string[] = [];
    public title: string = "";
    public year: number = 0;
    public booktitle: string = "";
    public ee: string[] = [];
    public type: DBLPElementType;
    public mdate : Date;

    private static parseFromXMLChild(node: Element): DBLPElement {
        var el = new DBLPElement();

        const children = node.childNodes;
        el.type = <DBLPElementType>node.nodeName;
        for (var i = 0; i < children.length; i++) {
            const child = children.item(i);
            if (child.nodeType == child.ELEMENT_NODE) {
                if (child.nodeName == "author") {
                    el.authors.push(child.textContent!);
                } else if (child.nodeName == "title") {
                    el.title = child.textContent!;
                } else if (child.nodeName == "year") {
                    el.year = Number.parseInt(child.textContent!);
                } else if (child.nodeName == "booktitle" || child.nodeName == "journal") {
                    el.booktitle = child.textContent!;
                } else if (child.nodeName == "ee") {
                    el.ee.push(child.textContent!);
                }
            }
            else if(child.nodeType == child.ATTRIBUTE_NODE){
                console.log(child.textContent);
            }
        }
        const mdate = node.getAttribute("mdate");
        if(mdate != null){
            const p = Date.parse(mdate);
            el.mdate = new Date(p);
        }
        
        return el;

    }
    private static get_root(doc: Document): ChildNode {
        const children = doc.childNodes;
        for (var i = 0; i < children.length; i++) {
            const child = children.item(i);
            if (child.nodeName == "dblp") {
                return child;
            }
        }
        throw new Error("Error");

    }
    public static parseFromXML(doc: Document): DBLPElement[] {

        var r: DBLPElement[] = [];
        const root = DBLPElement.get_root(doc);

        const children = root.childNodes;
        for (var i = 0; i < children.length; i++) {
            const child = children.item(i);
            if (child.nodeType == child.ELEMENT_NODE) {
                const p = DBLPElement.parseFromXMLChild(<Element>child);
                r.push(p);
            }
        }
        return r;
    }

}

export class DBLPArticle {
    public static loadJson(conference: string, year: number): PaperArticle[] {
        const filepath = `data/dblp/${conference}_${year}.json`;
        const text = fs.readFileSync(filepath, 'utf8');

        const document = JSON.parse(text);

        const outputArticles: PaperArticle[] = [];

        const collection: any[] = document["result"]["hits"]["hit"];
        if (collection != undefined) {
            collection.forEach((v) => {
                const p = new PaperArticle();
                const info = v["info"];
                p.title = getCorrectTitle(info["title"]);
                p.conferencePaperURL = info["ee"];
                p.conference = conference;
                p.year = year;
                p.doi = info["doi"];
                if (info["authors"] != undefined) {
                    const authorsInfo: any = info["authors"]["author"];

                    if (Array.isArray(authorsInfo)) {
                        p.authors = authorsInfo.map((inf) => inf["text"]);
                    } else {
                        p.authors = [authorsInfo["text"]];
                    }
                    outputArticles.push(p);
                    //console.log(p.toString());

                }

            })
        } else {
            console.log(`Skip: ${conference} ${year}`)
        }
        return outputArticles;

    }
    public static loadPaperArticlesInDBLPFolder(dblpFolderPath: string): PaperArticle[] {
        const jsonList: [string, number][] = [];
        const files = fs.readdirSync(dblpFolderPath)
        files.filter((v) => {
            return fs.statSync(dblpFolderPath + "/" + v).isFile() && /.*\.json$/.test(dblpFolderPath + "/" + v);
        }).forEach((filename) => {
            const p1 = filename.split(".")[0];
            const conferenceName = p1.split("_")[0];
            const year = Number.parseInt(p1.split("_")[1]);
            console.log([conferenceName, year]);

            jsonList.push([conferenceName, year]);
        })

        const hList: PaperArticle[] = [];
        jsonList.forEach(([conf, year]) => {
            DBLPArticle.loadJson(conf, year).forEach((v) => {
                hList.push(v);
            })
        })
        return hList;
    }
    public static loadPaperArticlesInDBLPFolderWithFilter(dblpFolderPath: string, doiListPath: string): PaperArticle[] {
        const hList = this.loadPaperArticlesInDBLPFolder(dblpFolderPath);

        const hListMap: Map<string, number> = new Map();
        hList.forEach((v, i) => {
            if (v.doi != undefined) {
                hListMap.set(v.doi, i);
            } else if (v.conferencePaperURL != undefined) {
                hListMap.set(v.conferencePaperURL, i);
            }
        })

        const doiListText = fs.readFileSync(doiListPath, 'utf8');
        const doiListLines = doiListText.toString().split(/\r\n|\n/);

        const r: PaperArticle[] = new Array();
        doiListLines.forEach((paper_id) => {
            const paper_id_type = PaperArticle.get_paper_id_type(paper_id);
            if (paper_id_type == "arxiv") {

            } else if (paper_id_type == "doi" || paper_id_type == "otherURL") {
                const value = hListMap.get(paper_id);
                if (value != undefined) {
                    r.push(hList[value]);
                } else {
                    if (paper_id_type == "doi") {
                        console.log(`Unhit: https://doi.org/${paper_id}`)
                    } else {
                        console.log(`Unhit: ${paper_id}`)

                    }
                }

            }

        })
        return r;
    }

}