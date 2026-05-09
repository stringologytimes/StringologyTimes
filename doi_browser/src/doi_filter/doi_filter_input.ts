import { DOIInfo } from "../doi_info";
import { DOIInfoCollection } from "../doi_info";
import { DOIStatus } from "../doi_info";
import { DOIFilterResult } from "./doi_filter_result";
export class DOIFilterInput {
    public minimum_year: number | null = null;
    public maximum_year: number | null = null;
    public type: string | null = null;
    public authors: string[] = [];
    public tags: string[] = [];
    public volume: string | null = null;
    public container_title: string | null = null;
    public doiReferences: string[] = [];
    public status: DOIStatus | null = null;


    public static buildFromURLParameters(): DOIFilterInput {
        let r = new DOIFilterInput();
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
            }
        }
        return r;
    }

    
    private filter(collection: DOIInfoCollection, candidates: number[]): number[] {
        return candidates.filter(candidate => {
            const doiInfo = collection.getDOIInfo(candidate);
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
            if(this.status != null && doiInfo.status != this.status){
                return false;
            }
            return true;
        });
    }

    public search(doiInfoCollectionFilter: DOIFilterResult, collection: DOIInfoCollection): number[] {
        let r: number[] = doiInfoCollectionFilter.doiIDs.map(doiID => doiID);
        return this.filter(collection, r);
    }
    public is_empty(): boolean {
        return this.minimum_year == null && this.maximum_year == null && this.type == null && 
        this.authors.length == 0 && this.tags.length == 0 && this.volume == null && this.container_title == null 
        && this.doiReferences.length == 0 && this.status == null;
    }
    public copy(): DOIFilterInput {
        const r = new DOIFilterInput();
        r.minimum_year = this.minimum_year;
        r.maximum_year = this.maximum_year;
        r.type = this.type;
        r.authors = this.authors;
        r.tags = this.tags;
        r.volume = this.volume;
        r.container_title = this.container_title;
        r.doiReferences = this.doiReferences;
        r.status = this.status;
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
        if(this.status != null && doiInfo.status != this.status){
            return false;
        }
        return true;
    }

    public isIncluded(item : DOIFilterInput): boolean {
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
        if(this.type != null && item.type != null){
            if(this.type != item.type){
                return false;
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
        if(this.status != null && item.status != null){
            if(this.status != item.status){
                return false;
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
        if(this.status != null && item.status != null){
            if(this.status != item.status){
                return false;
            }
        }

        return true;
    }


}
