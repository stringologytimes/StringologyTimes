import { DOIInfoCollection } from "./doi_info";
import { DOIInfo } from "./doi_info";
import { DOIFilterInput } from "./doi_filter_input";


export class BrowserInfo {
    public doiInfoCollection: DOIInfoCollection | null = null;
    public doiInfoSearchInput: DOIFilterInput = new DOIFilterInput();
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