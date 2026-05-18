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


export class BrowserInfo {
    public doiInfoCollection: DOIInfoCollection | null = null;
    //public pageNumber : number = -1;
    //public pageSize : number = 100;

    public currentDOIFilter: DOIFilter = new DOIFilter();
    public doiFilterInputNumber: number = 0;
    public doiFilterInputHashStack = new Array<string>();


    public cacheAssociatedWithDOIFilterHash = new Map<string, [DOIFilter]>();
    public cacheAssociatedWithDOIQueryHash = new Map<string, [DOIFilterResult, SummaryInfo]>();

    public initialize(doiInfoCollection: DOIInfoCollection): void {
        const emptyDOIFilterWithViewSetting = new DOIFilter();
        this.currentDOIFilter = emptyDOIFilterWithViewSetting.copy();
        this.doiInfoCollection = doiInfoCollection;
        this.doiFilterInputNumber = 0;
        this.doiFilterInputHashStack = [];
        this.doiFilterInputHashStack.push(this.currentDOIFilter.getHash());

        this.cacheAssociatedWithDOIFilterHash.clear();
        this.cacheAssociatedWithDOIQueryHash.clear();

        {
            const emptyDOIFilterWithViewSetting = new DOIFilter();
            const newDOIFilterResult = new DOIFilterResult(null, this.doiInfoCollection!, emptyDOIFilterWithViewSetting.query.sortBy);
            const summaryInfo = new SummaryInfo();
            summaryInfo.build(newDOIFilterResult, emptyDOIFilterWithViewSetting.query, this.doiInfoCollection!);

            this.cacheAssociatedWithDOIFilterHash.set(emptyDOIFilterWithViewSetting.getHash(), [emptyDOIFilterWithViewSetting.copy()]);
            this.cacheAssociatedWithDOIQueryHash.set(emptyDOIFilterWithViewSetting.query.getHash(), [newDOIFilterResult, summaryInfo]);
        }



    }


