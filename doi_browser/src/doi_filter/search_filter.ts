import { DOIRecord } from "../doi_record";
import { DOIRecordCollection } from "../doi_record_collection";
import { DOIStatus } from "../doi_record";
import { PrimarySearchResult } from "./primary_search_result";

export type SortByType = "alphabetical-order-by-container-title" | "ascending-order-by-date" | "descending-order-by-date" | "article-count" | "unordered";


export class SearchFilter {
    public minimum_year: number | null = null;
    public maximum_year: number | null = null;
    public types: string[] = [];
    public authors: string[] = [];
    public tags: string[] = [];
    public ancestor_doi: string | null = null;
    public top_container_type: string | null = null;
    public doiReferences: string[] = [];    
    public excludeStatus: DOIStatus[] = [];
    public keywords: string[] = [];
    private filter(collection: DOIRecordCollection, candidates: number[]): number[] {
        return candidates.filter(candidate => {
            const doiInfo = collection.getDOIInfo(candidate);
            return this.contain(doiInfo, collection);
        });
    }

    public searchUsingPreviousResult(primarySearchResult: PrimarySearchResult, collection: DOIRecordCollection): number[] {
        let r: number[] = primarySearchResult.doiIDs.map(doiID => doiID);
        return this.filter(collection, r);
    }
    public search(collection: DOIRecordCollection): number[] {
        let r: number[] = [];
        for(let i = 0; i < collection.lightweightDOIRecords.length; i++){
            const doiInfo = collection.getDOIInfo(i);
            if(this.contain(doiInfo, collection)){
                r.push(i);
            }
        }
        return r;
    }

    public is_empty(): boolean {
        return this.minimum_year == null && this.maximum_year == null && this.types.length == 0 && 
        this.authors.length == 0 && this.tags.length == 0 && this.ancestor_doi == null 
        && this.keywords.length == 0 && this.excludeStatus.length == 0 && this.doiReferences.length == 0 && this.top_container_type == null;
    }
    public copy(): SearchFilter {
        const r = new SearchFilter();
        r.minimum_year = this.minimum_year;
        r.maximum_year = this.maximum_year;
        r.types = this.types.map(type => type);
        r.authors = this.authors.map(author => author);
        r.tags = this.tags.map(tag => tag);
        r.ancestor_doi = this.ancestor_doi;
        r.doiReferences = this.doiReferences.map(doiReference => doiReference);
        r.excludeStatus = this.excludeStatus.map(excludeStatus => excludeStatus);
        r.top_container_type = this.top_container_type;
        //r.sortBy = this.sortBy;
        r.keywords = this.keywords.map(keyword => keyword);
        return r;
    }

    public get_parents() : SearchFilter[] {
        var r = new Array<SearchFilter>();
        if(this.minimum_year != null){
            var copy = this.copy();
            copy.minimum_year = null;
            r.push(copy);
        }
        if(this.maximum_year != null){
            var copy = this.copy();
            copy.maximum_year = null;
            r.push(copy);
        }
        if(this.types.length > 0){
            var copy = this.copy();
            copy.types = [];
            r.push(copy);
        }

        if(this.authors.length > 0){
            var copy = this.copy();
            copy.authors = [];
            r.push(copy);
        }
        if(this.tags.length > 0){
            var copy = this.copy();
            copy.tags = [];
            r.push(copy);
        }

        if(this.ancestor_doi != null){
            var copy = this.copy();
            copy.ancestor_doi = null;
            r.push(copy);
        }
        if(this.doiReferences.length > 0){
            var copy = this.copy();
            copy.doiReferences = [];
            r.push(copy);
        }
        if(this.excludeStatus.length > 0){
            var copy = this.copy();
            copy.excludeStatus = [];
            r.push(copy);
        }
        /*
        if(this.sortBy != "unordered"){
            var copy = this.copy();
            copy.sortBy = "unordered";
            r.push(copy);
        }
        */
        if(this.keywords.length > 0){
            var copy = this.copy();
            copy.keywords = [];
            r.push(copy);
        }
        return r;
    }

    public getHash(): string {
        var obj: any = {};

        if(this.minimum_year != null){
            obj.minimum_year = this.minimum_year;
        }
        if(this.maximum_year != null){
            obj.maximum_year = this.maximum_year;
        }
        if(this.types.length > 0){
            obj.types = this.types;
        }
        if(this.authors.length > 0){
            obj.authors = this.authors;
        }
        if(this.tags.length > 0){
            obj.tags = this.tags;
        }
        if(this.ancestor_doi != null){
            obj.ancestor_doi = this.ancestor_doi;
        }
        if(this.doiReferences.length > 0){
            obj.doiReferences = this.doiReferences;
        }
        if(this.excludeStatus.length > 0){
            obj.excludeStatus = this.excludeStatus;
        }
        if(this.top_container_type != null){
            obj.top_container_type = this.top_container_type;
        }
        /*
        if(this.sortBy != "unordered"){
            obj.sortBy = this.sortBy;
        }
        */
        if(this.keywords.length > 0){
            obj.keywords = this.keywords;
        }

        return JSON.stringify(obj);
    }

