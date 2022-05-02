



export type DBLPElementType = 'article' | 'inproceedings' | 'proceedings'



export class DBLPInproceedings {
    public authors: string[] = [];
    public title: string = "";
    public year: number = 0;
    public booktitle: string = "";
    public ee: string[] = [];
    //public type: DBLPElementType;
    //public mdate: Date;
}

export class DBLPArticle {
    public authors: string[] = [];
    public title: string = "";
    public year: number = 0;
    public booktitle: string = "";
    public ee: string[] = [];
    //public month = 1;
    //public type: DBLPElementType;
    //public mdate: Date;
}
export class DBLPProceedings {
    public editors: string[] = [];
    public title: string = "";
    public year: number = 0;
    public booktitle: string = "";
    public ee: string[] = [];
    //public mdate: Date;
}
export type DBLPElementClass = DBLPInproceedings | DBLPArticle | DBLPProceedings;


export class DBLPElement {

    private static parseFromXMLChild(node: Element): DBLPElementClass {

        const children = node.childNodes;
        const type = <DBLPElementType>node.nodeName;
        if (type == "article") {
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
                        el.booktitle = child.textContent!;
                    } else if (child.nodeName == "ee") {
                        el.ee.push(this.get_formal_ee(child.textContent!));
                    }
                }
            }
            return el;
        } else if (type == "inproceedings") {
            var elA = new DBLPInproceedings();
            for (var i = 0; i < children.length; i++) {
                const child = children.item(i);
                if (child.nodeType == child.ELEMENT_NODE) {
                    if (child.nodeName == "author") {
                        elA.authors.push(child.textContent!);
                    } else if (child.nodeName == "title") {
                        elA.title = child.textContent!;
                    } else if (child.nodeName == "year") {
                        elA.year = Number.parseInt(child.textContent!);
                    } else if (child.nodeName == "booktitle") {
                        elA.booktitle = child.textContent!;
                    } else if (child.nodeName == "ee") {
                        elA.ee.push(this.get_formal_ee(child.textContent!));
                    }
                }
            }
            return elA;

        } else if (type == "proceedings") {
            var el2: DBLPProceedings = new DBLPProceedings();
            for (var i = 0; i < children.length; i++) {
                const child = children.item(i);
                if (child.nodeType == child.ELEMENT_NODE) {
                    if (child.nodeName == "author") {
                        el2.editors.push(child.textContent!);
                    } else if (child.nodeName == "title") {
                        el2.title = child.textContent!;
                    } else if (child.nodeName == "year") {
                        el2.year = Number.parseInt(child.textContent!);
                    } else if (child.nodeName == "booktitle") {
                        el2.booktitle = child.textContent!;
                    } else if (child.nodeName == "ee") {
                        el2.ee.push(this.get_formal_ee(child.textContent!));
                    }
                }
            }
            return el2;
        }
        else {
            console.log(type);
            throw new Error("Error");
        }


    }
    private static get_formal_ee(ee : string){
        if(ee.indexOf("http://arxiv.org/") == 0){
            return "https" + ee.substring(4);
        }else if (ee.indexOf("http://arxiv.org/") == 0){
            return ee;
        }else{
            return ee;
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
    public static get_sanitized_title(title : string) : string{

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
                r.push(p);
            }
        }
        return r;
    }

}
