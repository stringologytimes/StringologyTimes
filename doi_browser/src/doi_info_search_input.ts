import { DOIInfo } from "./browser";
import { DOIInfoCollection } from "./browser";
import { DOIStatus } from "./browser";
import { DOIInfoCollectionFilter } from "./browser";
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

    
    private filter(collection: DOIInfoCollection, candidates: number[]): number[] {
        return candidates.filter(candidate => {
            const doiInfo = collection.getDOIInfo(candidate);
            if(this.minimum_year != null && doiInfo.year < this.minimum_year){
                return false;
            }
            if(this.maximum_year != null && doiInfo.year > this.maximum_year){
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

    public search(doiInfoCollectionFilter: DOIInfoCollectionFilter, collection: DOIInfoCollection): number[] {
        let r: number[] = doiInfoCollectionFilter.doiIDs.map(doiID => doiID);
        return this.filter(collection, r);
    }
    


}
