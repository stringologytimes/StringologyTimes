const fs = require('fs');

async function loadFile(path: string): Promise<string> {
    const ret = new Promise(((resolve, reject) => {
        fs.readFile(path, 'utf-8', (err, data) => {
            if (err) { throw err; }

            resolve(data);
        });
    }))
    let s: string = "";
    await ret.then((v: string) => {
        s = v;
    });
    return s;
};
async function loadJson(path: string): Promise<any[]> {
    const ret = await loadFile(path);
    return JSON.parse(ret);
}

function getReverseColor(color: string): string {
    const R = parseInt(color.substr(0, 2), 16);
    const G = parseInt(color.substr(2, 2), 16);
    const B = parseInt(color.substr(4, 2), 16);


    /*
    var max = Math.max(R, Math.max(G, B));
    var min = Math.min(R, Math.min(G, B));
    //最大値と最小値を足す
    var sum = max + min;
    */

    if ((R * 0.299 + G * 0.587 + B * 0.114) < 186) {
        return color = 'FFFFFF';
    }else{
        return '000000'
    }

    /*
    //R、G、B 値を和から引く
    var newR = (sum - R).toString(16);
    var newG = (sum - G).toString(16);
    var newB = (sum - B).toString(16);

    return newR+newG+newB;
    */

}

type LabelJson = { id: number, node_id: string, url: string, name: string, color: string, default: boolean };
type IssueJson = {
    url: string,
    repository_url: string,
    labels_url: string,
    comments_url: string,
    events_url: string,
    html_url: string,
    id: number,
    node_id: string,
    number: number,
    title: string,
    user: any;
    labels: LabelJson[],
    state: string,
    locked: boolean,
    assignee: any,
    assignees: any[],
    milestone: any,
    comments: number,
    created_at: string,
    updated_at: string,
    closed_at: any,
    author_association: string,
    body: string

}

type LabelItem = { name: string, color: string }

class IssueItem {
    public title: string = "";
    public number: number;
    public labels: LabelItem[] = [];
    public html_url: string;
    public created_at: string;
    public year : number;

    public toString(): string {
        const labelStr = this.labels.map((v) => {
            const revColor = getReverseColor(v.color);
            return `<span style="background-color:#${v.color};color:#${getReverseColor(v.color)}">${v.name}</span>`
        }).join(" ");

        return `${this.created_at.substr(5,2)} [${this.title}](${this.html_url}) ${labelStr}`
    }
}

function createIssueItems(issues: IssueJson[]): IssueItem[] {
    return issues.map((v) => {
        const r = new IssueItem;
        r.title = v.title;
        //console.log(v);
        r.year = 0;

        if (v.labels != undefined) {
            r.labels = v.labels.map((w) => { 
                if(w.color == "aaaaaa" ){
                    r.year = parseInt(w.name);
                }
                return { name: w.name, color: w.color } 
            });
        }
        r.number = v.number;
        r.html_url = v.html_url;
        r.created_at = v.created_at.substr(0,10);

        
        return r;
    })
}


async function main() {
    const json = <IssueJson[]>await loadJson("./data/issue.json");
    const items = createIssueItems(json);

    const filteredItems = items.filter((v)=>{
        return v.year == 2019;
    }).reverse();
    const str = filteredItems.map((v, i) => {
        return `${i+1}. ${v.toString()}`;
    }).join("\n\n")

    fs.writeFile('./data/output.md', str, (err, data) => {
        if (err) console.log(err);
        else console.log('write end');
    });    
}

async function main2() {
    const json = <IssueJson[]>await loadJson("./data/issue.json");
    const items = createIssueItems(json);
    //let re = new RegExp('https://arxiv.org/abs/\d+\.\d+?');
    const re = new RegExp('https://arxiv.org/abs/[0-9]+\.[0-9]+');

    const urlList : string[] = new Array(0);
    json.forEach((v) =>{
        var result = v.body.match(re);
        if(result != null){            
            urlList.push(result[0]);
        }
    })
    const log = urlList.join("\n");
    fs.writeFile('./data/fil_arxiv_url.txt', log, (err, data) => {
        if (err) console.log(err);
        else console.log('write end');
    });    

}


main2();
console.log('ファイル読み込み中でも処理が走ります。');