    public static buildFromJSON(json: string): SearchFilter {
        var obj: any = JSON.parse(json);
        var r = new SearchFilter();

        if(obj.minimum_year != null){
            r.minimum_year = obj.minimum_year;
        }
        if(obj.maximum_year != null){
            r.maximum_year = obj.maximum_year;
        }
        if(obj.types.length > 0){
            r.types = obj.types.map((v: any) => v as string);
        }
        if(obj.authors.length > 0){
            r.authors = obj.authors.map((v: any) => v as string);
        }
        if(obj.tags.length > 0){
            r.tags = obj.tags.map((v: any) => v as string);
        }
        if(obj.ancestor_doi != null){
            r.ancestor_doi = obj.ancestor_doi;
        }
        if(obj.doiReferences.length > 0){
            r.doiReferences = obj.doiReferences.map((v: any) => v as string);
        }
        if(obj.excludeStatus.length > 0){
            r.excludeStatus = obj.excludeStatus.map((v: any) => v as DOIStatus);
        }
        if(obj.top_container_type != null){
            r.top_container_type = obj.top_container_type;
        }
        /*
        if(obj.sortBy != "unordered"){
            r.sortBy = obj.sortBy;
        }
        */
        if(obj.keywords.length > 0){
            r.keywords = obj.keywords.map((v: any) => v as string);
        }
        return r;
    }
    public contain(doiInfo: DOIRecord, collection: DOIRecordCollection): boolean {
        if(this.minimum_year != null && doiInfo.year < this.minimum_year){
            return false;
        }
        if(this.maximum_year != null && doiInfo.year > this.maximum_year){
            return false;
        }
        if(this.types.length > 0 && !this.types.includes(doiInfo.type)){
            return false;
        }
        if(this.ancestor_doi != null && !collection.ancestorCheck(doiInfo.id, this.ancestor_doi)){
            return false;
        }
        if(this.top_container_type != null && !collection.topContainerTypeCheck(doiInfo.id, this.top_container_type)){
            return false;
        }
        if(this.doiReferences.length > 0 && !doiInfo.doiReferences.every(doiReference => this.doiReferences.includes(doiReference))){
            return false;
        }
        if(this.excludeStatus.length > 0){
            for(let i = 0; i < this.excludeStatus.length; i++){
                if(doiInfo.getStatus() == this.excludeStatus[i]){
                    return false;
                }
            }
        }

        for(let i = 0; i < this.tags.length; i++){
            if(!doiInfo.tags.includes(this.tags[i])){
                return false;
            }
        }

        if(this.keywords != null){
            const bArray = [];

            for(let i = 0; i < this.keywords.length; i++){
                var keyword = this.keywords[i];
                let b = false;


                if(keyword.indexOf("@DOI:") == 0){
                    const doiKeyword = keyword.substring(5);
                    if(doiKeyword.length > 0){
                        var fstChar = doiKeyword.charAt(0);
                        if(fstChar == "="){
                            var regexPattern = doiKeyword.substring(1);
                            var regex = new RegExp(regexPattern);
                            if(regex.test(doiInfo.doi)){
                                b = true;
                            }
                        }else{
                            if(doiInfo.doi == doiKeyword){
                                b = true;
                            }        
                        }

                    }else{
                        b = true;
                    }


                }
                else if(keyword.indexOf("@CONTAINER_DOI:") == 0){
                    const containerDOIKeyword = keyword.substring(15);
                    if(doiInfo.container_DOI == containerDOIKeyword){                    
                        b = true;
                    }else if(containerDOIKeyword == "null" && doiInfo.container_DOI == ""){
                        b = true;
                    }
                }
                else if(keyword.indexOf("@CONTAINER_TITLE:") == 0){
                    const containerTitleKeyword = keyword.substring(17);
                    if(doiInfo.container_title == containerTitleKeyword){                    
                        b = true;
                    }else if(containerTitleKeyword == "null" && doiInfo.container_title == ""){
                        b = true;
                    }
                }
                else{
                    if(doiInfo.title.indexOf(keyword) != -1){
                        b = true;
                    }
                    if(doiInfo.doi.indexOf(keyword) != -1){
                        b = true;
                    }
                }
                bArray.push(b);
            }

            if(!bArray.every(b => b)){
                return false;
            }
        }
        return true;
    }

    public isIncluded(item : SearchFilter): boolean {
        if(this.minimum_year != null && item.minimum_year != null){            
            if(this.minimum_year < item.minimum_year){
                return false;
            }
        }
        if(this.maximum_year != null && item.maximum_year != null){
            if(this.maximum_year > item.maximum_year){
                return false;
            }
        }
        if(item.types.length > 0){
            item.types.forEach(element => {
                if(!this.types.includes(element)){
                    return false;
                }                    
            });
        }
        if(this.authors.length > 0){
            return false;
        }
        if(this.tags.length > 0){
            return false;
        }
        if(this.ancestor_doi != null && item.ancestor_doi != null){
            if(this.ancestor_doi != item.ancestor_doi){
                return false;
            }
        }
        if(this.top_container_type != null && item.top_container_type != null){
            if(this.top_container_type != item.top_container_type){
                return false;
            }
        }
        if(item.excludeStatus.length > 0){
            for(let i = 0; i < item.excludeStatus.length; i++){
                if(!this.excludeStatus.includes(item.excludeStatus[i])){
                    return false;
                }
            }
        }


        if(this.doiReferences.length > 0){
            return false;
        }

        for(let i = 0; i < item.tags.length; i++){
            if(!this.tags.includes(item.tags[i])){
                return false;
            }
        }

        if(this.keywords.length > 0){
            if(item.keywords.length > 0){
                return false;
            }
        }

        return true;
    }



}
