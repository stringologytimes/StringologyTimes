


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
    if (remove_empty) {
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
    public status: DOIStatus = "unknown";
}

export class DOIInfoSearchInput {
    public minimum_year: number | null = null;
    public maximum_year: number | null = null;
    public authors: string[] = [];
    public tags: string[] = [];
    public volume: string | null = null;
    public container_title: string | null = null;
    public doiReferences: string[] = [];
    public status: DOIStatus | null = null;

    public static buildFromURLParameters(): DOIInfoSearchInput {
        let r = new DOIInfoSearchInput();
        const sp = new URL(location.href).searchParams;

        for (const [k, v] of sp.entries()) {
            if(k == "minimum_year"){
                r.minimum_year = parseInt(v);
            }
            else if(k == "maximum_year"){
                r.maximum_year = parseInt(v);
            }
            else if(k == "author"){
                r.authors.push(v);
            }
            else if(k == "tag"){
                r.tags.push(v);
            }else if(k == "volume"){
                r.volume = v;
            }else if(k == "container_title"){
                r.container_title = v;
            }else if(k == "doi_reference"){
                r.doiReferences.push(v);
            }
        }
        return r;
    }


}

export class DOIInfoCollection {
    private lightweightDOIInfos: LightWeightDOIInfo[] = [];
    private authorList: string[] = [];
    private yearToDoiMapper: Map<number, number[]> = new Map();
    private authorToDoiMapper: Map<string, number[]> = new Map();
    private containerTitleToDoiMapper: Map<string, number[]> = new Map();
    private doiReferencesToDoiMapper: Map<string, number[]> = new Map();

    public length(): number {
        return this.lightweightDOIInfos.length;
    }
    public getDOIByID(id: number): string {
        return this.lightweightDOIInfos[id].doi;
    }
    public getMaxmumYear(): number {
        if (this.yearToDoiMapper.size == 0) {
            return 0;
        } else {
            const yearArr = Array.from(this.yearToDoiMapper.keys());
            return Math.max(...yearArr);
        }
    }
    public getMinimumYear(): number {
        if (this.yearToDoiMapper.size == 0) {
            return 0;
        } else {
            const yearArr = Array.from(this.yearToDoiMapper.keys());
            return Math.min(...yearArr);
        }
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

        if (this.lightweightDOIInfos[index].status == 1) {
            r.status = "primary";
        } else if (this.lightweightDOIInfos[index].status == 0) {
            r.status = "secondary";
        } else {
            r.status = "unknown";
        }
        return r;
    }
    private searchByYear(minimum_year: number | null = null, maximum_year: number | null = null, candidates: DOIInfo[] | null = null): DOIInfo[] {
        const r: DOIInfo[] = [];
        let minYear = minimum_year ?? this.getMinimumYear();
        let maxYear = maximum_year ?? this.getMaxmumYear();
        if (minYear > maxYear) {
            minYear = maxYear;
        }
        if(candidates == null){
            for (let year = minYear; year <= maxYear; year++) {
                const doiIds = this.yearToDoiMapper.get(year);
                if (doiIds) {
                    doiIds.forEach(doiId => {
                        r.push(this.getDOIInfo(doiId));
                    });
                }
            }    
        }else{
            candidates.forEach(candidate => {
                if(candidate.year >= minYear && candidate.year <= maxYear){
                    r.push(candidate);
                }
            });
        }
        return r;
    }
    private searchByAuthor(author: string, candidates : DOIInfo[] | null = null): DOIInfo[] {
        const r: DOIInfo[] = [];
        if(candidates == null){
            const doiIds = this.authorToDoiMapper.get(author);
            if(doiIds){
                doiIds.forEach(doiId => {
                    r.push(this.getDOIInfo(doiId));
                });
            }    
        }else{
            candidates.forEach(candidate => {
                if(candidate.authors.includes(author)){
                    r.push(candidate);
                }
            });
        }
        return r;
    }
    private searchByAuthors(authors: string[], candidates : DOIInfo[] | null = null): DOIInfo[] {
        let r: DOIInfo[] = [];
        if(candidates == null){
            if(authors.length == 0){
                return r;
            }else{
                r = this.searchByAuthor(authors[0]);
                for(let i = 1; i < authors.length; i++){
                    r = this.searchByAuthor(authors[i], r);
                }
                return r;
            }
        }else{
            candidates.forEach(candidate => r.push(candidate));
            authors.forEach(author => {
                r = this.searchByAuthor(author, r);
            });
            return r;
        }
    }
    private searchByDOIReference(doi_reference: string, candidates : DOIInfo[] | null = null): DOIInfo[] {
        let r: DOIInfo[] = [];
        if(candidates == null){
            const doiIds = this.doiReferencesToDoiMapper.get(doi_reference);
            if(doiIds){
                doiIds.forEach(doiId => {
                    r.push(this.getDOIInfo(doiId));
                });
            }
        }else{
            r = candidates.filter(candidate => candidate.doiReferences.includes(doi_reference));
        }
        return r;

    }
    private searchByDOIReferences(doi_references: string[], candidates : DOIInfo[] | null = null): DOIInfo[] {
        let r: DOIInfo[] = [];
        if(candidates == null){
            if(doi_references.length == 0){
                return r;
            }else{
                r = this.searchByDOIReference(doi_references[0], null);
                for(let i = 1; i < doi_references.length; i++){
                    r = this.searchByDOIReference(doi_references[i], r);
                }
                return r;
            }
        }else{
            candidates.forEach(candidate => r.push(candidate));
            doi_references.forEach(doi_reference => {
                r = this.searchByDOIReference(doi_reference, r);
            });
            return r;
        }
    }


