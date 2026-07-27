import { DOIInfoCollection } from "./doi_info";
import { DOIFilterResult } from "./doi_filter/doi_filter_result";
import { DOIFilter } from "./doi_filter/doi_filter";
import { SummaryInfo } from "./doi_filter/summary_info";
import { renderFilterBox } from "./render/doi_filter_box_render";
import { renderViewSettingBox } from "./render/view_setting_box_render";
import { DOIFilterStandardRender } from "./render/doi_filter_standard_render";
import { renderContainerTitleList } from "./render/doi_filter_container_title_render";
import { SortByType } from "./doi_filter/doi_filter_query";
import { DOIStatus } from "./doi_info";
import { ViewModeType } from "./doi_filter/doi_filter_view_setting";
import { renderSeriesTitleList } from "./render/doi_filter_series_title_render";
import { DOIResultCache } from "./doi_filter/doi_result_cache";


export class BrowserInfo {
    public doiInfoCollection: DOIInfoCollection | null = null;
    //public pageNumber : number = -1;
    //public pageSize : number = 100;

    public currentDOIFilter: DOIFilter = new DOIFilter();
    public doiResultCache: DOIResultCache = new DOIResultCache();

    public initialize(doiInfoCollection: DOIInfoCollection): void {
        const emptyDOIFilterWithViewSetting = new DOIFilter();
        this.currentDOIFilter = emptyDOIFilterWithViewSetting.copy();
        this.doiInfoCollection = doiInfoCollection;
        this.doiResultCache.initialize(doiInfoCollection, this.currentDOIFilter);


    }


    public getCurrentDOIFilterWithViewSetting(): DOIFilter {
        return this.currentDOIFilter;
    }

    public getCurrentDOIFilterResult(): DOIFilterResult {
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

    public processURLParameters(): void {
        const url = new URL(window.location.href);
        var type = url.searchParams.get("type");
        if (type) {
            this.currentDOIFilter.query.type = type;
        }else{
            this.currentDOIFilter.query.type = null;
        }

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
    }


    public processCurrentDOIFilterInput(): void {


        if (this.doiInfoCollection != null) {
            this.doiResultCache.processCurrentDOIFilterInput(this.doiInfoCollection, this.currentDOIFilter);

        }


    }
    public print(): void {
        /*
        console.log("cacheAssociatedWithDOIFilterHash: ");
        this.cacheAssociatedWithDOIFilterHash.forEach(([a, b], key) => {
            console.log(key + "/" + a.getHash() + "/" + b.doiIDs.length);
        });
        */
    }

    public render(): void {
        if (this.doiInfoCollection != null) {

            const currentDOIFilterWithViewSetting = this.getCurrentDOIFilterWithViewSetting();
            const currentDOIFilterResult = this.getCurrentDOIFilterResult();
            const currentSummaryInfo = this.getCurrentSummaryInfo();


            console.log("Render start");
            const renderStartTime1 = performance.now();
            renderFilterBox(currentDOIFilterResult, currentDOIFilterWithViewSetting.query, this.doiInfoCollection!, currentSummaryInfo);
            const renderStartTime2 = performance.now();
            console.log("renderFilterBox time: " + (renderStartTime2 - renderStartTime1) + " ms");

            renderViewSettingBox(currentDOIFilterWithViewSetting.viewSetting, currentSummaryInfo);
            const renderStartTime3 = performance.now();
            console.log("renderViewSettingBox time: " + (renderStartTime3 - renderStartTime2) + " ms");

            if (currentDOIFilterWithViewSetting.viewSetting.viewMode == "article_list") {
                DOIFilterStandardRender.render(currentDOIFilterResult, currentDOIFilterWithViewSetting.viewSetting.getItemIndex(), currentDOIFilterWithViewSetting.viewSetting.pageSize!, this.doiInfoCollection!);
            }
            else if (currentDOIFilterWithViewSetting.viewSetting.viewMode == "container_title_list") {
                renderContainerTitleList(currentDOIFilterResult, currentDOIFilterWithViewSetting.viewSetting, currentSummaryInfo);
            }
            else if (currentDOIFilterWithViewSetting.viewSetting.viewMode == "series_title_list") {
                console.log(currentSummaryInfo);
                renderSeriesTitleList(currentDOIFilterResult, currentDOIFilterWithViewSetting.viewSetting, currentSummaryInfo);
            }
            else {
                throw new Error("Unknown view mode");
            }
            const renderStartTime4 = performance.now();
            console.log("DOIFilterMainBoxRender time: " + (renderStartTime4 - renderStartTime3));

            //Render.render(browserInfo);
            //updatePaginationControls(browserInfo);
        }        
    }





    //public searchCountCache: Map<string, number> = new Map();
    //public idSequenceCache : Map<number, number[]> = new Map();

    /*

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
    */
}