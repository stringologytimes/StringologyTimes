import { DOIInfoCollection } from "../doi_info";
import { DOIInfo } from "../doi_info";
import { DOIFilterQuery } from "./doi_filter_query";

export class DOIFilterResult {
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
            if(doiID >= r.lightweightDOIInfos.length){
                console.log("doiID is greater than the length of lightweightDOIInfos");
                console.log("doiID: " + doiID);
                console.log("length of lightweightDOIInfos: " + r.lightweightDOIInfos.length);
                throw new Error("doiID is greater than the length of lightweightDOIInfos");
            }
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

    public search(doiFilterInput: DOIFilterQuery, collection: DOIInfoCollection): DOIFilterResult {
        const resultDOIIDs: number[] = this.doiIDs.filter(doiID => {
            const doiInfo = collection.getDOIInfo(doiID);
            return doiFilterInput.contain(doiInfo);
        });

        return new DOIFilterResult(resultDOIIDs, collection);
    }




}