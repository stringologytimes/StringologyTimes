import {node_request_X, dblpElementInfo, RequestCollection} from "./node_request"



function addToDBLPElements(nodes: HTMLCollectionOf<Element>, output: HTMLElement[]) {
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node instanceof HTMLElement) {
            output.push(node);
        }
    }
}
function createCollections(info_list : dblpElementInfo[]): RequestCollection[] {
    const r : RequestCollection[] = [];
    let x = new RequestCollection();

    info_list.forEach((v) =>{
        if(x.collection.length > 50){
            r.push(x);
            x = new RequestCollection();
        }
        x.collection.push(v);
    })
    r.push(x);
    return r;
}


export function preprocessDBLP(dblpElements: HTMLElement[]){
    addToDBLPElements(document.body.getElementsByClassName("entry inproceedings"), dblpElements);
    addToDBLPElements(document.body.getElementsByClassName("entry informal"), dblpElements);
    addToDBLPElements(document.body.getElementsByClassName("entry article"), dblpElements);
    
    const dblpElementsWithURL: dblpElementInfo[] = [];
    
    dblpElements.forEach((node) => {
        //node.style.border = "5px solid red";
    
        const aNodes = node.getElementsByTagName("a");
        let doi: string | null = null;
        let arXivURL: string | null = null;
    
        for (let i = 0; i < aNodes.length; i++) {
            const aNode = aNodes.item(i);
            if (aNode != null) {
                const hrefStr = aNode.getAttribute("href");
                if (hrefStr != null) {
                    if (hrefStr.indexOf("https://doi.org/") == 0) {
                        doi = hrefStr.substring("https://doi.org/".length);
                        break;
                    } else if (hrefStr.indexOf("https://arxiv.org/") == 0) {
                        arXivURL = hrefStr;
                        break;
                    }
                    else if (hrefStr.indexOf("http://arxiv.org/") == 0) {
                        arXivURL = hrefStr.replace("http", "https");
                        break;
                    }
                }
            }
        }
    
        let registURL: string | null = null;
        if (doi != null) {
            registURL = doi;
        }
        if (arXivURL != null) {
            registURL = arXivURL;
        }
    
    
        if (registURL != null) {
            const xnode = { node: node, url: registURL! };
            dblpElementsWithURL.push(xnode);
            //node_request(node, registURL!);
        }
    
    })
    
    const collections = createCollections(dblpElementsWithURL);
    collections.forEach((v) =>{
        node_request_X(v);
    })
    
    
}