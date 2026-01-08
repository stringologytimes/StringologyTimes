



import { load_gzip_text_lines, load_gzip_integer_list_lines, load_gzip_integer_lines } from "./gzip_loader";

export type DOIStatus = "primary" | "secondary" | "unknown";

export class LightWeightDOIInfo {
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authorIDs: number[] = [];
    public status: number = -1;
    public type: string = "unknown";
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



export class DOIInfoCollection {
    private lightweightDOIInfos: LightWeightDOIInfo[] = [];
    private authorList: string[] = [];
    private yearToDoiMapper: Map<number, number[]> = new Map();
    private authorToDoiMapper: Map<string, number[]> = new Map();
    private containerTitleToDoiMapper: Map<string, number[]> = new Map();
    private doiReferencesToDoiMapper: Map<string, number[]> = new Map();
    private typeToDOIInfoMapper: Map<string, number[]> = new Map();

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
    public searchByYear(minimum_year: number | null = null, maximum_year: number | null = null, candidates: DOIInfo[] | null = null): DOIInfo[] {
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
    public searchByAuthor(author: string, candidates : DOIInfo[] | null = null): DOIInfo[] {
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
    public searchByAuthors(authors: string[], candidates : DOIInfo[] | null = null): DOIInfo[] {
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
    public searchByDOIReference(doi_reference: string, candidates : DOIInfo[] | null = null): DOIInfo[] {
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
    public searchByDOIReferences(doi_references: string[], candidates : DOIInfo[] | null = null): DOIInfo[] {
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


    public searchByContainerTitle(container_title: string, candidates : DOIInfo[] | null = null): DOIInfo[] {
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
    

    public intitalizeFilterOptions() : void {
        const type_list = Array.from(this.typeToDOIInfoMapper.keys());
        type_list.sort();
        const typeSelect = document.getElementById("type-select");
        if(typeSelect){
            typeSelect.innerHTML = "";
            const defaultOption = document.createElement("option");
            defaultOption.value = "";
            defaultOption.textContent = "Any";
            typeSelect.appendChild(defaultOption);
            type_list.forEach(type => {
                const option = document.createElement("option");
                option.value = type;
                option.textContent = type;
                typeSelect.appendChild(option);
            });    
        }

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

        const type_list = await load_gzip_text_lines(folderURL + "/type.csv.gz");
        type_list.forEach((type, index) => {
            if(type.length > 0){
                r.lightweightDOIInfos[index].type = type;
            }else{
                r.lightweightDOIInfos[index].type = "unknown";
            }
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

            if(r.typeToDOIInfoMapper.has(doiInfo.type)){
                r.typeToDOIInfoMapper.get(doiInfo.type)!.push(index);
            }else{
                //console.log("type: " + doiInfo.type + " " + index + "/" + r.lightweightDOIInfos.length);

                r.typeToDOIInfoMapper.set(doiInfo.type, [index]);
            }


        });



        return r;

    }
}



//export let doiInfoCollection: DOIInfoCollection = new DOIInfoCollection();