import { DOIFilterQuery } from "./doi_filter_query";
import { DOIFilterViewSetting } from "./doi_filter_view_setting";
export class DOIFilter {
    public query: DOIFilterQuery = new DOIFilterQuery();
    public viewSetting: DOIFilterViewSetting = new DOIFilterViewSetting();

    public static buildFromURLParameters(): DOIFilter {        
        let r = new DOIFilter();
        r.query = DOIFilterQuery.buildFromURLParameters();
        r.viewSetting = DOIFilterViewSetting.buildFromURLParameters();
       return r;
    }

    public copy(): DOIFilter {
        let r = new DOIFilter();
        r.query = this.query.copy();
        r.viewSetting = this.viewSetting.copy();
        return r;
    }


    public getHash(): string {
        return this.query.getHash() + this.viewSetting.getHash();
    }
}