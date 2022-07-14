



export type DBLPElementType = 'article' | 'inproceedings' | 'proceedings'


function get_formal_ee(ee: string) : string {
    if (ee.indexOf("http://arxiv.org/") == 0) {
        return "https" + ee.substring(4);
    } else if (ee.indexOf("http://arxiv.org/") == 0) {
        return ee;
    } else {
        return ee;
    }
}
export class DBLPInproceedings {
    public authors: string[] = [];
    public title: string = "";
    public year: number = 0;
    public booktitle: string = "";
    public ee: string[] = [];
    public url: string;

    public get proceedingsURL() : string{
        const x = this.url.split("#");
        return `https://dblp.org/${x[0]}`;
    }
    public static parseFromXMLInproceedings(node: Element): DBLPInproceedings {
        const children = node.childNodes;

        var el = new DBLPInproceedings();
        for (var i = 0; i < children.length; i++) {
            const child = children.item(i);
            if (child.nodeType == child.ELEMENT_NODE) {
                if (child.nodeName == "author") {
                    el.authors.push(child.textContent!);
                } else if (child.nodeName == "title") {
                    el.title = child.textContent!;
                } else if (child.nodeName == "year") {
                    el.year = Number.parseInt(child.textContent!);
                } else if (child.nodeName == "booktitle") {
                    el.booktitle = child.textContent!;
                } else if (child.nodeName == "ee") {
                    el.ee.push(get_formal_ee(child.textContent!));
                } else if (child.nodeName == "url") {
                    el.url = child.textContent!;
                }
            }
        }
        return el;
    }
}

export class DBLPArticle {
    public authors: string[] = [];
    public title: string = "";
    public year: number = 0;
    public journal: string = "";
    public ee: string[] = [];
    public url: string;
    public volume: string;
    public get journalURL() : string{
        const x = this.url.split("#");
        return `https://dblp.org/${x[0]}`;
    }
    public static parseFromXMLArticle(node: Element): DBLPArticle {
        const children = node.childNodes;

        var el = new DBLPArticle();
        for (var i = 0; i < children.length; i++) {
            const child = children.item(i);
            if (child.nodeType == child.ELEMENT_NODE) {
                if (child.nodeName == "author") {
                    el.authors.push(child.textContent!);
                } else if (child.nodeName == "title") {
                    el.title = child.textContent!;
                } else if (child.nodeName == "year") {
                    el.year = Number.parseInt(child.textContent!);
                } else if (child.nodeName == "journal") {
                    el.journal = child.textContent!;
                } else if (child.nodeName == "ee") {
                    el.ee.push(get_formal_ee(child.textContent!));
                } else if (child.nodeName == "url") {
                    el.url = child.textContent!;
                } else if (child.nodeName == "volume") {
                    el.volume = child.textContent!;
                }
            }
        }
        return el;
    }

}
export class DBLPProceedings {
    public editors: string[] = [];
    public title: string = "";
    public year: number = 0;
    public booktitle: string = "";
    public ee: string[] = [];
    public url: string;

    public static parseFromXMLProceedings(node: Element): DBLPProceedings {
        const children = node.childNodes;
        var el: DBLPProceedings = new DBLPProceedings();
        for (var i = 0; i < children.length; i++) {
            const child = children.item(i);
            if (child.nodeType == child.ELEMENT_NODE) {
                if (child.nodeName == "author") {
                    el.editors.push(child.textContent!);
                } else if (child.nodeName == "title") {
                    el.title = child.textContent!;
                } else if (child.nodeName == "year") {
                    el.year = Number.parseInt(child.textContent!);
                } else if (child.nodeName == "booktitle") {
                    el.booktitle = child.textContent!;
                } else if (child.nodeName == "ee") {
                    el.ee.push(get_formal_ee(child.textContent!));
                } else if (child.nodeName == "url") {
                    el.url = child.textContent!;
                }
            }
        }
        return el;
    }
}
export type DBLPElementClass = DBLPInproceedings | DBLPArticle | DBLPProceedings;


export class DBLPElement {

    
    
    


    private static parseFromXMLChild(node: Element): DBLPElementClass | null {

        const type = <DBLPElementType>node.nodeName;
        if (type == "article") {
            const el = DBLPArticle.parseFromXMLArticle(node);
            return el;
        } else if (type == "inproceedings") {
            const el = DBLPInproceedings.parseFromXMLInproceedings(node);
            return el;

        } else if (type == "proceedings") {
            const el: DBLPProceedings = DBLPProceedings.parseFromXMLProceedings(node);
            return el;
        }
        else {
            console.log(type);
            return null;
            //throw new Error("Error");
        }


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
    public static get_sanitized_title(title: string): string {

        let r = title.replace(/\r?\n/g, '');
        r = r.replace(/\|/g, '│');
        return r;
    }
    public static parseFromXML(doc: Document): DBLPElementClass[] {

        var r: DBLPElementClass[] = [];
        const root = DBLPElement.get_root(doc);

        const children = root.childNodes;
        for (var i = 0; i < children.length; i++) {
            const child = children.item(i);
            if (child.nodeType == child.ELEMENT_NODE) {
                const p = DBLPElement.parseFromXMLChild(<Element>child);
                if(p != null){
                    r.push(p);
                }
            }
        }
        return r;
    }

}
