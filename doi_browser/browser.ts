


export function load_gzip_text(url: string): Promise<string> {
    return fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Failed to fetch file: ${response.statusText}`);
            }
            return response.arrayBuffer();
        })
        .then(buffer => {
            // @ts-ignore
            const pako = (globalThis as any).pako || (window as any).pako; // Assume pako is available in global scope
            if (!pako) {
                throw new Error('pako library not loaded. Please include pako in your HTML.');
            }
            const compressed = new Uint8Array(buffer);
            const decompressed = pako.ungzip(compressed);
            // Decode to UTF-8 text
            const decoder = new TextDecoder('utf-8');
            return decoder.decode(decompressed);
        });
}
export async function load_gzip_text_lines(url: string, remove_empty: boolean = true): Promise<string[]> {
    if(remove_empty){
        return load_gzip_text(url).then(text => text.split('\n').filter(line => line.trim()));
    } else {
        return load_gzip_text(url).then(text => text.split('\n'));
    }
}
export async function load_gzip_integer_lines(url: string): Promise<number[]> {
    return load_gzip_text(url).then(text => text.split('\n').map(line => parseInt(line)));
}

export async function load_gzip_integer_list_lines(url: string): Promise<number[][]> {
    const lines = await load_gzip_text(url).then(text => text.split('\n').filter(line => line.trim()));
    const r: number[][] = [];
    lines.forEach(line => {
        const parts = line.split(',');
        const row: number[] = [];
        parts.forEach(part => {
            row.push(parseInt(part));
        });
        r.push(row);
    });
    return r;
}

type DOIStatus = "primary" | "secondary" | "unknown";

export class LightWeightDOIInfo {
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authorIDs: number[] = [];
    public status: number = -1;

    public container_title: string = "";
    public volume: string = "";
    public tags: string[] = [];
    public doiReferenceIDs: number[] = [];
}
export class DOIInfo {
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authors: string[] = [];
    public container_title: string = "";
    public volume: string = "";
    public tags: string[] = [];
    public doiReferences: string[] = [];
    public status: DOIStatus = "unknown";
}


export class DOIInfoCollection {
    private lightweightDOIInfos: LightWeightDOIInfo[] = [];
    private authorList: string[] = [];

    public length(): number {
        return this.lightweightDOIInfos.length;
    }
    public getDOIByID(id: number): string {
        return this.lightweightDOIInfos[id].doi;
    }
    public getDOIInfo(index: number): DOIInfo {
        let r = new DOIInfo();
        r.doi = this.lightweightDOIInfos[index].doi;
        r.title = this.lightweightDOIInfos[index].title;
        r.year = this.lightweightDOIInfos[index].year;
        r.month = this.lightweightDOIInfos[index].month;
        r.authors = this.lightweightDOIInfos[index].authorIDs.map(id => this.authorList[id]);
        r.container_title = this.lightweightDOIInfos[index].container_title;
        r.volume = this.lightweightDOIInfos[index].volume;
        r.doiReferences = this.lightweightDOIInfos[index].doiReferenceIDs.map(id => this.getDOIByID(id));

        if(this.lightweightDOIInfos[index].status == 1){
            r.status = "primary";
        } else if(this.lightweightDOIInfos[index].status == 0){
            r.status = "secondary";
        } else {
            r.status = "unknown";
        }
        return r;
    }


    public static async load(folderURL: string): Promise<DOIInfoCollection> {
        let r = new DOIInfoCollection();
        const doi_list = await load_gzip_text_lines(folderURL + "/doi.csv.gz");
        console.log("size of doi_list: " + doi_list.length);
        doi_list.forEach(line => {
            let doiInfo = new LightWeightDOIInfo();
            doiInfo.doi = line;
            r.lightweightDOIInfos.push(doiInfo);
        });

        var word_list = await load_gzip_text_lines(folderURL + "/word.csv.gz", false);
        var title_list = await load_gzip_integer_list_lines(folderURL + "/compressed_title.csv.gz");
        title_list.forEach((numbers, index) => {
            const title = numbers.map(numbers => word_list[numbers]).join(" ");
            r.lightweightDOIInfos[index].title = title;
        });

        const year_list = await load_gzip_integer_lines(folderURL + "/year.csv.gz");
        year_list.forEach((year, index) => {
            r.lightweightDOIInfos[index].year = year;
        });

        const month_list = await load_gzip_integer_lines(folderURL + "/month.csv.gz");
        month_list.forEach((month, index) => {
            r.lightweightDOIInfos[index].month = month;
        });

        r.authorList = await load_gzip_text_lines(folderURL + "/full_name.csv.gz", false);
        const author_number_list = await load_gzip_integer_list_lines(folderURL + "/compressed_full_name.csv.gz");
        author_number_list.forEach((numbers, index) => {
            r.lightweightDOIInfos[index].authorIDs = numbers;
        });

        const volume_list = await load_gzip_text_lines(folderURL + "/volume.csv.gz");
        volume_list.forEach((volume, index) => {
            r.lightweightDOIInfos[index].volume = volume;
        });

        const container_title_list = await load_gzip_text_lines(folderURL + "/container_title.csv.gz");
        container_title_list.forEach((container_title, index) => {
            r.lightweightDOIInfos[index].container_title = container_title;
        });

        const doi_references_list = await load_gzip_integer_list_lines(folderURL + "/compressed_doi_reference.csv.gz");
        doi_references_list.forEach((numbers, index) => {
            r.lightweightDOIInfos[index].doiReferenceIDs = numbers;
        });

        const status_list = await load_gzip_integer_lines(folderURL + "/doi_flag.csv.gz");
        status_list.forEach((status, index) => {
            if(index >= r.lightweightDOIInfos.length){
                console.log("status_list is longer than lightweightDOIInfos");
                throw new Error("status_list is longer than lightweightDOIInfos");
            }
            r.lightweightDOIInfos[index].status = status;
        });

        return r;

    }
}
//export let doiInfoCollection: DOIInfoCollection = new DOIInfoCollection();