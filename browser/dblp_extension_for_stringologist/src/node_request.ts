export interface dblpElementInfo {
    node: HTMLElement;
    url: string;
}

export class RequestCollection {
    public collection: dblpElementInfo[] = [];
}

export function node_request2(parent: HTMLElement, button: HTMLElement, url: string) {
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
export function node_request_X(info_collection: RequestCollection ) {
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