    public getCurrentDOIFilterWithViewSetting(): DOIFilter {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            return new DOIFilter();
        } else {
            const hash = this.doiFilterInputHashStack[this.doiFilterInputNumber];
            if (!this.cacheAssociatedWithDOIFilterHash.has(hash)) {
                throw new Error("No current DOI filter partial result");
            }
            const [result] = this.cacheAssociatedWithDOIFilterHash.get(hash)!;
            return result;
        }
    }
    public getCurrentDOIFilterResult(): DOIFilterResult {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            throw new Error("No current DOI filter result");
        } else {
            const rp = this.getCurrentDOIFilterWithViewSetting();
            const hash = rp.query.getHash();
            if (!this.cacheAssociatedWithDOIQueryHash.has(hash)) {
                throw new Error("No current DOI filter result");
            }
            const [result, _] = this.cacheAssociatedWithDOIQueryHash.get(hash)!;
            return result;
        }
    }
    public getCurrentSummaryInfo(): SummaryInfo {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            throw new Error("No current DOI filter result");
        } else {
            const rp = this.getCurrentDOIFilterWithViewSetting();
            const hash = rp.query.getHash();
            if (!this.cacheAssociatedWithDOIQueryHash.has(hash)) {
                throw new Error("No current DOI filter result");
            }
            const [_, result] = this.cacheAssociatedWithDOIQueryHash.get(hash)!;
            return result;
        }
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
        var keywords = url.searchParams.get("keywords");
        if (keywords) {
            this.currentDOIFilter.query.keywords = keywords;
        }else{
            this.currentDOIFilter.query.keywords = null;
        }
    }


    public processCurrentDOIFilterInput(): void {


        if (this.doiInfoCollection != null) {
            const currentDOIFilterWithViewSetting = this.currentDOIFilter.copy();
            const hash = currentDOIFilterWithViewSetting.getHash();
            const queryHash = currentDOIFilterWithViewSetting.query.getHash();


            while (this.doiFilterInputHashStack.length - 1 > this.doiFilterInputNumber && this.doiFilterInputHashStack.length > 0) {

                this.doiFilterInputHashStack.shift();
            }

            this.doiFilterInputHashStack.push(hash);
            this.doiFilterInputNumber = this.doiFilterInputHashStack.length - 1;
            this.cacheAssociatedWithDOIFilterHash.set(hash, [this.currentDOIFilter.copy()]);
            //this.doiFilterWithViewSettingMap.set(hash, this.currentDOIFilterWithViewSetting);

            console.log("queryHash: " + queryHash);

            if (!this.cacheAssociatedWithDOIQueryHash.has(queryHash)) {
                if (this.doiFilterInputNumber > 0) {                    
                    const parentHash = this.doiFilterInputHashStack[this.doiFilterInputNumber - 1];
                    const [parentDOIFilter] = this.cacheAssociatedWithDOIFilterHash.get(parentHash)!;
                    if (this.currentDOIFilter.query.isIncluded(parentDOIFilter.query)) {
                        const [parentDOIFilterResult, _] = this.cacheAssociatedWithDOIQueryHash.get(parentDOIFilter.query.getHash())!;
                        const newDOIFilterResult = parentDOIFilterResult.search(this.currentDOIFilter.query, this.doiInfoCollection!);
                        console.log("newDOIFilterResult.doiIDs.length: " + newDOIFilterResult.doiIDs.length);
                        const newSummaryInfo = new SummaryInfo();
                        newSummaryInfo.build(newDOIFilterResult, this.currentDOIFilter.query, this.doiInfoCollection!);
                        this.cacheAssociatedWithDOIQueryHash.set(queryHash, [newDOIFilterResult, newSummaryInfo]);
                    } else {
                        const emptyDOIFilter = new DOIFilter();
                        const emptyQueryHash = emptyDOIFilter.query.getHash();
                        const [emptyDOIFilterResult, _] = this.cacheAssociatedWithDOIQueryHash.get(emptyQueryHash)!;

                        const newDOIFilterResult = emptyDOIFilterResult.search(this.currentDOIFilter.query, this.doiInfoCollection!);
                        const newSummaryInfo = new SummaryInfo();

                        console.log("newDOIFilterResult.doiIDs.length: " + newDOIFilterResult.doiIDs.length);


                        newSummaryInfo.build(newDOIFilterResult, this.currentDOIFilter.query, this.doiInfoCollection!);
                        this.cacheAssociatedWithDOIQueryHash.set(queryHash, [newDOIFilterResult, newSummaryInfo]);
                    }
                } else {
                    throw new Error("No parent DOI filter result");
                }
            }

            if(!this.cacheAssociatedWithDOIFilterHash.has(hash)){
                throw new Error("Logic error");
            }
            if(!this.cacheAssociatedWithDOIQueryHash.has(queryHash)){
                throw new Error("Logic error");
            }
        }

        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            throw new Error("Logic error");
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
            console.log("renderFilterBox time: " + (renderStartTime2 - renderStartTime1));

            renderViewSettingBox(currentDOIFilterWithViewSetting.viewSetting, currentSummaryInfo);
            const renderStartTime3 = performance.now();
            console.log("renderViewSettingBox time: " + (renderStartTime3 - renderStartTime2));

            if (currentDOIFilterWithViewSetting.viewSetting.viewMode == "article_list") {
                DOIFilterStandardRender.render(currentDOIFilterResult, currentDOIFilterWithViewSetting.viewSetting.getItemIndex(), currentDOIFilterWithViewSetting.viewSetting.pageSize!, this.doiInfoCollection!);
            }
            else if (currentDOIFilterWithViewSetting.viewSetting.viewMode == "container_title_list") {
                renderContainerTitleList(currentDOIFilterResult, currentDOIFilterWithViewSetting.viewSetting, currentSummaryInfo);
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

    public debugPrint(): void {
        console.log("cacheAssociatedWithDOIFilterHash: ");
        for(const key of this.cacheAssociatedWithDOIQueryHash.keys()){
            const [a, b] = this.cacheAssociatedWithDOIQueryHash.get(key)!;
            console.log(key + "/" + a.doiIDs.length);
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