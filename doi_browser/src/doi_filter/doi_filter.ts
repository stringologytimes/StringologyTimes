import { PrimarySearchFilter } from "./primary_search_filter";
import { SearchResultViewSettings } from "./search_result_view_settings";
export class DOIFilter {
    public query: PrimarySearchFilter = new PrimarySearchFilter();
    public viewSetting: SearchResultViewSettings = new SearchResultViewSettings();

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
        r.query = PrimarySearchFilter.buildFromJSON(obj.query);
        r.viewSetting = SearchResultViewSettings.buildFromJSON(obj.viewSetting);
        return r;
    }
}