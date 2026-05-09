import { DOIInfoCollection } from "./doi_info";
import { DOIInfo } from "./doi_info";
import { DOIFilterInput } from "./doi_filter/doi_filter_input";
import { DOIFilterPartialResult } from "./doi_filter/doi_filter_partial_result";
import { DOIFilterResult } from "./doi_filter/doi_filter_result";
import { DOIFilterViewSetting, DOIFilterWithViewSetting } from "./doi_filter/doi_filter_view_setting";

import { SummaryCache } from "./doi_filter/summary_cache";
import { SummaryInfo } from "./doi_filter/summary_cache";


export class BrowserInfo {
    public doiInfoCollection: DOIInfoCollection | null = null;
    //public pageNumber : number = -1;
    //public pageSize : number = 100;

    public currentDOIFilterWithViewSetting: DOIFilterWithViewSetting = new DOIFilterWithViewSetting();
    public doiFilterInputNumber: number = 0;
    public doiFilterInputHashStack = new Array<string>();

    public doiFilterWithViewSettingMap = new Map<string, DOIFilterWithViewSetting>();
    public doiFilterPartialResultMap = new Map<string, DOIFilterPartialResult>();
    public doiFilterResultMap = new Map<string, DOIFilterResult>();

    public summaryCache: SummaryCache = new SummaryCache();

    public initialize(doiFilterWithViewSetting: DOIFilterWithViewSetting, doiInfoCollection: DOIInfoCollection): void {
        this.currentDOIFilterWithViewSetting = doiFilterWithViewSetting;
        this.doiInfoCollection = doiInfoCollection;
        this.doiFilterInputNumber = 0;
        this.doiFilterInputHashStack = [];
        this.doiFilterInputHashStack.push(this.currentDOIFilterWithViewSetting.getHash());

        this.doiFilterWithViewSettingMap = new Map<string, DOIFilterWithViewSetting>();
        const emptyDOIFilterWithViewSetting = new DOIFilterWithViewSetting();
        this.doiFilterWithViewSettingMap.set(this.currentDOIFilterWithViewSetting.getHash(), this.currentDOIFilterWithViewSetting);
        this.doiFilterWithViewSettingMap.set(emptyDOIFilterWithViewSetting.getHash(), emptyDOIFilterWithViewSetting);

        this.doiFilterResultMap = new Map<string, DOIFilterResult>();
        const newDOIFilterResult = new DOIFilterResult(null, this.doiInfoCollection!);
        this.doiFilterResultMap.set(emptyDOIFilterWithViewSetting.doiFilterInput.getHash(), newDOIFilterResult);
        if(emptyDOIFilterWithViewSetting.getHash() != this.currentDOIFilterWithViewSetting.getHash()){
            const newDOIFilterResult2 = newDOIFilterResult.search(this.currentDOIFilterWithViewSetting.doiFilterInput, this.doiInfoCollection!);
            this.doiFilterResultMap.set(this.currentDOIFilterWithViewSetting.doiFilterInput.getHash(), newDOIFilterResult2);
        }

        this.doiFilterPartialResultMap = new Map<string, DOIFilterPartialResult>();
        const currentDOIFilterResult = this.doiFilterResultMap.get(this.currentDOIFilterWithViewSetting.doiFilterInput.getHash())!;
        const currentDOIFilterPartialResult = new DOIFilterPartialResult(currentDOIFilterResult.doiIDs, this.currentDOIFilterWithViewSetting, this.doiInfoCollection!);
        this.doiFilterPartialResultMap.set(this.currentDOIFilterWithViewSetting.getHash(), currentDOIFilterPartialResult);

        if(!this.summaryCache.hasSummaryInfo(this.currentDOIFilterWithViewSetting.doiFilterInput)){
            this.summaryCache.createSummaryInfo(currentDOIFilterResult, this.currentDOIFilterWithViewSetting.doiFilterInput, this.doiInfoCollection!);
        }
        
    }


