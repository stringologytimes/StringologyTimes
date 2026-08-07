import { SearchFilter } from "./search_filter";
import { SearchResultViewSettings } from "./search_result_view_settings";

/*
export class DOIFilter {
    public query: SearchFilter = new SearchFilter();
    public viewSetting: SearchResultViewSettings = new SearchResultViewSettings();


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
        r.query = SearchFilter.buildFromJSON(obj.query);
        r.viewSetting = SearchResultViewSettings.buildFromJSON(obj.viewSetting);
        return r;
    }
}
*/