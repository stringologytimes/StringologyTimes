import { DOIRecordCollection } from "./doi_record_collection";
import { renderViewSettingBox } from "./render/settings/view_setting_box_render";
import { DOIFilterStandardRender } from "./render/doi_filter_standard_render";
import { SearchResultCache } from "./doi_filter/search_result_cache";
import { getDOIRecordTypeList } from "./doi_record_collection";
import { containerTypeList, paperTypeList } from "./doi_record";
import { PrimarySearchFilterRender } from "./render/settings/primary_search_filter_render";
import { SearchFilter } from "./doi_filter/search_filter";
import { SecondarySearchFilterRender } from "./render/settings/secondary_search_filter_render";
import { hideLoading, showLoading, yieldForPaint } from "./loading_overlay";
import { FoundRecordSummary } from "./doi_filter/found_record_summary";
import { SearchResultViewSettings } from "./doi_filter/search_result_view_settings";


export class BrowserInfo {
    public doiInfoCollection: DOIRecordCollection | null = null;
    public primarySearchFilter : SearchFilter | null = null;
    public primaryResultIDs : number[] = [];
    public primaryResultSummary : FoundRecordSummary = new FoundRecordSummary();

    public secondarySearchFilter : SearchFilter | null = null;
    public secondaryResultIDs : number[] = [];
    public secondaryResultSummary : FoundRecordSummary = new FoundRecordSummary();

    public viewSetting : SearchResultViewSettings = new SearchResultViewSettings();

    //public currentDOIFilter: DOIFilter = new DOIFilter();
    //public doiResultCache: DOIResultCache = new DOIResultCache();

    public initialize(doiInfoCollection: DOIRecordCollection): void {
        //const emptyDOIFilterWithViewSetting = new DOIFilter();
        this.doiInfoCollection = doiInfoCollection;
        
        /*
        this.currentDOIFilter = emptyDOIFilterWithViewSetting.copy();
        this.doiResultCache.initialize(doiInfoCollection, this.currentDOIFilter);
        */

        PrimarySearchFilterRender.initialize(doiInfoCollection);


    }


    /*
    public getCurrentDOIFilterWithViewSetting(): DOIFilter {
        return this.currentDOIFilter;
    }

    public getCurrentDOIFilterResult(): PrimarySearchResult {
        var [result, _] = this.doiResultCache.search(this.doiInfoCollection!, this.currentDOIFilter);
        return result;
    }    



    public getCurrentSummaryInfo(): SummaryInfo {
        var [_, summaryInfo] = this.doiResultCache.search(this.doiInfoCollection!, this.currentDOIFilter);
        return summaryInfo;
    }

    public setCurrentDOIFilterWithViewSetting(doiFilterWithViewSetting: DOIFilter): void {
        this.currentDOIFilter = doiFilterWithViewSetting.copy();
    }
    */

    public getTypesFromURLParameters(): string[] {
        const url = new URL(window.location.href);
        var types = url.searchParams.getAll("type");
        if (types.length > 0) {
            if(types.includes("Null")){
                return [];
            }else{
                var result : string[] = [];
                types.forEach(type => {
                    if(type == "Container-Any"){
                        containerTypeList.forEach(type => {
                            result.push(type);
                        });
                    }else if(type == "Paper-Any"){
                        paperTypeList.forEach(type => {
                            result.push(type);
                        });
                    }
                    else if(type == "Other-Any"){
                        throw new Error("Other-Any is not supported");
                    }
                    else{
                        result.push(type);
                    }
                });
                return result;
            }
        }else{
            return getDOIRecordTypeList();
        }
    }



