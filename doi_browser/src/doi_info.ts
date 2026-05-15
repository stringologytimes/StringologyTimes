



import { load_gzip_text_lines, load_gzip_integer_list_lines, load_gzip_integer_lines } from "./gzip_loader";

export type DOIStatus = "primary" | "secondary" | "unknown";

export class LightWeightDOIInfo {
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authorIDs: number[] = [];
    public isPrimary: boolean = false;
    public type: string = "Unknown";
    public container_title: string = "";
    public volume: string = "";
    public tags: string[] = [];    
    public doiReferenceIDs: number[] = [];
}
export class DOIInfo {
    public id: number = -1;
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authors: string[] = [];
    public container_title: string = "";
    public volume: string = "";
    public tags: string[] = [];
    public doiReferences: string[] = [];
    public keywords: string[] = [];
    public type: string = "Unknown";
    public isPrimary: boolean = false;

    public getStatus(): DOIStatus {
        if (this.isPrimary) {
            return "primary";
        } else {
            return "secondary";
        }
    }
}



export class DOIInfoCollection {
    public lightweightDOIInfos: LightWeightDOIInfo[] = [];
    public authorList: string[] = [];
    public tagList: string[] = [];


    public length(): number {
        return this.lightweightDOIInfos.length;
    }
    public getDOIByID(id: number): string {
        if(id >= this.lightweightDOIInfos.length){
            console.log("id is greater than the length of lightweightDOIInfos");
            console.log("id: " + id);
            console.log("length of lightweightDOIInfos: " + this.lightweightDOIInfos.length);
            throw new Error("id is greater than the length of lightweightDOIInfos");
        }
        if(Number.isNaN(id)){
            console.log("id is NaN");
            console.log("id: " + id);
            throw new Error("id is NaN");
        }
        return this.lightweightDOIInfos[id].doi;
    }

    public getDOIInfo(index: number): DOIInfo {
        let r = new DOIInfo();
        r.id = index;
        r.doi = this.lightweightDOIInfos[index].doi;
        r.title = this.lightweightDOIInfos[index].title;
        r.year = this.lightweightDOIInfos[index].year;
        r.month = this.lightweightDOIInfos[index].month;
        r.authors = this.lightweightDOIInfos[index].authorIDs.map(id => this.authorList[id]);
        r.container_title = this.lightweightDOIInfos[index].container_title;
        r.volume = this.lightweightDOIInfos[index].volume;

        r.doiReferences = this.lightweightDOIInfos[index].doiReferenceIDs.map(id => this.getDOIByID(id));
        r.type = this.lightweightDOIInfos[index].type;
        r.tags = this.lightweightDOIInfos[index].tags.map(tag => tag);
        if (this.lightweightDOIInfos[index].isPrimary) {
            r.isPrimary = true;
        } else{
            r.isPrimary = false;
        }
        return r;
    }

    public static async load(folderURL: string): Promise<DOIInfoCollection> {
        console.log("loading DOIInfoCollection from: " + folderURL);
        let r = new DOIInfoCollection();
        const doi_list = await load_gzip_text_lines(folderURL + "/doi.csv.gz");
        console.log("size of doi_list: " + doi_list.length);
        doi_list.forEach(line => {
            let doiInfo = new LightWeightDOIInfo();
            doiInfo.doi = line;
            r.lightweightDOIInfos.push(doiInfo);
        });

        var word_list = await load_gzip_text_lines(folderURL + "/word.csv.gz");
        var title_list = await load_gzip_integer_list_lines(folderURL + "/compressed_title.csv.gz");
        title_list.forEach((numbers, index) => {
            const title = numbers.map(numbers => word_list[numbers]).join(" ");
            r.lightweightDOIInfos[index].title = title;
        });

        const year_list = await load_gzip_integer_lines(folderURL + "/year.csv.gz");
        console.log("size of year_list: " + year_list.length);
        year_list.forEach((year, index) => {
            r.lightweightDOIInfos[index].year = year;
        });

        const month_list = await load_gzip_integer_lines(folderURL + "/month.csv.gz");
        console.log("size of month_list: " + month_list.length);
        month_list.forEach((month, index) => {
            r.lightweightDOIInfos[index].month = month;
        });

        r.authorList = await load_gzip_text_lines(folderURL + "/full_name.csv.gz");
        const author_number_list = await load_gzip_integer_list_lines(folderURL + "/compressed_full_name.csv.gz");
        console.log("size of author_number_list: " + author_number_list.length);
        author_number_list.forEach((numbers, index) => {
            r.lightweightDOIInfos[index].authorIDs = numbers;
        });

        const volume_list = await load_gzip_text_lines(folderURL + "/volume.csv.gz");
        console.log("size of volume_list: " + volume_list.length);
        volume_list.forEach((volume, index) => {
            r.lightweightDOIInfos[index].volume = volume;
        });

        const container_title_list = await load_gzip_text_lines(folderURL + "/container_title.csv.gz");
        console.log("size of container_title_list: " + container_title_list.length);
        container_title_list.forEach((container_title, index) => {
            r.lightweightDOIInfos[index].container_title = container_title;
        });

        const doi_references_list = await load_gzip_integer_list_lines(folderURL + "/compressed_doi_reference.csv.gz");
        console.log("size of doi_references_list: " + doi_references_list.length);
        doi_references_list.forEach((numbers, index) => {
            r.lightweightDOIInfos[index].doiReferenceIDs = numbers;
        });
        
        const type_list = await load_gzip_text_lines(folderURL + "/type.csv.gz");
        console.log("size of type_list: " + type_list.length);
        type_list.forEach((type, index) => {
            if (type.length > 0) {
                r.lightweightDOIInfos[index].type = type;
            } else {
                r.lightweightDOIInfos[index].type = "unknown";
            }
        });


        const status_list = await load_gzip_integer_lines(folderURL + "/doi_flag.csv.gz");
        console.log("size of status_list: " + status_list.length);
        status_list.forEach((status, index) => {
            if (index >= r.lightweightDOIInfos.length) {
                console.log("status_list is longer than lightweightDOIInfos");
                throw new Error("status_list is longer than lightweightDOIInfos");
            }            
            r.lightweightDOIInfos[index].isPrimary = status == 1;
        });

        for(let i = 0; i < r.lightweightDOIInfos.length; i++){
            if(r.lightweightDOIInfos[i] === undefined){
                console.log("lightweightDOIInfos[i] is undefined");
                console.log("i: " + i);
                console.log("length of lightweightDOIInfos: " + r.lightweightDOIInfos.length);
                throw new Error("lightweightDOIInfos[i] is undefined");
            }
        }

        const tag_list = await load_gzip_text_lines(folderURL + "/tag.csv.gz");
        const tag_index_list = await load_gzip_integer_list_lines(folderURL + "/tag_of_each_element.csv.gz");
        r.tagList = tag_list;

        tag_index_list.forEach((numbers, index) => {
            numbers.forEach((number) => {
                r.lightweightDOIInfos[index].tags.push(tag_list[number]);
            });
        });

        console.log("lightweightDOIInfos is loaded successfully : " + r.lightweightDOIInfos.length);



        return r;

    }
}




//export let doiInfoCollection: DOIInfoCollection = new DOIInfoCollection();