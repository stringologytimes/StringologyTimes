export type PaperIDType = "doi" | "arxiv" | "otherURL" | "none";

export class PaperArticle {
    year: number = 0;
    conference: string = "";
    title: string = "";
    conferencePaperURL: string = "";
    authors: string[] = new Array(0);
    arxivURL: string = "";
    videoURL : string = "";
    slideURL : string = "";
    doi : string = "";
    public constructor() {
    }
    public static get_paper_id_type(id : string) : PaperIDType{
        if(id.indexOf('https://arxiv.org/abs/')== 0){
            return "arxiv";

        }else if(id.indexOf('10.')==0){
            return "doi";

        }else if(id.indexOf('http')==0){
            return "otherURL";
        }
        else{
            return "none";
        }
    }
    public copy() : PaperArticle{
        const r = new PaperArticle();
        r.conference = this.conference;
        r.title = this.title;
        r.conferencePaperURL = this.conferencePaperURL;
        this.authors.forEach((v)=>{
            r.authors.push(v);
        })
        r.arxivURL = this.arxivURL;
        r.videoURL = this.videoURL;
        r.slideURL = this.slideURL;
        return r;
    }
    public clear(){
        this.title = "";
        this.conferencePaperURL = "";
        this.authors = new Array(0);
        this.arxivURL = "";
        this.videoURL = "";
        this.slideURL = "";
    }
    public isEmpty(){
        return this.title == null;
    }
    public uniqueName() : string {
        return `${this.conference}_${this.year}_${this.title.toLowerCase()}`
    }
    public static parseList(list : string) : PaperArticle[] {
        const lines = list.split("\r\n");
        const r : PaperArticle[] = <PaperArticle[]>(lines.map((v) => PaperArticle.parse(v)).filter((v) => v != null));
        return r;
    }
    public static parse(line : string) : PaperArticle | null {
        const p = line.split("\t");
        const r = new PaperArticle();
        if(p.length > 4){
            r.year = Number.parseInt(p[0]);
            r.conference = p[1];
            r.title = p[2];
            r.authors = p[3].split(", ");    
            if(p.length > 4){
                r.conferencePaperURL = p[4];
            }
            if(p.length > 5){
                r.arxivURL = p[5];
            }
            if(p.length > 6){
                r.videoURL = p[6];
            }
            if(p.length > 7){
                r.slideURL = p[7];
            }
            if(p.length > 8){
                r.doi = p[8];
            }
            if(isNaN(r.year)){
                console.log(`Skip: ${line}`);
                return null;
            }else{
                return r;    

            }

        }else{
            return null;
        }
    }

    public toCSSString() : string{

        return `${this.year}\t${this.conference}\t${this.title}\t${this.authors.join(", ")}\t${this.conferencePaperURL}\t${this.arxivURL}\t${this.videoURL}\t${this.slideURL}`;

    }
}

export class ConferenceArticle{
    public year : number;
    public conference : string;
    public conferenceURL : string;
    public acceptedPaperURL : string;
    public deadline : Date = new Date();
    public dblpName : string | null;
    public dblpSubname : string = "";
    public dblpSuffixName : string = "";

    public static parseList(list : string) : ConferenceArticle[] {
        const lines = list.split("\r\n");
        const r : ConferenceArticle[] = <ConferenceArticle[]>(lines.map((v) => ConferenceArticle.parse(v)).filter((v) => v != null));
        return r;
    }

    public static parse(line : string) : ConferenceArticle | null {
        const p = line.split("\t");
        const r = new ConferenceArticle();
        if(p.length > 3){
            r.year = Number.parseInt(p[0]);
            r.conference = p[1];
            r.conferenceURL = p[2];
            if(p.length > 3){
                r.acceptedPaperURL = p[3];
            }
            if(p.length > 4){
                const date = new Date();
                const time = p[4].split("/");
                date.setFullYear(Number.parseInt(time[0]));
                date.setMonth(Number.parseInt(time[1]));
                date.setDate(Number.parseInt(time[2]));
                r.deadline = date;
            }
            if(p.length > 5){
                r.dblpName = p[5];                
            }
            if(p.length > 6){
                r.dblpSubname = p[6];                
            }
            if(p.length > 7){
                r.dblpSuffixName = p[7];
            }

            if(isNaN(r.year)){
                console.log(`Skip: ${line}`);
                return null;
            }else{
                return r;    

            }
        }else{
            return null;
        }
    }
    public getDeadlineString() : string{
        const month = this.deadline.getMonth() < 10 ? `0${this.deadline.getMonth()}` : this.deadline.getMonth().toString();
        const day = this.deadline.getDate() < 10 ? `0${this.deadline.getDate()}` : this.deadline.getDate().toString();
        const year = this.deadline.getFullYear().toString();

        return `${year}/${month}/${day}`;
    }
    public getDBLPJSONURL() : string {
        const subname = this.dblpSubname.length > 0 ? this.dblpSubname : this.dblpName;

        if(this.dblpSuffixName.length > 0){
            return `https://dblp.org/search/publ/api?q=toc%3Adb/conf/${subname}/${this.dblpName}${this.year}${this.dblpSuffixName}.bht%3A&h=1000&format=jsonp`
        }else{
            return `https://dblp.org/search/publ/api?q=toc%3Adb/conf/${subname}/${this.dblpName}${this.year}.bht%3A&h=1000&format=jsonp`
        }
    }
    public toString(){
        return `Year: ${this.year}, Conference: ${this.conference}, conferenceURL: ${this.conferenceURL}, acceptedPaperURL: ${this.acceptedPaperURL}, deadline: ${this.getDeadlineString()}`;

    }
}