    private searchByContainerTitle(container_title: string, candidates : DOIInfo[] | null = null): DOIInfo[] {
        let r: DOIInfo[] = [];
        if(candidates == null){
            const doiIds = this.containerTitleToDoiMapper.get(container_title);
            if(doiIds){
                doiIds.forEach(doiId => {
                    r.push(this.getDOIInfo(doiId));
                });
            }
        }else{
            r = candidates.filter(candidate => candidate.container_title == container_title);
        }
        return r;
    }
    private filter(input: DOIInfoSearchInput, candidates: DOIInfo[]): DOIInfo[] {
        let r: DOIInfo[] = candidates.map(candidate => candidate);
        if(input.minimum_year != null || input.maximum_year != null){
            r = this.searchByYear(input.minimum_year, input.maximum_year, r);
        }
        
        if (input.authors.length > 0){
            r = this.searchByAuthors(input.authors, r);
        }
        
        if(input.container_title != null){
            r = this.searchByContainerTitle(input.container_title, r);
        }
        
        if(input.doiReferences.length > 0){
            r = this.searchByDOIReferences(input.doiReferences, r);
        }
        
        if(input.status != null){
            throw new Error("status is not supported yet");
        }
        return r;
    }

    public search(input: DOIInfoSearchInput): DOIInfo[] {
        let r: DOIInfo[] = [];
        if(input.minimum_year != null || input.maximum_year != null){
            r = this.searchByYear(input.minimum_year, input.maximum_year);
        }else if (input.authors.length > 0){
            r = this.searchByAuthor(input.authors[0]);
        }else if(input.container_title != null){
            r = this.searchByContainerTitle(input.container_title);
        }else if(input.doiReferences.length > 0){
            r = this.searchByDOIReference(input.doiReferences[0]);
        }else if(input.status != null){
            throw new Error("status is not supported yet");
        }

        return this.filter(input, r);
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
            if (index >= r.lightweightDOIInfos.length) {
                console.log("status_list is longer than lightweightDOIInfos");
                throw new Error("status_list is longer than lightweightDOIInfos");
            }
            r.lightweightDOIInfos[index].status = status;
        });

        r.lightweightDOIInfos.forEach((doiInfo, index) => {
            if(r.yearToDoiMapper.has(doiInfo.year)){
                r.yearToDoiMapper.get(doiInfo.year)!.push(index);
            }else{
                r.yearToDoiMapper.set(doiInfo.year, [index]);
            }

            if(r.containerTitleToDoiMapper.has(doiInfo.container_title)){
                r.containerTitleToDoiMapper.get(doiInfo.container_title)!.push(index);
            }else{
                r.containerTitleToDoiMapper.set(doiInfo.container_title, [index]);
            }

            doiInfo.authorIDs.forEach(authorID => {
                const author = r.authorList[authorID];
                if(r.authorToDoiMapper.has(author)){
                    r.authorToDoiMapper.get(author)!.push(index);
                }else{
                    r.authorToDoiMapper.set(author, [index]);
                }
            });

            doiInfo.doiReferenceIDs.forEach(doiReferenceID => {
                const doiReference = r.getDOIByID(doiReferenceID);
                if(r.doiReferencesToDoiMapper.has(doiReference)){
                    r.doiReferencesToDoiMapper.get(doiReference)!.push(index);
                }else{
                    r.doiReferencesToDoiMapper.set(doiReference, [index]);
                }
            });

        });


        return r;

    }
}

export class BrowserInfo {
    public doiInfoCollection: DOIInfoCollection | null = null;
    public foundDOIList: DOIInfo[] | null =null;
    public pageNumber : number = 0;
    public pageSize : number = 100;

    public getCurrentDOIListPart(): DOIInfo[] {
        if(this.foundDOIList == null){
            return [];
        }
        else if(this.foundDOIList.length == 0){
            return [];
        }
        else{
            let startIndex = this.pageNumber * this.pageSize;
            if(startIndex >= this.foundDOIList.length){
                startIndex = 0;
            }
            let endIndex = startIndex + this.pageSize;
            if(endIndex >= this.foundDOIList.length){
                endIndex = this.foundDOIList.length - 1;
            }
            const r : DOIInfo[] = [];
            for(let i = startIndex; i <= endIndex; i++){
                r.push(this.foundDOIList[i]);
            }
            return r;
        }
    }
}

//export let doiInfoCollection: DOIInfoCollection = new DOIInfoCollection();