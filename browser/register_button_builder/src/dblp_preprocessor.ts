import { processPaperElementInfoCollection, PaperElementInfo, PaperElementInfoCollection, createCollections, getURLForNodeRequest } from "./node_request"



function createPaperCreateInfoArraySub(nodes: HTMLCollectionOf<Element>): PaperElementInfo[] {
    const r: PaperElementInfo[] = new Array();
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node instanceof HTMLElement) {
            const info = createPaperElementInfo(node);
            if (info != null) {
                r.push(info);
            }
        }
    }
    return r;
}
function createPaperCreateInfoArray(): PaperElementInfo[] {
    const r: PaperElementInfo[] = new Array();
    createPaperCreateInfoArraySub(document.body.getElementsByClassName("entry inproceedings")).forEach((v) => {
        r.push(v);
    })
    createPaperCreateInfoArraySub(document.body.getElementsByClassName("entry informal")).forEach((v) => {
        r.push(v);
    })
    createPaperCreateInfoArraySub(document.body.getElementsByClassName("entry article")).forEach((v) => {
        r.push(v);
    })
    return r;

}


function createPaperElementInfo(node: HTMLElement): PaperElementInfo | null {
    const aNodes = node.getElementsByTagName("a");
    let url: string | null = null;

    for (let i = 0; i < aNodes.length; i++) {
        const aNode = aNodes.item(i);
        if (aNode != null) {
            const hrefStr = aNode.getAttribute("href");
            if (hrefStr != null) {
                url = getURLForNodeRequest(hrefStr);
                if (url != null) {
                    break;
                }
            }
        }
    }
    if (url != null) {
        const xnode = { node: node, url: url! };
        return xnode;
    } else {
        return null;
    }
}


export function preprocessDBLP() {
    const paperCreateInfoArray = createPaperCreateInfoArray();
    const collections = createCollections(paperCreateInfoArray);
    collections.forEach((v) => {
        processPaperElementInfoCollection(v);
    })


}
