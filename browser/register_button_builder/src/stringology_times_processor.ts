import { processPaperElementInfoCollection, PaperElementInfo, PaperElementInfoCollection, createCollections, getURLForNodeRequest } from "./node_request"

function createPaperCreateInfoArray() : PaperElementInfo[] {
    const r : PaperElementInfo[] = new Array();

    const nodes = document.body.getElementsByTagName("a");
    for(let i = 0;i < nodes.length;i++){
        const node = nodes.item(i);
        if(node != null){
            const url = node.getAttribute("href");
            if(url != null){
                const regularURL = getURLForNodeRequest(url);
                if(regularURL != null){
                    const info : PaperElementInfo = { url : regularURL, node : node};
                    r.push(info);
    
                }
            }
        }
    }
    return r;

}

export function preprocessStringologyTimes() {
    const paperCreateInfoArray = createPaperCreateInfoArray();
    
    const collections = createCollections(paperCreateInfoArray);
    collections.forEach((v) => {
        processPaperElementInfoCollection(v);
    })
    


}
