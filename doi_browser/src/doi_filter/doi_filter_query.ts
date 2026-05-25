import { DOIInfo } from "../doi_info";
import { DOIInfoCollection } from "../doi_info";
import { DOIStatus } from "../doi_info";
import { DOIFilterResult } from "./doi_filter_result";

export type SortByType = "alphabetical-order-by-container-title" | "ascending-order-by-date" | "descending-order-by-date" | "article-count" | "unordered";


export class DOIFilterQuery {
    public minimum_year: number | null = null;
    public maximum_year: number | null = null;
    public type: string | null = null;
    public authors: string[] = [];
    public tags: string[] = [];
    public volume: string | null = null;
    public container_title: string | null = null;
    public doiReferences: string[] = [];    
    public excludeStatus: DOIStatus[] = [];
    public sortBy: SortByType = "unordered";
    public keywords: string[] = [];

    public static buildFromURLParameters(): DOIFilterQuery {
        let r = new DOIFilterQuery();
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
            }else if(k == "type"){
                r.type = v;
            }else if(k == "volume"){
                r.volume = v;
            }else if(k == "container_title"){
                r.container_title = v;
            }else if(k == "doi_reference"){
                r.doiReferences.push(v);
            }else if(k == "tag1"){
                r.tags.push(v);
            }else if(k == "tag2"){
                r.tags.push(v);
            }else if(k == "tag3"){
                r.tags.push(v);
            }
        }
        return r;
    }

    
    private filter(collection: DOIInfoCollection, candidates: number[]): number[] {
        return candidates.filter(candidate => {
            const doiInfo = collection.getDOIInfo(candidate);
            return this.contain(doiInfo);
        });
    }

    public search(doiInfoCollectionFilter: DOIFilterResult, collection: DOIInfoCollection): number[] {
        let r: number[] = doiInfoCollectionFilter.doiIDs.map(doiID => doiID);
        return this.filter(collection, r);
    }
    public is_empty(): boolean {
        return this.minimum_year == null && this.maximum_year == null && this.type == null && 
        this.authors.length == 0 && this.tags.length == 0 && this.volume == null && this.container_title == null 
        && this.keywords.length == 0
        && this.excludeStatus.length == 0 && this.doiReferences.length == 0;
    }
    public copy(): DOIFilterQuery {
        const r = new DOIFilterQuery();
        r.minimum_year = this.minimum_year;
        r.maximum_year = this.maximum_year;
        r.type = this.type;
        r.authors = this.authors.map(author => author);
        r.tags = this.tags.map(tag => tag);
        r.volume = this.volume;
        r.container_title = this.container_title;
        r.doiReferences = this.doiReferences.map(doiReference => doiReference);
        r.excludeStatus = this.excludeStatus.map(excludeStatus => excludeStatus);
        r.sortBy = this.sortBy;
        r.keywords = this.keywords.map(keyword => keyword);
        return r;
    }

    public getHash(): string {
        return JSON.stringify(this);
    }
    
    public contain(doiInfo: DOIInfo): boolean {
        if(this.minimum_year != null && doiInfo.year < this.minimum_year){
            return false;
        }
        if(this.maximum_year != null && doiInfo.year > this.maximum_year){
            return false;
        }
        if(this.type != null && doiInfo.type != this.type){
            return false;
        }
        if(this.container_title != null && doiInfo.container_title != this.container_title){
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
                    if(doiInfo.doi == doiKeyword){
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

    public isIncluded(item : DOIFilterQuery): boolean {
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
        if(item.type != null){
            if(this.type == null){
                return false;
            }
            else{
                if(this.type != item.type){
                    return false;
                }
            }
        }
        if(this.authors.length > 0){
            return false;
        }
        if(this.tags.length > 0){
            return false;
        }
        if(this.volume != null && item.volume != null){
            if(this.volume != item.volume){
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
        if(this.container_title != null && item.container_title != null){
            if(this.container_title != item.container_title){
                return false;
            }
        }
        if(this.doiReferences.length > 0){
            return false;
        }

        if(this.sortBy != item.sortBy){
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