    public processURLParameters(): void {
        const url = new URL(window.location.href);
        /*
        this.currentDOIFilter.query.types = this.getTypesFromURLParameters();

        
        var containerTitle = url.searchParams.get("container_title");
        if (containerTitle) {
            this.currentDOIFilter.query.container_title = containerTitle;
        }else{
            this.currentDOIFilter.query.container_title = null;
        }
        

        
        var seriesTitle = url.searchParams.get("series_title");
        if (seriesTitle) {
            this.currentDOIFilter.query.series_title = seriesTitle;
        }else{
            this.currentDOIFilter.query.series_title = null;
        }
        

        var minimumYear = url.searchParams.get("minimum_year");
        if (minimumYear) {
            this.currentDOIFilter.query.minimum_year = parseInt(minimumYear);
        }else{
            this.currentDOIFilter.query.minimum_year = null;
        }
        var maximumYear = url.searchParams.get("maximum_year");
        if (maximumYear) {
            this.currentDOIFilter.query.maximum_year = parseInt(maximumYear);
        }else{
            this.currentDOIFilter.query.maximum_year = null;
        }

        
        var sortBy = url.searchParams.get("sort_by");
        if (sortBy) {
            this.currentDOIFilter.query.sortBy = sortBy as SortByType;
        }else{
            this.currentDOIFilter.query.sortBy = "unordered";
        }
        


        var tags = url.searchParams.getAll("tag");
        if (tags.length > 0) {
            this.currentDOIFilter.query.tags = tags;
        }else{
            this.currentDOIFilter.query.tags = [];
        }
        var excludeStatus = url.searchParams.getAll("exclude_status");
        this.currentDOIFilter.query.excludeStatus = [];

        this.currentDOIFilter.query.excludeStatus = excludeStatus.map(status => status as DOIStatus);

        var viewMode = url.searchParams.get("view_mode");
        if (viewMode) {
            this.currentDOIFilter.viewSetting.viewMode = viewMode as ViewModeType;
        }else{
            this.currentDOIFilter.viewSetting.viewMode = "article_list";
        }

        var pageSize = url.searchParams.get("page_size");
        if (pageSize) {
            this.currentDOIFilter.viewSetting.pageSize = parseInt(pageSize);
        }else{
            this.currentDOIFilter.viewSetting.pageSize = 100;
        }

        var pageNumber = url.searchParams.get("page_number");
        if (pageNumber) {
            this.currentDOIFilter.viewSetting.pageNumber = parseInt(pageNumber);
        }else{
            this.currentDOIFilter.viewSetting.pageNumber = 0;
        }
        var keywords = url.searchParams.getAll("keyword");
        this.currentDOIFilter.query.keywords = keywords.map(keyword => keyword);
        */
    }


    /*
    public processCurrentDOIFilterInput(): void {


        if (this.doiInfoCollection != null) {
            this.doiResultCache.processCurrentDOIFilterInput(this.doiInfoCollection, this.currentDOIFilter);

        }
    }
    */
    public print(): void {
        /*
        console.log("cacheAssociatedWithDOIFilterHash: ");
        this.cacheAssociatedWithDOIFilterHash.forEach(([a, b], key) => {
            console.log(key + "/" + a.getHash() + "/" + b.doiIDs.length);
        });
        */
    }

    public async render(PrimarySearchFilter: SearchFilter): Promise<void> {
        if (this.doiInfoCollection != null) {
            this.primarySearchFilter = PrimarySearchFilter;
            //const currentDOIFilterWithViewSetting = this.getCurrentDOIFilterWithViewSetting();

            showLoading("Searching...");
            await yieldForPaint();
            let foundRecordIDs: number[];

            const psfStartTime = performance.now();            
            try {
                foundRecordIDs = PrimarySearchFilter.search(this.doiInfoCollection!);
            } finally {
                hideLoading();
            }

            const psfEndTime = performance.now();            
            console.log("render-PrimarySearchFilter time: " + (psfEndTime - psfStartTime) + " ms, " + "foundRecordIDs: " + foundRecordIDs.length);
            const foundRecordSummary = this.doiInfoCollection!.buildRecordSummary(foundRecordIDs);

            const ssfStartTime = performance.now();            
            SecondarySearchFilterRender.initialize(foundRecordIDs, foundRecordSummary, this.doiInfoCollection!);
            const ssfEndTime = performance.now();
            console.log("render-SecondarySearchFilterRender time: " + (ssfEndTime - ssfStartTime) + " ms");

            renderViewSettingBox(this.viewSetting, foundRecordIDs.length);
            const renderStartTime3 = performance.now();
            //console.log("renderViewSettingBox time: " + (renderStartTime3 - renderStartTime2) + " ms");

            if (this.viewSetting.viewMode == "article_list") {
                const startIndex = this.viewSetting.getItemIndex();
                const endIndex = Math.min(startIndex + this.viewSetting.pageSize!, foundRecordIDs.length);
                const foundRecordIDsPart = foundRecordIDs.slice(startIndex, endIndex);

                DOIFilterStandardRender.render(foundRecordIDsPart, startIndex, this.doiInfoCollection!);
            }

            const renderStartTime4 = performance.now();
            console.log("render-DOIFilterMainBoxRender time: " + (renderStartTime4 - renderStartTime3) + " ms, pageSize: " + this.viewSetting.pageSize);

        }        
    }


}