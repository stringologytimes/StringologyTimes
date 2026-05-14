import { DOIFilterResult } from "./doi_filter_result";
import { DOIFilterQuery } from "./doi_filter_query";
import { DOIInfoCollection } from "../doi_info";

export class SummaryInfo {
    public doiCount: number = 0;
    public doiCategoryList: string[] = [];
    public doiCategoryCountList: number[] = [];
    public containerTitleList: string[] = [];
    public containerTitleCountList: number[] = [];
    public yearFromList: string[] = [];
    public yearFromCountList: number[] = [];
    public yearToList: string[] = [];
    public yearToCountList: number[] = [];
    public tagList: string[] = [];
    public tagCountList: number[] = [];

    public build(filterResult: DOIFilterResult, filterInput: DOIFilterQuery, doiInfoCollection: DOIInfoCollection){
        this.doiCount = filterResult.doiIDs.length;
        this.doiCategoryList = filterResult.getTypes();
        this.doiCategoryList.sort();

        const doiNumberFilterSet = new Set<number>(filterResult.doiIDs);
        this.doiCategoryCountList = this.doiCategoryList.map(type => filterResult.searchByType(type, doiNumberFilterSet, doiInfoCollection).length);

        this.containerTitleList = filterResult.getContainerTitles();
        if(filterInput.sortBy == "alphabetical-order-by-container-title"){
            this.containerTitleList.sort();
        }else if(filterInput.sortBy == "article-count"){
            const containerTitleToDoiCountMapper = new Map<string, number>();
            this.containerTitleList.forEach(containerTitle => {
                containerTitleToDoiCountMapper.set(containerTitle, filterResult.searchByContainerTitle(containerTitle, doiNumberFilterSet, doiInfoCollection).length);
            });
            this.containerTitleList = this.containerTitleList.sort((a, b) => containerTitleToDoiCountMapper.get(a)! - containerTitleToDoiCountMapper.get(b)!);
        }


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
                if (i == yearToList.length - 1 || (yearToDoiCountList[i + 1] - yearToDoiCountList[i] > 0)) {
                    this.yearToList.push(yearToList[i]);
                    this.yearToCountList.push(yearToDoiCountList[i]);
                }
            }

        }

        {
            filterResult.getTags().forEach(tag => {
                this.tagList.push(tag);
            });
            this.tagList.sort();
            this.tagCountList = this.tagList.map(tag => filterResult.searchByTag(tag, doiNumberFilterSet, doiInfoCollection).length);
        }
    }
}



