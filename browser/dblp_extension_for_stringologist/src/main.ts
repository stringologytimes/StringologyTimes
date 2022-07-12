//document.body.style.border = "5px solid red";


const dblpElements: HTMLElement[] = []

interface dblpElementInfo {
    node: HTMLElement;
    url: string;
}

class RequestCollection {
    public collection: dblpElementInfo[] = [];
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

function addToDBLPElements(nodes: HTMLCollectionOf<Element>, output: HTMLElement[]) {
    for (let i = 0; i < nodes.length; i++) {
        const node = nodes.item(i);
        if (node instanceof HTMLElement) {
            dblpElements.push(node);
        }
    }
}
function node_request2(parent: HTMLElement, button: HTMLElement, url: string) {
    let button_request = new XMLHttpRequest();
    button_request.open('GET', `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=register&url=${encodeURI(url!)}`, true);
    button_request.onload = function () {
        const json = JSON.parse(this.responseText);
        const json_line = json["result"][0];
        const result: string = json_line[1];
        if (result == "SUCCESS") {
            button.textContent = "OK";
            button.onclick = null;
            parent.style.backgroundColor = "springgreen"
        } else {
            button.textContent = "ERROR";
            button.onclick = null;
            parent.style.backgroundColor = "red"

        }

    }
    button_request.send();

}
function node_request_X(info_collection: RequestCollection ) {
    const url_parameter = info_collection.collection.map((v) => v.url).join(",");
    let request = new XMLHttpRequest();
    const get_url = `http://lampin.sakura.ne.jp/cgi-bin/paper_check.cgi?mode=check&url=${encodeURI(url_parameter)}`;
    request.open('GET', get_url, true);
    request.onload = function () {
        const json = JSON.parse(this.responseText);
        const result_list: [string, string][] = json["result"];
        result_list.forEach((v, i) => {
            const check_result: string = v[1];
            const node = info_collection.collection[i].node;
            const url = info_collection.collection[i].url;
            if (check_result == "NOT_REGISTRED") {
                const button = document.createElement("button");
                button.setAttribute("custom-url", url);
                button.textContent = "Regist";
                button.onclick = (e) => {
                    node_request2(node, button, url);
                }
                node.appendChild(button);

            } else if (check_result == "DUPLICATION") {
                node.style.backgroundColor = "springgreen"
            }

        })
    }
    request.onerror = function () {
        console.log("error");

    }
    request.onabort = function () {
        console.log("abort");

    }
    request.ontimeout = function () {
        console.log("timeout");

    }

    request.send();

}

//addToDBLPElements(document.body.getElementsByClassName("entry inproceedings toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry informal toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry article toc"), dblpElements);
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
