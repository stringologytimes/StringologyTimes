import { BrowserInfo } from "../browser_info";
import { DOIFilterResult } from "../doi_filter/doi_filter_result";
import { DOIInfoCollection } from "../doi_info";
import { DOIFilterInput } from "../doi_filter/doi_filter_input";
/*
function getUniqueStringSet(items: string[]): string[] {
  const uniqueSet = new Set<string>();
  items.forEach(item => {
    uniqueSet.add(item);
  });
  return Array.from(uniqueSet);
}
*/

const doiCategoryCache = new Map<string, string[]>();
const doiCategoryCountCache = new Map<string, number[]>();

const containerTitleCache = new Map<string, string[]>();
const containerTitleCountCache = new Map<string, number[]>();

const yearFromCache = new Map<string, string[]>();
const yearFromCountCache = new Map<string, number[]>();

const yearToCache = new Map<string, string[]>();
const yearToCountCache = new Map<string, number[]>();




function setSelectHTMLElement(selectElement: HTMLSelectElement, options: string[], doiCountList: number[], selectedValue: string | null, dontCareValueName: string, currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection) {
  selectElement.innerHTML = "";
  const defaultOption = document.createElement("option");
  defaultOption.value = "dont-care";
  defaultOption.textContent = dontCareValueName;
  selectElement.appendChild(defaultOption);


  options.forEach((optionValue, index) => {
    const option = document.createElement("option");
    const doiCount = doiCountList[index];
    option.value = optionValue;
    option.textContent = `${optionValue} (${doiCount})`;

    if (optionValue == selectedValue) {
      option.selected = true;
    }
    selectElement.appendChild(option);
  });
}

function renderDOICategorySelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, cacheKey: string, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedValue: string | null) {
  let type_list: string[] = [];
  if(doiCategoryCache.has(cacheKey)){
    type_list = doiCategoryCache.get(cacheKey)!;
  }else{
    type_list = currentDOIInfoCollectionFilter.getTypes();
    type_list.sort();
    doiCategoryCache.set(cacheKey, type_list);
  }

  let doiCategoryCountList: number[] = [];
  if(doiCategoryCountCache.has(cacheKey)){
    doiCategoryCountList = doiCategoryCountCache.get(cacheKey)!;
  }else{
    doiCategoryCountList = type_list.map(type => currentDOIInfoCollectionFilter.searchByType(type, doiNumberFilterSet, doiInfoCollection).length);
    doiCategoryCountCache.set(cacheKey, doiCategoryCountList);
  }
  
  const typeSelect = document.getElementById("type-select");
  if (typeSelect && typeSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(typeSelect, type_list, doiCategoryCountList, selectedValue, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}
function renderContainerTitleSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, cacheKey: string, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedValue: string | null) {

  let containerTitle_list: string[] = [];
  if(containerTitleCache.has(cacheKey)){
    containerTitle_list = containerTitleCache.get(cacheKey)!;
  }else{
    containerTitle_list = currentDOIInfoCollectionFilter.getContainerTitles();
    containerTitleCache.set(cacheKey, containerTitle_list);
  }

  let containerTitleDoiCountList: number[] = [];
  if(containerTitleCountCache.has(cacheKey)){
    containerTitleDoiCountList = containerTitleCountCache.get(cacheKey)!;
  }else{
    containerTitleDoiCountList = containerTitle_list.map(containerTitle => currentDOIInfoCollectionFilter.searchByContainerTitle(containerTitle, doiNumberFilterSet, doiInfoCollection).length);
    containerTitleCountCache.set(cacheKey, containerTitleDoiCountList);
  }

  const containerTitleSelect = document.getElementById("container-title-select");
  if (containerTitleSelect && containerTitleSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(containerTitleSelect, containerTitle_list, containerTitleDoiCountList, selectedValue, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

function renderMinimumYearSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, cacheKey: string, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedMinimumYear: number | null, selectedMaximumYear: number | null) {
  const yearFromSelect = document.getElementById("year-from-select");

  let yearFromList: string[] = [];
  if(yearFromCache.has(cacheKey)){
    yearFromList = yearFromCache.get(cacheKey)!;
  }else{
    const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
    const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();
  
    yearFromList = Array.from({ length: maxYear - minYear + 1 }, (_, index) => minYear + index).map(year => year.toString());  
    yearFromCache.set(cacheKey, yearFromList);
  }
  
  let yearFromDoiCountList: number[] = [];
  if(yearFromCountCache.has(cacheKey)){
    yearFromDoiCountList = yearFromCountCache.get(cacheKey)!;
  }else{
    const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
    const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();

    const currentMaxYear = selectedMaximumYear == null ? maxYear : selectedMaximumYear;
    const yearFromDoiCountList = yearFromList.map(year => currentDOIInfoCollectionFilter.searchByYear(parseInt(year), currentMaxYear, doiNumberFilterSet, doiInfoCollection).length);
  
    yearFromCountCache.set(cacheKey, yearFromDoiCountList);
  }


  const filteredYearList = [];
  const filteredYearDoiCountList = [];
  for (let i = 0; i < yearFromList.length; i++) {
    if (i == yearFromList.length - 1 || (yearFromDoiCountList[i] - yearFromDoiCountList[i + 1] > 0)) {
      filteredYearList.push(yearFromList[i]);
      filteredYearDoiCountList.push(yearFromDoiCountList[i]);
    }
  }

  const selectedMinimumYearStr = selectedMinimumYear == null ? null : selectedMinimumYear.toString();
  if (yearFromSelect && yearFromSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearFromSelect, filteredYearList, filteredYearDoiCountList, selectedMinimumYearStr, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

function renderMaximumYearSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, cacheKey: string, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedMinimumYear: number | null, selectedMaximumYear: number | null) {
  const yearToSelect = document.getElementById("year-to-select");

  let yearToList: string[] = [];
  if(yearToCache.has(cacheKey)){
    yearToList = yearToCache.get(cacheKey)!;
  }else{
    const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
    const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();
  
    yearToList = Array.from({ length: maxYear - minYear + 1 }, (_, index) => minYear + index).map(year => year.toString());
    yearToCache.set(cacheKey, yearToList);
  }

  let yearToDoiCountList: number[] = [];
  if(yearToCountCache.has(cacheKey)){
    yearToDoiCountList = yearToCountCache.get(cacheKey)!;
  }else{
    const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
    const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();
    const currentMinimumYear = selectedMinimumYear == null ? minYear : selectedMinimumYear;
    yearToDoiCountList = yearToList.map(year => currentDOIInfoCollectionFilter.searchByYear(currentMinimumYear, parseInt(year), doiNumberFilterSet, doiInfoCollection).length);
    yearToCountCache.set(cacheKey, yearToDoiCountList);
  }

  const filteredYearList = [];
  const filteredYearDoiCountList = [];
  for (let i = 0; i < yearToList.length; i++) {
    if (i == 0 || (yearToDoiCountList[i] - yearToDoiCountList[i - 1] > 0)) {
      filteredYearList.push(yearToList[i]);
      filteredYearDoiCountList.push(yearToDoiCountList[i]);
    }
  }
  const selectedMaximumYearStr = selectedMaximumYear == null ? null : selectedMaximumYear.toString();

  if (yearToSelect && yearToSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearToSelect, filteredYearList, filteredYearDoiCountList, selectedMaximumYearStr, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

export function renderFilterBox(filterResult: DOIFilterResult, filterInput: DOIFilterInput, doiInfoCollection: DOIInfoCollection) {
  //const doiIDs = Array.from({length: doiInfoCollection.length()}, (_, index) => index);
  //const foundDOIIDs : number[] = browserInfo.doiInfoSearchInput.search(new DOIFilterResult(doiIDs, doiInfoCollection), doiInfoCollection);
  //const currentDOIInfoCollectionFilter = ;
  const doiNumberFilterSet = new Set<number>(filterResult.doiIDs);
  const cacheKey = filterInput.getHashWithoutDetailedParamters();

  renderDOICategorySelectBox(filterResult, cacheKey, doiNumberFilterSet, doiInfoCollection, filterInput.type);
  renderContainerTitleSelectBox(filterResult, cacheKey, doiNumberFilterSet, doiInfoCollection, filterInput.container_title);
  renderMinimumYearSelectBox(filterResult, cacheKey, doiNumberFilterSet, doiInfoCollection, filterInput.minimum_year, filterInput.maximum_year);
  renderMaximumYearSelectBox(filterResult, cacheKey, doiNumberFilterSet, doiInfoCollection, filterInput.minimum_year, filterInput.maximum_year);

}