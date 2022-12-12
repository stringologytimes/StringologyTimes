import { DOMParser } from 'xmldom'
import * as fs from 'fs' 

export type ArxivXMLInfo = {
    document: XMLDocument,
    dic: Set<string>    
}

export function loadArxivXML(arxivXMLPath: string): ArxivXMLInfo {
    const xml = fs.readFileSync(arxivXMLPath, 'utf8');
    const document = new DOMParser().parseFromString(xml);
    const dic = new Set<string>();
    const info: ArxivXMLInfo = { document: document, dic: dic };
    const entries = document.getElementsByTagName("entry");
    for (let i = 0; i < entries.length; i++) {
        const entry = entries.item(i)!;
        const id = entry.getAttribute("id")!;
        if (!dic.has(id)) {
            dic.add(id);
        } else {
            document.removeChild(entry);
            console.log(`Remove: /${id}/`);
        }
    }
    return info;
}

export class ArxivArticle {
    node: Element;
    date: Date;
    id: string;
    title: string;
    url: string;
    public constructor(_node: Element) {
        this.node = _node;
        const publishedNode: Element | null = this.node.getElementsByTagName("published").item(0)!;
        if(publishedNode == undefined){
            console.log(this.node.textContent);

        }

        const dateStr: string = this.node.getElementsByTagName("published").item(0)!.textContent!;
        this.date = new Date(dateStr);

        this.id = this.node.getAttribute("id")!;
        this.title = this.node.getElementsByTagName("title").item(0)!.textContent!;
        this.url = `https://arxiv.org/abs/${this.id}`;
    }

    public static loadArxivArticles(arxivXMLPath : string): ArxivArticle[] {
        const arxivInfo: ArxivXMLInfo = loadArxivXML(arxivXMLPath);
        const entryCol = arxivInfo.document.getElementsByTagName('entry');
        const arr: ArxivArticle[] = new Array(0);
        for (let i = 0; i < entryCol.length; i++) {
            arr.push(new ArxivArticle(entryCol.item(i)!));
        }
        return arr;
    }
    
}


