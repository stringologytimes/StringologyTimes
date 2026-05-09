import { DOIInfoCollection } from "../doi_info";
import { DOIInfo } from "../doi_info";
import { DOIFilterInput } from "./doi_filter_input";
import { DOIFilterWithViewSetting } from "./doi_filter_view_setting";

export class DOIFilterPartialResult {
    public doiIDs: number[] = [];
    public firstDOINumber: number = 0;

    public constructor(doiIDs: number[] | null, doiFilterWithViewSetting: DOIFilterWithViewSetting | null, collection: DOIInfoCollection) {
        if (doiIDs == null) {
            this.doiIDs = [];
            this.firstDOINumber = 0;
        } else {
            if(doiFilterWithViewSetting == null){
                this.doiIDs = doiIDs.map(doiID => doiID);
                this.firstDOINumber = 0;
            }else{
                if(doiFilterWithViewSetting.doiFilterViewSetting.viewMode == "article_list" && doiFilterWithViewSetting.doiFilterViewSetting.pageNumber != null && doiFilterWithViewSetting.doiFilterViewSetting.pageSize != null){                    
                    this.firstDOINumber = doiFilterWithViewSetting.doiFilterViewSetting.pageNumber * doiFilterWithViewSetting.doiFilterViewSetting.pageSize;
                    for(let i = this.firstDOINumber; i < this.firstDOINumber + doiFilterWithViewSetting.doiFilterViewSetting.pageSize; i++){
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