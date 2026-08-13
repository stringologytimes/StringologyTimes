import { DOIRecordCollection } from "./doi_record_collection";
import { renderViewSettingBox } from "./render/view_setting_box_render";
import { DOIFilterStandardRender } from "./render/doi_filter_standard_render";
import { SearchResultCache } from "./doi_filter/search_result_cache";
import { getDOIRecordTypeList } from "./doi_record_collection";
import { containerTypeList, paperTypeList } from "./doi_record";
//import { PrimarySearchFilterRender } from "./render/settings/primary_search_filter_render";
import { SearchFilter } from "./doi_filter/search_filter";
//import { SecondarySearchFilterRender } from "./render/settings/secondary_search_filter_render";
import { hideLoading, showLoading, yieldForPaint } from "./loading_overlay";
import { FoundRecordSummary } from "./doi_filter/found_record_summary";
import { SearchResultViewSettings } from "./doi_filter/search_result_view_settings";
import { AnyContainerType, AnyPaperType, AnyOtherType } from "./doi_record";
import { URLProcessor } from "./url_processor";
import { SearchFilterBoxFunctions } from "./render/settings/fieldset/search_filter_box_functions";
import { SearchResultSortOrder } from "./render/settings/fieldset/sort_order_functions";


export class BrowserInfo {
    public doiInfoCollection: DOIRecordCollection | null = null;
    public primarySearchFilter : SearchFilter = new SearchFilter();
    public primaryResultCache: Map<string, number[]> = new Map();
    public primaryResultSummaryCache: Map<string, FoundRecordSummary> = new Map();

    public secondarySearchFilter : SearchFilter = new SearchFilter();
    public sortOrder: SearchResultSortOrder = new SearchResultSortOrder();
    public finalResultCache: Map<string, number[]> = new Map();
    public finalResultSummaryCache: Map<string, FoundRecordSummary> = new Map();

    public viewSettings : SearchResultViewSettings = new SearchResultViewSettings();

    //public currentDOIFilter: DOIFilter = new DOIFilter();
    //public doiResultCache: DOIResultCache = new DOIResultCache();

    private getFianlHash(): string {
        return this.primarySearchFilter.getHash(true) + "-" + this.secondarySearchFilter.getHash(false) + "-" + this.sortOrder.getHash();
    }


    private renderMainWindow(): void {
        if (this.viewSettings.mode == "article_list") {
            const finalHash = this.getFianlHash();
            const foundRecordIDs = this.finalResultCache.get(finalHash)!;
            const startIndex = this.viewSettings.getItemIndex();
            const endIndex = Math.min(startIndex + this.viewSettings.pageSize!, foundRecordIDs.length);
            const foundRecordIDsPart = foundRecordIDs.slice(startIndex, endIndex);

            console.log("startIndex: " + startIndex + ", endIndex: " + endIndex + ", foundRecordIDsPart.length: " + foundRecordIDsPart.length);

            DOIFilterStandardRender.render(foundRecordIDsPart, startIndex, this.doiInfoCollection!);
        }
    }
    private renderFilterBoxes(updatePrimaryFilterBox: boolean, updateSecondaryFilterBox: boolean): void {
        if(updatePrimaryFilterBox){
            SearchFilterBoxFunctions.initializeFilterBox(true, this.doiInfoCollection!.recordSummary, this.doiInfoCollection!);
        }
        if(updateSecondaryFilterBox){
            const foundRecordSummary = this.primaryResultSummaryCache.get(this.primarySearchFilter.getHash(true))!;
            SearchFilterBoxFunctions.initializeFilterBox(false, foundRecordSummary, this.doiInfoCollection!);    
        }
    }
    private renderViewSettingBox(): void {
        renderViewSettingBox(this.viewSettings, this.finalResultCache.get(this.getFianlHash())!.length);
    }
    private renderSortOrderBox(): void {
        this.sortOrder.render();
    }

