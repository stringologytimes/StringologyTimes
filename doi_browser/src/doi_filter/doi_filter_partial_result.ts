import { DOIInfoCollection } from "../doi_info";
import { DOIInfo } from "../doi_info";
import { DOIFilterInput } from "./doi_filter_input";

export class DOIFilterPartialResult {
    public doiIDs: number[] = [];
    public firstDOINumber: number = 0;

    public constructor(doiIDs: number[] | null, doiFilterInput: DOIFilterInput | null, collection: DOIInfoCollection) {
        if (doiIDs == null) {
            this.doiIDs = [];
            this.firstDOINumber = 0;
        } else {
            if(doiFilterInput == null){
                this.doiIDs = doiIDs.map(doiID => doiID);
                this.firstDOINumber = 0;
            }else{
                if(doiFilterInput.viewMode == "article_list" && doiFilterInput.pageNumber != null && doiFilterInput.pageSize != null){                    
                    this.firstDOINumber = doiFilterInput.pageNumber * doiFilterInput.pageSize;
                    for(let i = this.firstDOINumber; i < this.firstDOINumber + doiFilterInput.pageSize; i++){
                        if(i < doiIDs.length){
                            this.doiIDs.push(doiIDs[i]);
                        }else{
                            break;
                        }
                    }
                }else{
                    this.doiIDs = doiIDs.map(doiID => doiID);
                    this.firstDOINumber = 0;    
                }

            }


        }
    }
}