import { DOIInfoCollection } from "./doi_info";
import { DOIInfo } from "./doi_info";
import { DOIFilterInput } from "./doi_filter/doi_filter_input";
import { DOIFilterPartialResult } from "./doi_filter/doi_filter_partial_result";
import { DOIFilterResult } from "./doi_filter/doi_filter_result";


export class BrowserInfo {
    public doiInfoCollection: DOIInfoCollection | null = null;
    //public pageNumber : number = -1;
    //public pageSize : number = 100;

    public currentDOIFilterInput: DOIFilterInput = new DOIFilterInput();
    public doiFilterInputNumber: number = 0;
    public doiFilterInputHashStack = new Array<string>();

    public doiFilterInputMap = new Map<string, DOIFilterInput>();
    public doiFilterPartialResultMap = new Map<string, DOIFilterPartialResult>();
    public doiFilterResultMap = new Map<string, DOIFilterResult>();

    public initialize(doiFilterInput: DOIFilterInput, doiInfoCollection: DOIInfoCollection): void {
        this.currentDOIFilterInput = doiFilterInput;
        this.doiInfoCollection = doiInfoCollection;
        this.doiFilterInputNumber = 0;
        this.doiFilterInputHashStack = [];
        this.doiFilterInputHashStack.push(this.currentDOIFilterInput.getHash());

        this.doiFilterInputMap = new Map<string, DOIFilterInput>();
        const emptyDOIFilterInput = new DOIFilterInput();
        this.doiFilterInputMap.set(this.currentDOIFilterInput.getHash(), this.currentDOIFilterInput);
        this.doiFilterInputMap.set(emptyDOIFilterInput.getHash(), emptyDOIFilterInput);

        this.doiFilterResultMap = new Map<string, DOIFilterResult>();
        const newDOIFilterResult = new DOIFilterResult(null, this.doiInfoCollection!);
        this.doiFilterResultMap.set(emptyDOIFilterInput.getHashWithoutDetailedParamters(), newDOIFilterResult);
        if(emptyDOIFilterInput.getHash() != this.currentDOIFilterInput.getHash()){
            const newDOIFilterResult2 = newDOIFilterResult.search(this.currentDOIFilterInput, this.doiInfoCollection!);
            this.doiFilterResultMap.set(this.currentDOIFilterInput.getHashWithoutDetailedParamters(), newDOIFilterResult2);
        }

        this.doiFilterPartialResultMap = new Map<string, DOIFilterPartialResult>();
        const currentDOIFilterResult = this.doiFilterResultMap.get(this.currentDOIFilterInput.getHashWithoutDetailedParamters())!;
        const currentDOIFilterPartialResult = new DOIFilterPartialResult(currentDOIFilterResult.doiIDs, this.currentDOIFilterInput, this.doiInfoCollection!);
        this.doiFilterPartialResultMap.set(this.currentDOIFilterInput.getHash(), currentDOIFilterPartialResult);

        
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
    public getCurrentDOIFilterInput(): DOIFilterInput {
        if (this.doiFilterInputNumber >= this.doiFilterInputHashStack.length) {
            return new DOIFilterInput();
        } else {
            const result = this.doiFilterInputMap.get(this.doiFilterInputHashStack[this.doiFilterInputNumber])!;
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
            const rp = this.getCurrentDOIFilterInput();
            const result = this.doiFilterResultMap.get(rp.getHashWithoutDetailedParamters())!;
            if(result == null){
                throw new Error("No current DOI filter result");
            }
            return result;
        }
    }
    public processCurrentDOIFilterInput(): void {
        if (this.doiInfoCollection != null) {
            const hash = this.currentDOIFilterInput.getHash();
            const hashWithoutDetailedParamters = this.currentDOIFilterInput.getHashWithoutDetailedParamters();

            while (this.doiFilterInputHashStack.length > this.doiFilterInputNumber && this.doiFilterInputHashStack.length > 0) {
                this.doiFilterInputHashStack.shift();
            }

            this.doiFilterInputHashStack.push(hash);
            this.doiFilterInputNumber = this.doiFilterInputHashStack.length - 1;
            this.doiFilterInputMap.set(hash, this.currentDOIFilterInput);



            if (!this.doiFilterResultMap.has(hashWithoutDetailedParamters)) {
                let parentDOIFilterInput = new DOIFilterInput();

                if(!this.doiFilterResultMap.has(parentDOIFilterInput.getHashWithoutDetailedParamters())){
                    const newDOIFilterResult = new DOIFilterResult(null, this.doiInfoCollection!);
                    this.doiFilterResultMap.set(parentDOIFilterInput.getHashWithoutDetailedParamters(), newDOIFilterResult);
                }

                if (this.doiFilterInputNumber > 0) {
                    const previousDOIFilterInput = this.doiFilterInputMap.get(this.doiFilterInputHashStack[this.doiFilterInputNumber - 1])!;
                    if (this.currentDOIFilterInput.isIncluded(previousDOIFilterInput)) {
                        parentDOIFilterInput = previousDOIFilterInput;
                    }
                }



                const parentDOIFilterResult = this.doiFilterResultMap.get(parentDOIFilterInput.getHashWithoutDetailedParamters())!;
                const newDOIFilterResult = parentDOIFilterResult.search(this.currentDOIFilterInput, this.doiInfoCollection!);
                this.doiFilterResultMap.set(hashWithoutDetailedParamters, newDOIFilterResult);
            }


            const filterResult = this.doiFilterResultMap.get(hashWithoutDetailedParamters)!;
            const partialResult = new DOIFilterPartialResult(filterResult.doiIDs, this.currentDOIFilterInput, this.doiInfoCollection!);
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