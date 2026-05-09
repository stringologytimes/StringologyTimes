import { DOIInfoCollection } from "../doi_info";
import { DOIInfo } from "../doi_info";
import { DOIFilterQuery } from "./doi_filter_query";
import { DOIFilter } from "./doi_filter";

export class DOIFilterPartialResult {
    public doiIDs: number[] = [];
    public firstDOINumber: number = 0;

    public constructor(doiIDs: number[] | null, doiFilterWithViewSetting: DOIFilter | null, collection: DOIInfoCollection) {
        if (doiIDs == null) {
            this.doiIDs = [];
            this.firstDOINumber = 0;
        } else {
            if(doiFilterWithViewSetting == null){
                this.doiIDs = doiIDs.map(doiID => doiID);
                this.firstDOINumber = 0;
            }else{
                if(doiFilterWithViewSetting.viewSetting.viewMode == "article_list" && doiFilterWithViewSetting.viewSetting.pageNumber != null && doiFilterWithViewSetting.viewSetting.pageSize != null){                    
                    this.firstDOINumber = doiFilterWithViewSetting.viewSetting.pageNumber * doiFilterWithViewSetting.viewSetting.pageSize;
                    for(let i = this.firstDOINumber; i < this.firstDOINumber + doiFilterWithViewSetting.viewSetting.pageSize; i++){
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