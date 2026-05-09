import { DOIInfoCollection } from "./doi_info";
import { DOIInfo } from "./doi_info";
import { DOIFilterQuery } from "./doi_filter/doi_filter_query";
import { DOIFilterPartialResult } from "./doi_filter/doi_filter_partial_result";
import { DOIFilterResult } from "./doi_filter/doi_filter_result";
import { DOIFilterViewSetting } from "./doi_filter/doi_filter_view_setting";
import { DOIFilter } from "./doi_filter/doi_filter";
import { SummaryInfo } from "./doi_filter/summary_info";


export class BrowserInfo {
    public doiInfoCollection: DOIInfoCollection | null = null;
    //public pageNumber : number = -1;
    //public pageSize : number = 100;

    public currentDOIFilterWithViewSetting: DOIFilter = new DOIFilter();
    public doiFilterInputNumber: number = 0;
    public doiFilterInputHashStack = new Array<string>();

    public cacheAssociatedWithDOIFilterHash = new Map<string, [DOIFilter, DOIFilterPartialResult]>();
    public cacheAssociatedWithQueryHash = new Map<string, [DOIFilterResult, SummaryInfo]>();

    public initialize(doiInfoCollection: DOIInfoCollection): void {
        const emptyDOIFilterWithViewSetting = new DOIFilter();
        this.currentDOIFilterWithViewSetting = emptyDOIFilterWithViewSetting.copy();
        this.doiInfoCollection = doiInfoCollection;
        this.doiFilterInputNumber = 0;
        this.doiFilterInputHashStack = [];
        this.doiFilterInputHashStack.push(this.currentDOIFilterWithViewSetting.getHash());

        this.cacheAssociatedWithDOIFilterHash.clear();
        this.cacheAssociatedWithQueryHash.clear();

        {
            const emptyDOIFilterWithViewSetting = new DOIFilter();
            const newDOIFilterResult = new DOIFilterResult(null, this.doiInfoCollection!);
            const partialResult = new DOIFilterPartialResult(newDOIFilterResult.doiIDs, emptyDOIFilterWithViewSetting, this.doiInfoCollection!);
            const summaryInfo = new SummaryInfo();
            summaryInfo.build(newDOIFilterResult, emptyDOIFilterWithViewSetting.query, this.doiInfoCollection!);

            this.cacheAssociatedWithDOIFilterHash.set(emptyDOIFilterWithViewSetting.getHash(), [emptyDOIFilterWithViewSetting.copy(), partialResult]);
            this.cacheAssociatedWithQueryHash.set(emptyDOIFilterWithViewSetting.query.getHash(), [newDOIFilterResult, summaryInfo]);
        }


        
    }