    public getCurrentDOIFilterPartialResult(): DOIFilterPartialResult {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            return new DOIFilterPartialResult([], null, this.doiInfoCollection!);
        } else {
            const result = this.doiFilterPartialResultMap.get(this.doiFilterInputHashStack[this.doiFilterInputNumber])!;
            if(result == null){
                throw new Error("No current DOI filter partial result");
            }
            return result;
        }
    }
    public getCurrentDOIFilterWithViewSetting(): DOIFilterWithViewSetting {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            return new DOIFilterWithViewSetting();
        } else {
            const result = this.doiFilterWithViewSettingMap.get(this.doiFilterInputHashStack[this.doiFilterInputNumber])!;
            if(result == null){
                throw new Error("No current DOI filter input");
            }
            return result;
        }
    }
    public getCurrentDOIFilterResult(): DOIFilterResult {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            throw new Error("No current DOI filter result");
        } else {
            const rp = this.getCurrentDOIFilterWithViewSetting();
            const result = this.doiFilterResultMap.get(rp.doiFilterInput.getHash())!;
            if(result == null){
                throw new Error("No current DOI filter result");
            }
            return result;
        }
    }
    public getCurrentSummaryInfo(): SummaryInfo {
        if(!this.summaryCache.hasSummaryInfo(this.currentDOIFilterWithViewSetting.doiFilterInput)){
            throw new Error("No current summary info");
        }
        return this.summaryCache.getSummaryInfo(this.currentDOIFilterWithViewSetting.doiFilterInput);
    }

    public processCurrentDOIFilterInput(): void {
        if (this.doiInfoCollection != null) {
            const hash = this.currentDOIFilterWithViewSetting.getHash();
            const hashWithoutDetailedParamters = this.currentDOIFilterWithViewSetting.doiFilterInput.getHash();

            while (this.doiFilterInputHashStack.length > this.doiFilterInputNumber && this.doiFilterInputHashStack.length > 0) {
                this.doiFilterInputHashStack.shift();
            }

            this.doiFilterInputHashStack.push(hash);
            this.doiFilterInputNumber = this.doiFilterInputHashStack.length - 1;
            this.doiFilterWithViewSettingMap.set(hash, this.currentDOIFilterWithViewSetting);



            if (!this.doiFilterResultMap.has(hashWithoutDetailedParamters)) {
                let parentDOIFilterWithViewSetting = new DOIFilterWithViewSetting();

                if(!this.doiFilterResultMap.has(parentDOIFilterWithViewSetting.doiFilterInput.getHash())){
                    const newDOIFilterResult = new DOIFilterResult(null, this.doiInfoCollection!);
                    this.doiFilterResultMap.set(parentDOIFilterWithViewSetting.doiFilterInput.getHash(), newDOIFilterResult);
                }

                if (this.doiFilterInputNumber > 0) {
                    const previousDOIFilterWithViewSetting = this.doiFilterWithViewSettingMap.get(this.doiFilterInputHashStack[this.doiFilterInputNumber - 1])!;
                    if (this.currentDOIFilterWithViewSetting.doiFilterInput.isIncluded(previousDOIFilterWithViewSetting.doiFilterInput)) {
                        parentDOIFilterWithViewSetting = previousDOIFilterWithViewSetting;
                    }
                }



                const parentDOIFilterResult = this.doiFilterResultMap.get(parentDOIFilterWithViewSetting.doiFilterInput.getHash())!;
                const newDOIFilterResult = parentDOIFilterResult.search(this.currentDOIFilterWithViewSetting.doiFilterInput, this.doiInfoCollection!);
                this.doiFilterResultMap.set(hashWithoutDetailedParamters, newDOIFilterResult);
                if(!this.summaryCache.hasSummaryInfo(this.currentDOIFilterWithViewSetting.doiFilterInput)){
                    this.summaryCache.createSummaryInfo(newDOIFilterResult, this.currentDOIFilterWithViewSetting.doiFilterInput, this.doiInfoCollection!);
                }
            }


            const filterResult = this.doiFilterResultMap.get(hashWithoutDetailedParamters)!;
            const partialResult = new DOIFilterPartialResult(filterResult.doiIDs, this.currentDOIFilterWithViewSetting, this.doiInfoCollection!);
            this.doiFilterPartialResultMap.set(hash, partialResult);
        }

        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length){
            throw new Error("Logic error");
        }
    }

    public debug(): void {
        console.log(`LOG1: ${this.doiFilterInputNumber}`);
        console.log(`LOG2: ${this.doiFilterInputHashStack.length}`);
        for(let i = 0; i < this.doiFilterInputHashStack.length; i++){
            console.log(`LOG: ${this.doiFilterInputHashStack[i]}`);
        }
        console.log(`LOG3: ${this.doiFilterResultMap.size}`);
        this.doiFilterResultMap.forEach((value, key) => {
            console.log(`LOG: ${key}`);
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