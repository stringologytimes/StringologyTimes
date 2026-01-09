



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
    public type: string = "unknown";
    public status: DOIStatus = "unknown";
}



export class DOIInfoCollection {
    public lightweightDOIInfos: LightWeightDOIInfo[] = [];
    public authorList: string[] = [];

    public length(): number {
        return this.lightweightDOIInfos.length;
    }
    public getDOIByID(id: number): string {
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

        if (this.lightweightDOIInfos[index].status == 1) {
            r.status = "primary";
        } else if (this.lightweightDOIInfos[index].status == 0) {
            r.status = "secondary";
        } else {
            r.status = "unknown";
        }
        return r;
    }





    /*
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
    */


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
            if (type.length > 0) {
                r.lightweightDOIInfos[index].type = type;
            } else {
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



        return r;

    }
}

export class DOIInfoCollectionFilter {
    public doiIDs: number[] = [];
    private yearToDoiMapper: Map<number, number[]> = new Map();
    private authorToDoiMapper: Map<string, number[]> = new Map();
    private containerTitleToDoiMapper: Map<string, number[]> = new Map();
    private doiReferencesToDoiMapper: Map<string, number[]> = new Map();
    private typeToDOIInfoMapper: Map<string, number[]> = new Map();


    public constructor(doiIDs: number[] | null, r: DOIInfoCollection) {
        if (doiIDs == null) {
            this.doiIDs = Array.from({ length: r.lightweightDOIInfos.length }, (_, index) => index);
        } else {
            this.doiIDs = doiIDs.map(doiID => r.getDOIInfo(doiID).id);
        }

        this.doiIDs.forEach(doiID => {
            const doiInfo = r.lightweightDOIInfos[doiID];
            if (this.yearToDoiMapper.has(doiInfo.year)) {
                this.yearToDoiMapper.get(doiInfo.year)!.push(doiID);
            } else {
                this.yearToDoiMapper.set(doiInfo.year, [doiID]);
            }

            if (this.containerTitleToDoiMapper.has(doiInfo.container_title)) {
                this.containerTitleToDoiMapper.get(doiInfo.container_title)!.push(doiID);
            } else {
                this.containerTitleToDoiMapper.set(doiInfo.container_title, [doiID]);
            }

            doiInfo.authorIDs.forEach(authorID => {
                const author = r.authorList[authorID];
                if (this.authorToDoiMapper.has(author)) {
                    this.authorToDoiMapper.get(author)!.push(doiID);
                } else {
                    this.authorToDoiMapper.set(author, [doiID]);
                }
            });

            doiInfo.doiReferenceIDs.forEach(doiReferenceID => {
                const doiReference = r.getDOIByID(doiReferenceID);
                if (this.doiReferencesToDoiMapper.has(doiReference)) {
                    this.doiReferencesToDoiMapper.get(doiReference)!.push(doiID);
                } else {
                    this.doiReferencesToDoiMapper.set(doiReference, [doiID]);
                }
            });

            if (this.typeToDOIInfoMapper.has(doiInfo.type)) {
                this.typeToDOIInfoMapper.get(doiInfo.type)!.push(doiID);
            } else {
                this.typeToDOIInfoMapper.set(doiInfo.type, [doiID]);
            }
        });

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
            const yearArr = Array.from(this.yearToDoiMapper.keys()).filter(year => year > 0);
            return Math.min(...yearArr);
        }
    }

    public getTypes(): string[] {
        return Array.from(this.typeToDOIInfoMapper.keys());
    }
    public getContainerTitles(): string[] {
        return Array.from(this.containerTitleToDoiMapper.keys());
    }
    public searchByYear(minimum_year: number | null = null, maximum_year: number | null = null, doiNumberFilterSet: Set<number>, collection: DOIInfoCollection): DOIInfo[] {
        const r: DOIInfo[] = [];
        let minYear = minimum_year ?? this.getMinimumYear();
        let maxYear = maximum_year ?? this.getMaxmumYear();
        if (minYear > maxYear) {
            minYear = maxYear;
        }

        for (let year = minYear; year <= maxYear; year++) {
            if (this.yearToDoiMapper.has(year)) {
                this.yearToDoiMapper.get(year)!.forEach(doiId => {
                    if (doiNumberFilterSet.has(doiId)) {
                        r.push(collection.getDOIInfo(doiId));
                    }
                });
            }
        }
        return r;
    }
    public searchByType(type: string, doiNumberFilterSet: Set<number>, collection: DOIInfoCollection): DOIInfo[] {
        const r: DOIInfo[] = [];
        if (this.typeToDOIInfoMapper.has(type)) {
            this.typeToDOIInfoMapper.get(type)!.forEach(doiId => {
                if (doiNumberFilterSet.has(doiId)) {
                    r.push(collection.getDOIInfo(doiId));
                }
            });
        }
        return r;
    }
    public searchByAuthor(author: string, doiNumberFilterSet: Set<number>, collection: DOIInfoCollection): DOIInfo[] {
        const r: DOIInfo[] = [];
        if (this.authorToDoiMapper.has(author)) {
            this.authorToDoiMapper.get(author)!.forEach(doiId => {
                if (doiNumberFilterSet.has(doiId)) {
                    r.push(collection.getDOIInfo(doiId));
                }
            });
        }
        return r;
    }
    public searchByAuthors(authors: string[], collection: DOIInfoCollection): DOIInfo[] {
        throw new Error("searchByAuthors is not implemented yet");
        /*
        let r: DOIInfo[] = [];
        if(authors.length == 0){
            return r;
        }else{
            const fstAuthor = authors[0];
            const r2 = this.searchByAuthor(fstAuthor, collection);            
            for(let i = 1; i < authors.length; i++){
                const author = authors[i];
                r = this.searchByAuthor(author, r);
            }
            return r;
        }


        candidates.forEach(candidate => r.push(candidate));
        authors.forEach(author => {
            r = this.searchByAuthor(author, r);
        });
        return r;
        */

    }
    public searchByDOIReference(doi_reference: string, doiNumberFilterSet: Set<number>, collection: DOIInfoCollection): DOIInfo[] {
        const r: DOIInfo[] = [];
        if (this.doiReferencesToDoiMapper.has(doi_reference)) {
            this.doiReferencesToDoiMapper.get(doi_reference)!.forEach(doiId => {
                if (doiNumberFilterSet.has(doiId)) {
                    r.push(collection.getDOIInfo(doiId));
                }
            });
        }
        return r;
    }
    public searchByDOIReferences(doi_references: string[], collection: DOIInfoCollection): DOIInfo[] {
        throw new Error("searchByDOIReferences is not implemented yet");
        /*
        let r: DOIInfo[] = [];
        candidates.forEach(candidate => r.push(candidate));
        doi_references.forEach(doi_reference => {
            r = this.searchByDOIReference(doi_reference, r);
        });
        return r;
        */

    }


    public searchByContainerTitle(container_title: string, doiNumberFilterSet: Set<number>, collection: DOIInfoCollection): DOIInfo[] {
        const r: DOIInfo[] = [];
        if (this.containerTitleToDoiMapper.has(container_title)) {
            this.containerTitleToDoiMapper.get(container_title)!.forEach(doiId => {
                if (doiNumberFilterSet.has(doiId)) {
                    r.push(collection.getDOIInfo(doiId));
                }
            });
        }
        return r;
    }




}



//export let doiInfoCollection: DOIInfoCollection = new DOIInfoCollection();