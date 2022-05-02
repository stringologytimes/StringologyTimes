



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
