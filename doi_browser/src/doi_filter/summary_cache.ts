import { DOIFilterResult } from "./doi_filter_result";
import { DOIFilterInput } from "./doi_filter_input";
import { DOIInfoCollection } from "../doi_info";

export class SummaryInfo {
    public doiCategoryList: string[] = [];
    public doiCategoryCountList: number[] = [];
    public containerTitleList: string[] = [];
    public containerTitleCountList: number[] = [];
    public yearFromList: string[] = [];
    public yearFromCountList: number[] = [];
    public yearToList: string[] = [];
    public yearToCountList: number[] = [];

    public build(filterResult: DOIFilterResult, filterInput: DOIFilterInput, doiInfoCollection: DOIInfoCollection){
        this.doiCategoryList = filterResult.getTypes();
        this.doiCategoryList.sort();

        const doiNumberFilterSet = new Set<number>(filterResult.doiIDs);
        this.doiCategoryCountList = this.doiCategoryList.map(type => filterResult.searchByType(type, doiNumberFilterSet, doiInfoCollection).length);

        this.containerTitleList = filterResult.getContainerTitles();
        this.containerTitleCountList = this.containerTitleList.map(containerTitle => filterResult.searchByContainerTitle(containerTitle, doiNumberFilterSet, doiInfoCollection).length);

        {
            let yearFromList: string[] = [];
            const minYear = filterResult.getMinimumYear();
            const maxYear = filterResult.getMaxmumYear();
            yearFromList = Array.from({ length: maxYear - minYear + 1 }, (_, index) => minYear + index).map(year => year.toString());

            let yearFromDoiCountList: number[] = [];
            const currentMaxYear = filterInput.maximum_year == null ? maxYear : filterInput.maximum_year;
            yearFromDoiCountList = yearFromList.map(year => filterResult.searchByYear(parseInt(year), currentMaxYear, doiNumberFilterSet, doiInfoCollection).length);

            for (let i = 0; i < yearFromList.length; i++) {
                if (i == yearFromList.length - 1 || (yearFromDoiCountList[i] - yearFromDoiCountList[i + 1] > 0)) {
                    this.yearFromList.push(yearFromList[i]);
                    this.yearFromCountList.push(yearFromDoiCountList[i]);
                }
            }
        }

        {

            let yearToList: string[] = [];
            const minYear = filterResult.getMinimumYear();
            const maxYear = filterResult.getMaxmumYear();
            yearToList = Array.from({ length: maxYear - minYear + 1 }, (_, index) => minYear + index).map(year => year.toString());

            let yearToDoiCountList: number[] = [];
            const currentMinimumYear = filterInput.minimum_year == null ? minYear : filterInput.minimum_year;
            yearToDoiCountList = yearToList.map(year => filterResult.searchByYear(currentMinimumYear, parseInt(year), doiNumberFilterSet, doiInfoCollection).length);

            for (let i = 0; i < yearToList.length; i++) {
                if (i == yearToList.length - 1 || (yearToDoiCountList[i] - yearToDoiCountList[i + 1] > 0)) {
                    this.yearToList.push(yearToList[i]);
                    this.yearToCountList.push(yearToDoiCountList[i]);
                }
            }
        }
    }
}


export class SummaryCache {
    public summaryInfoMap = new Map<string, SummaryInfo>();

    public hasSummaryInfo(filterInput: DOIFilterInput): boolean {
        return this.summaryInfoMap.has(filterInput.getHash());
    }
    public getSummaryInfo(filterInput: DOIFilterInput): SummaryInfo {
        return this.summaryInfoMap.get(filterInput.getHash())!;
    }

    public createSummaryInfo(filterResult: DOIFilterResult, filterInput: DOIFilterInput, doiInfoCollection: DOIInfoCollection) {
        const cacheKey = filterInput.getHash();
        if (!this.summaryInfoMap.has(cacheKey)) {
            let summaryInfo = new SummaryInfo();
            summaryInfo.build(filterResult, filterInput, doiInfoCollection);
            this.summaryInfoMap.set(cacheKey, summaryInfo);
        }
    }

}
