


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


export class DOIInfo {
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authors: string[] = [];
    public container_title: string = "";
    public volume: string = "";
    public tags: string[] = [];

}

export class DOIInfoCollection {
    public doiInfos: DOIInfo[] = [];

    public static async load(folderURL: string): Promise<DOIInfoCollection> {
        let r = new DOIInfoCollection();
        const doi_list = await load_gzip_text_lines(folderURL + "/doi.csv.gz");
        console.log("size of doi_list: " + doi_list.length);
        doi_list.forEach(line => {
            let doiInfo = new DOIInfo();
            doiInfo.doi = line;
            r.doiInfos.push(doiInfo);
        });

        var word_list = await load_gzip_text_lines(folderURL + "/word.csv.gz", false);
        var title_list = await load_gzip_integer_list_lines(folderURL + "/compressed_title.csv.gz");
        title_list.forEach((numbers, index) => {
            const title = numbers.map(numbers => word_list[numbers]).join(" ");
            r.doiInfos[index].title = title;
        });

        const year_list = await load_gzip_text_lines(folderURL + "/year.csv.gz");
        year_list.forEach((year, index) => {
            if (year.length > 0) {
                r.doiInfos[index].year = parseInt(year);
            } else {
                r.doiInfos[index].year = -1;
            }
        });

        const month_list = await load_gzip_text_lines(folderURL + "/month.csv.gz");
        month_list.forEach((month, index) => {
            if (month.length > 0) {
                r.doiInfos[index].month = parseInt(month);
            } else {
                r.doiInfos[index].month = -1;
            }
        });

        const fullname_list = await load_gzip_text_lines(folderURL + "/full_name.csv.gz", false);
        const author_number_list = await load_gzip_integer_list_lines(folderURL + "/compressed_full_name.csv.gz");
        author_number_list.forEach((numbers, index) => {
            numbers.map(numbers => fullname_list[numbers]).forEach(fullname => {
                r.doiInfos[index].authors.push(fullname);
            });
        });

        const volume_list = await load_gzip_text_lines(folderURL + "/volume.csv.gz");
        volume_list.forEach((volume, index) => {
            r.doiInfos[index].volume = volume;
        });

        const container_title_list = await load_gzip_text_lines(folderURL + "/container_title.csv.gz");
        container_title_list.forEach((container_title, index) => {
            r.doiInfos[index].container_title = container_title;
        });

        return r;

    }
}
//export let doiInfoCollection: DOIInfoCollection = new DOIInfoCollection();