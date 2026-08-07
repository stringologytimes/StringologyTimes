//import { DOIFilter } from "./doi_filter";
import { PrimarySearchResult } from "./primary_search_result";
import { SummaryInfo } from "./summary_info";
import { DOIRecordCollection } from "../doi_record_collection";
import { SearchFilter } from "./search_filter";

export class SearchResultCache {
    //private doiFilterInputNumber: number = 0;
    //private doiFilterInputHashStack = new Array<string>();

    private doiFilterResultCache = new Map<string, PrimarySearchResult>();
    private summaryInfoCache = new Map<string, SummaryInfo>();

    
    //private cacheAssociatedWithDOIFilterHash = new Map<string, [DOIFilter]>();
    //private cacheAssociatedWithDOIQueryHash = new Map<string, [DOIFilterResult, SummaryInfo]>();
    

    /*
    public initialize(doiInfoCollection: DOIRecordCollection, currentDOIFilter: DOIFilter): void {
        this.doiFilterResultCache.clear();
        this.summaryInfoCache.clear();


        {
            const emptyDOIFilterWithViewSetting = new DOIFilter();
            const newDOIFilterResult = new PrimarySearchResult(null, doiInfoCollection!, "unordered");
            const summaryInfo = new SummaryInfo();
            summaryInfo.build(newDOIFilterResult, emptyDOIFilterWithViewSetting.query, doiInfoCollection!);

            this.doiFilterResultCache.set(emptyDOIFilterWithViewSetting.query.getHash(), newDOIFilterResult);
            this.summaryInfoCache.set(emptyDOIFilterWithViewSetting.query.getHash(), summaryInfo);
        }
    }
    */

    


    /*
    public search(doiInfoCollection: DOIRecordCollection, currentDOIFilter: DOIFilter) : [PrimarySearchResult, SummaryInfo] {
        var queryHash = currentDOIFilter.query.getHash();
        var b1 = this.doiFilterResultCache.has(queryHash);

        var queryHashWithViewSetting = currentDOIFilter.getHash();
        var b2 = this.summaryInfoCache.has(queryHashWithViewSetting);

        console.log("search: " + queryHash + "/" + queryHashWithViewSetting + "/" + b1 + "/" + b2);


        if(b1){
            var result = this.doiFilterResultCache.get(queryHash)!;
            if(b2){
                var summaryInfo = this.summaryInfoCache.get(queryHashWithViewSetting)!;
                return [result, summaryInfo];
            }else{
                var summaryInfo = new SummaryInfo();
                summaryInfo.build(result, currentDOIFilter.query, doiInfoCollection);
                this.summaryInfoCache.set(queryHashWithViewSetting, summaryInfo);
                return [result, summaryInfo];
            }
        }else{
            var parentQueries = currentDOIFilter.query.get_parents();
            var min_count = doiInfoCollection.length() + 1;
            var parentInfo : SearchFilter | null = null;
            for(const parentQuery of parentQueries){
                var parentHash = parentQuery.getHash();
                if(this.doiFilterResultCache.has(parentHash)){
                    var parentResult = this.doiFilterResultCache.get(parentHash)!;
                    if(parentResult.doiIDs.length < min_count){
                        min_count = parentResult.doiIDs.length;
                        parentInfo = parentQuery;
                    }
                }
            }

            if(parentInfo != null){
                console.log("parentInfo: " + parentInfo.getHash());
            }else{
                console.log("parentInfo: null");
            }


            if(parentInfo != null){
                var parentDOIFilterResult = this.doiFilterResultCache.get(parentInfo.getHash())!;
                const newDOIFilterResult = parentDOIFilterResult.search(currentDOIFilter.query, doiInfoCollection!);
                //const newDOIFilterResult = currentDOIFilter.query.search(parentDOIFilterResult, doiInfoCollection!);
                var newSummaryInfo = new SummaryInfo();
                newSummaryInfo.build(newDOIFilterResult, currentDOIFilter.query, doiInfoCollection);

                this.doiFilterResultCache.set(queryHash, newDOIFilterResult);
                this.summaryInfoCache.set(queryHashWithViewSetting, newSummaryInfo);
                return [newDOIFilterResult, newSummaryInfo];
            }else{
                const emptyDOIFilter = new DOIFilter();
                const emptyQueryHash = emptyDOIFilter.query.getHash();
                const emptyDOIFilterResult = this.doiFilterResultCache.get(emptyQueryHash)!;
                const newDOIFilterResult = emptyDOIFilterResult.search(currentDOIFilter.query, doiInfoCollection!);
                var newSummaryInfo = new SummaryInfo();
                newSummaryInfo.build(newDOIFilterResult, currentDOIFilter.query, doiInfoCollection);
                this.doiFilterResultCache.set(queryHash, newDOIFilterResult);
                this.summaryInfoCache.set(queryHashWithViewSetting, newSummaryInfo);
                return [newDOIFilterResult, newSummaryInfo];
            }
        }
    }
    */


    /*
    public processCurrentDOIFilterInput(doiInfoCollection: DOIRecordCollection, currentDOIFilter: DOIFilter): void {

        const currentDOIFilterWithViewSetting = currentDOIFilter.copy();
        const hash = currentDOIFilterWithViewSetting.getHash();
        const queryHash = currentDOIFilterWithViewSetting.query.getHash();

        this.search(doiInfoCollection, currentDOIFilterWithViewSetting);
    }
    */


}