    public getCurrentDOIFilterPartialResult(): DOIFilterPartialResult {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            return new DOIFilterPartialResult([], null, this.doiInfoCollection!);
        } else {
            const hash = this.doiFilterInputHashStack[this.doiFilterInputNumber];
            if(!this.cacheAssociatedWithDOIFilterHash.has(hash)){
                throw new Error("No current DOI filter partial result");
            }
            const [_, result] = this.cacheAssociatedWithDOIFilterHash.get(hash)!;
            return result;
        }
    }
    public getCurrentDOIFilterWithViewSetting(): DOIFilter {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            return new DOIFilter();
        } else {
            const hash = this.doiFilterInputHashStack[this.doiFilterInputNumber];
            if(!this.cacheAssociatedWithDOIFilterHash.has(hash)){
                throw new Error("No current DOI filter partial result");
            }
            const [result, _] = this.cacheAssociatedWithDOIFilterHash.get(hash)!;
            return result;
        }
    }
    public getCurrentDOIFilterResult(): DOIFilterResult {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            throw new Error("No current DOI filter result");
        } else {            
            const rp = this.getCurrentDOIFilterWithViewSetting();
            const hash = rp.query.getHash();
            if(!this.cacheAssociatedWithQueryHash.has(hash)){
                throw new Error("No current DOI filter result");
            }
            const [result, _] = this.cacheAssociatedWithQueryHash.get(hash)!;
            return result;
        }
    }
    public getCurrentSummaryInfo(): SummaryInfo {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            throw new Error("No current DOI filter result");
        } else {            
            const rp = this.getCurrentDOIFilterWithViewSetting();
            const hash = rp.query.getHash();
            if(!this.cacheAssociatedWithQueryHash.has(hash)){
                throw new Error("No current DOI filter result");
            }
            const [_, result] = this.cacheAssociatedWithQueryHash.get(hash)!;
            return result;
        }
    }
    public setCurrentDOIFilterWithViewSetting(doiFilterWithViewSetting: DOIFilter): void {
        this.currentDOIFilterWithViewSetting = doiFilterWithViewSetting.copy();
    }

    public processCurrentDOIFilterInput(): void {


        if (this.doiInfoCollection != null) {
            const currentDOIFilterWithViewSetting = this.currentDOIFilterWithViewSetting.copy();
            const hash = currentDOIFilterWithViewSetting.getHash();
            const queryHash = currentDOIFilterWithViewSetting.query.getHash();

            while (this.doiFilterInputHashStack.length - 1 > this.doiFilterInputNumber && this.doiFilterInputHashStack.length > 0) {
                
                this.doiFilterInputHashStack.shift();
            }

            this.doiFilterInputHashStack.push(hash);
            this.doiFilterInputNumber = this.doiFilterInputHashStack.length - 1;
            //this.doiFilterWithViewSettingMap.set(hash, this.currentDOIFilterWithViewSetting);

            if(!this.cacheAssociatedWithQueryHash.has(queryHash)){
                if (this.doiFilterInputNumber > 0) {
                    const parentHash = this.doiFilterInputHashStack[this.doiFilterInputNumber - 1];
                    const [parentDOIFilter, _] = this.cacheAssociatedWithDOIFilterHash.get(parentHash)!;
                    if(parentDOIFilter.query.isIncluded(this.currentDOIFilterWithViewSetting.query)){
                        const [parentDOIFilterResult, __] = this.cacheAssociatedWithQueryHash.get(parentDOIFilter.query.getHash())!;
                        const newDOIFilterResult = parentDOIFilterResult.search(this.currentDOIFilterWithViewSetting.query, this.doiInfoCollection!);
                        const newSummaryInfo = new SummaryInfo();
                        newSummaryInfo.build(newDOIFilterResult, this.currentDOIFilterWithViewSetting.query, this.doiInfoCollection!);
                        this.cacheAssociatedWithQueryHash.set(queryHash, [newDOIFilterResult, newSummaryInfo]);
                    }else{
                        const emptyDOIFilter = new DOIFilter();
                        const emptyQueryHash = emptyDOIFilter.query.getHash();
                        const [emptyDOIFilterResult, _] = this.cacheAssociatedWithQueryHash.get(emptyQueryHash)!;

                        const newDOIFilterResult = emptyDOIFilterResult.search(this.currentDOIFilterWithViewSetting.query, this.doiInfoCollection!);
                        const newSummaryInfo = new SummaryInfo();
                        newSummaryInfo.build(newDOIFilterResult, this.currentDOIFilterWithViewSetting.query, this.doiInfoCollection!);
                        this.cacheAssociatedWithQueryHash.set(queryHash, [newDOIFilterResult, newSummaryInfo]);
                    }
                }else{
                    throw new Error("No parent DOI filter result");
                }
            }

            const [currentDOIFilterResult, currentSummaryInfo] = this.cacheAssociatedWithQueryHash.get(queryHash)!;
            const currentDOIFilterPartialResult = new DOIFilterPartialResult(currentDOIFilterResult.doiIDs, this.currentDOIFilterWithViewSetting, this.doiInfoCollection!);
            this.cacheAssociatedWithDOIFilterHash.set(hash, [this.currentDOIFilterWithViewSetting.copy(), currentDOIFilterPartialResult]);
        }

        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length){
            throw new Error("Logic error");
        }
    }
    public print(): void {
        console.log("cacheAssociatedWithDOIFilterHash: ");
        this.cacheAssociatedWithDOIFilterHash.forEach(([a, b], key) => {
            console.log(key + "/" + a.getHash() + "/" + b.doiIDs.length);
        });
        console.log("cacheAssociatedWithQueryHash: ");
        this.cacheAssociatedWithQueryHash.forEach((value, key) => {
            console.log(key);
        });
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