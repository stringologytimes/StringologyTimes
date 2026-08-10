import { DOIRecordCollection } from "./doi_record_collection";
import { renderViewSettingBox } from "./render/settings/view_setting_box_render";
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


export class BrowserInfo {
    public doiInfoCollection: DOIRecordCollection | null = null;
    public primarySearchFilter : SearchFilter = new SearchFilter();
    public primaryResultCache: Map<string, number[]> = new Map();
    public primaryResultSummaryCache: Map<string, FoundRecordSummary> = new Map();

    public secondarySearchFilter : SearchFilter = new SearchFilter();
    public sortBy: string = "";
    public finalResultCache: Map<string, number[]> = new Map();
    public finalResultSummaryCache: Map<string, FoundRecordSummary> = new Map();

    public viewSetting : SearchResultViewSettings = new SearchResultViewSettings();

    //public currentDOIFilter: DOIFilter = new DOIFilter();
    //public doiResultCache: DOIResultCache = new DOIResultCache();

    private getFianlHash(): string {
        return this.primarySearchFilter.getHash() + "-" + this.secondarySearchFilter.getHash();
    }


    private renderMainWindow(): void {
        if (this.viewSetting.viewMode == "article_list") {
            const finalHash = this.getFianlHash();
            const foundRecordIDs = this.finalResultCache.get(finalHash)!;
            const startIndex = this.viewSetting.getItemIndex();
            const endIndex = Math.min(startIndex + this.viewSetting.pageSize!, foundRecordIDs.length);
            const foundRecordIDsPart = foundRecordIDs.slice(startIndex, endIndex);

            DOIFilterStandardRender.render(foundRecordIDsPart, startIndex, this.doiInfoCollection!);
        }
    }
    private renderFilterBoxes(updatePrimaryFilterBox: boolean, updateSecondaryFilterBox: boolean): void {
        if(updatePrimaryFilterBox){
            SearchFilterBoxFunctions.initializeFilterBox(true, this.doiInfoCollection!.recordSummary, this.doiInfoCollection!);
        }
        if(updateSecondaryFilterBox){
            const foundRecordSummary = this.primaryResultSummaryCache.get(this.primarySearchFilter.getHash())!;
            SearchFilterBoxFunctions.initializeFilterBox(false, foundRecordSummary, this.doiInfoCollection!);    
        }
    }

    private async processPrimarySearchFilter(): Promise<void> {
        const b1 = this.primaryResultCache.has(this.primarySearchFilter.getHash());
        const b2 = this.primaryResultSummaryCache.has(this.primarySearchFilter.getHash());

        if(!b1 || !b2){
            showLoading("Searching...");
            await yieldForPaint();

            try {
                if(!b1){
                    const foundRecordIDs = this.primarySearchFilter.search(this.doiInfoCollection!);
                    this.primaryResultCache.set(this.primarySearchFilter.getHash(), foundRecordIDs);    
                }

                if(!b2){
                    const foundRecordIDs = this.primaryResultCache.get(this.primarySearchFilter.getHash())!;
                    const foundRecordSummary = this.doiInfoCollection!.buildRecordSummary(foundRecordIDs);
                    this.primaryResultSummaryCache.set(this.primarySearchFilter.getHash(), foundRecordSummary);
                }
            } finally {
                hideLoading();
            }    
        }
    }
    private async processSecondarySearchFilter(): Promise<void> {
        const finalHash = this.getFianlHash();
        const b1 = this.finalResultCache.has(finalHash);
        const b2 = this.finalResultSummaryCache.has(finalHash);

        if(!b1 || !b2){
            const recordIDs = this.primaryResultCache.get(this.primarySearchFilter.getHash())!;
            if(!b1){
                const foundRecordIDs = this.secondarySearchFilter.filter(this.doiInfoCollection!, recordIDs);
                this.finalResultCache.set(finalHash, foundRecordIDs);   
            }

            if(!b2){
                const foundRecordIDs = this.finalResultCache.get(finalHash)!;
                const foundRecordSummary = this.doiInfoCollection!.buildRecordSummary(foundRecordIDs);
                this.finalResultSummaryCache.set(finalHash, foundRecordSummary);
            }
        }
    }






    public async initialize(doiInfoCollection: DOIRecordCollection): Promise<void> {
        this.doiInfoCollection = doiInfoCollection;
        await this.rebuildFromURLParameters(true, true);
    }
    public async rebuildFromURLParameters(updatePrimaryFilterBox: boolean, updateSecondaryFilterBox: boolean): Promise<void> {
        this.primarySearchFilter = URLProcessor.buildSearchFilterFromURL(true);
        this.secondarySearchFilter = URLProcessor.buildSearchFilterFromURL(false);

        await this.processPrimarySearchFilter();
        await this.processSecondarySearchFilter();

        this.renderFilterBoxes(updatePrimaryFilterBox, updateSecondaryFilterBox);
        this.renderMainWindow();



    }

    public async rebuildByChangingPrimarySearchFilterBox(): Promise<void> {
        const newParameters = SearchFilterBoxFunctions.convertInputToURLParameters(true);
        SearchFilterBoxFunctions.setURLParameters(true, newParameters);
        SearchFilterBoxFunctions.resetURLParameters(false);

        await this.rebuildFromURLParameters(false, true);
    }

    public async rebuildByChangingSecondarySearchFilterBox(): Promise<void> {
        const newParameters = SearchFilterBoxFunctions.convertInputToURLParameters(false);
        SearchFilterBoxFunctions.setURLParameters(false, newParameters);

        await this.rebuildFromURLParameters(false, false);
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