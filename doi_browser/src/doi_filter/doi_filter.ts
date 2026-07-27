import { DOIFilterQuery } from "./doi_filter_query";
import { DOIFilterViewSetting } from "./doi_filter_view_setting";
export class DOIFilter {
    public query: DOIFilterQuery = new DOIFilterQuery();
    public viewSetting: DOIFilterViewSetting = new DOIFilterViewSetting();

    /*
    public static buildFromURLParameters(): DOIFilter {        
        let r = new DOIFilter();
        r.query = DOIFilterQuery.buildFromURLParameters();
        r.viewSetting = DOIFilterViewSetting.buildFromURLParameters();
       return r;
    }
    */

    public copy(): DOIFilter {
        let r = new DOIFilter();
        r.query = this.query.copy();
        r.viewSetting = this.viewSetting.copy();
        return r;
    }


    public getHash(): string {
        var obj: any = {};
        obj.query = this.query.getHash();
        obj.viewSetting = this.viewSetting.getHash();
        return JSON.stringify(obj);
    }

    public static buildFromJSON(json: string): DOIFilter {
        var obj: any = JSON.parse(json);
        var r = new DOIFilter();
        r.query = DOIFilterQuery.buildFromJSON(obj.query);
        r.viewSetting = DOIFilterViewSetting.buildFromJSON(obj.viewSetting);
        return r;
    }
}