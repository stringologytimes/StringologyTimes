import { DOIInfoCollection } from "./browser";
import { DOIInfo } from "./browser";
import { DOIStatus } from "./browser";

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

    private filter(conditions: DOIInfoCollection, candidates: DOIInfo[]): DOIInfo[] {
        let r: DOIInfo[] = candidates.map(candidate => candidate);
        if(this.minimum_year != null || this.maximum_year != null){
            r = conditions.searchByYear(this.minimum_year, this.maximum_year, r);
        }
        
        if (this.authors.length > 0){
            r = conditions.searchByAuthors(this.authors, r);
        }
        
        if(this.container_title != null){
            r = conditions.searchByContainerTitle(this.container_title, r);
        }
        
        if(this.doiReferences.length > 0){
            r = conditions.searchByDOIReferences(this.doiReferences, r);
        }
        
        if(this.status != null){
            throw new Error("status is not supported yet");
        }
        return r;
    }

    public search(conditions: DOIInfoCollection): DOIInfo[] {
        let r: DOIInfo[] = [];
        if(this.minimum_year != null || this.maximum_year != null){
            r = conditions.searchByYear(this.minimum_year, this.maximum_year);
        }else if (this.authors.length > 0){
            r = conditions.searchByAuthors(this.authors, r);
        }else if(this.container_title != null){
            r = conditions.searchByContainerTitle(this.container_title, r);
        }else if(this.doiReferences.length > 0){
            r = conditions.searchByDOIReferences(this.doiReferences, r);
        }
        return this.filter(conditions, r);
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