    private async processPrimarySearchFilter(): Promise<void> {
        const b1 = this.primaryResultCache.has(this.primarySearchFilter.getHash(true));
        const b2 = this.primaryResultSummaryCache.has(this.primarySearchFilter.getHash(true));

        if(!b1 || !b2){
            showLoading("Searching...");
            await yieldForPaint();

            try {
                if(!b1){
                    const foundRecordIDs = this.primarySearchFilter.search(this.doiInfoCollection!);
                    this.primaryResultCache.set(this.primarySearchFilter.getHash(true), foundRecordIDs);    
                }

                if(!b2){
                    const foundRecordIDs = this.primaryResultCache.get(this.primarySearchFilter.getHash(true))!;
                    const foundRecordSummary = this.doiInfoCollection!.buildRecordSummary(foundRecordIDs);
                    this.primaryResultSummaryCache.set(this.primarySearchFilter.getHash(true), foundRecordSummary);
                }
            } finally {
                hideLoading();
            }    
        }
    }
    private async processSecondarySearchFilterWithSortOrder(): Promise<void> {
        const finalHash = this.getFianlHash();
        const b1 = this.finalResultCache.has(finalHash);
        const b2 = this.finalResultSummaryCache.has(finalHash);

        if(!b1 || !b2){
            const recordIDs = this.primaryResultCache.get(this.primarySearchFilter.getHash(true))!;
            if(!b1){
                const foundRecordIDs = this.secondarySearchFilter.filter(this.doiInfoCollection!, recordIDs);
                this.sortOrder.sort(foundRecordIDs, this.doiInfoCollection!);
                this.finalResultCache.set(finalHash, foundRecordIDs);   
            }

            if(!b2){
                const foundRecordIDs = this.finalResultCache.get(finalHash)!;
                const foundRecordSummary = this.doiInfoCollection!.buildRecordSummary(foundRecordIDs);
                this.finalResultSummaryCache.set(finalHash, foundRecordSummary);
            }
        }
    }



    public getCurrentPrimarySummary(): FoundRecordSummary {
        return this.primaryResultSummaryCache.get(this.primarySearchFilter.getHash(true))!;
    }



    public async initialize(doiInfoCollection: DOIRecordCollection): Promise<void> {
        this.doiInfoCollection = doiInfoCollection;
        await this.rebuildFromURLParameters(true, true, true, true);
    }
    public async rebuildFromURLParameters(updatePrimaryFilterBox: boolean, updateSecondaryFilterBox: boolean, updateViewSettingBox: boolean, updateSortOrderBox: boolean): Promise<void> {
        this.primarySearchFilter = URLProcessor.buildSearchFilterFromURL(true);
        this.secondarySearchFilter = URLProcessor.buildSearchFilterFromURL(false);
        this.viewSettings = SearchResultViewSettings.buildFromURLParameters();
        this.sortOrder = SearchResultSortOrder.buildFromURLParameters();
        
        console.log("secondarySearchFilter: " + this.secondarySearchFilter.getHash(false));

        await this.processPrimarySearchFilter();
        await this.processSecondarySearchFilterWithSortOrder();

        this.renderFilterBoxes(updatePrimaryFilterBox, updateSecondaryFilterBox);
        this.renderMainWindow();
        if(updateViewSettingBox){
            this.renderViewSettingBox();
        }
        if(updateSortOrderBox){
            this.renderSortOrderBox();
        }

        const finalRecordCount = this.finalResultCache.get(this.getFianlHash())!.length;
        const searchResultMessageDiv = document.getElementById("search-result-message-div");
        if(searchResultMessageDiv != null){
            searchResultMessageDiv!.textContent = "Found " + finalRecordCount + " records";
        }



    }

    public async rebuildByChangingPrimarySearchFilterBox(): Promise<void> {
        const newParameters = SearchFilterBoxFunctions.convertInputToURLParameters(true);
        SearchFilterBoxFunctions.setURLParameters(true, newParameters);
        SearchFilterBoxFunctions.resetURLParameters(false);

        await this.rebuildFromURLParameters(false, true, true, true);
    }

    public async rebuildByChangingSecondarySearchFilterBox(): Promise<void> {
        const newParameters = SearchFilterBoxFunctions.convertInputToURLParameters(false);
        SearchFilterBoxFunctions.setURLParameters(false, newParameters);

        await this.rebuildFromURLParameters(false, false, true, true);
    }

    public async rebuildByChangingViewSettingBox(): Promise<void> {
        this.viewSettings = SearchResultViewSettings.convertHTMLElementToInstance();
        const newParameters = this.viewSettings.convertToURLParameters();
        URLProcessor.resetURLParameters(SearchResultViewSettings.getURLParameterKeys(), false);
        URLProcessor.setURLParameters(newParameters, true);
        await this.rebuildFromURLParameters(false, false, true, true);
    }

    

    public print(): void {
        /*
        console.log("cacheAssociatedWithDOIFilterHash: ");
        this.cacheAssociatedWithDOIFilterHash.forEach(([a, b], key) => {
            console.log(key + "/" + a.getHash() + "/" + b.doiIDs.length);
        });
        */
    }

    
}