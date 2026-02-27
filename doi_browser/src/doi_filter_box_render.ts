import { BrowserInfo } from "./browser_info";
import { DOIFilterResult } from "./doi_filter_result";
import { DOIInfoCollection } from "./doi_info";
import { DOIFilterInput } from "./doi_filter_input";
/*
function getUniqueStringSet(items: string[]): string[] {
  const uniqueSet = new Set<string>();
  items.forEach(item => {
    uniqueSet.add(item);
  });
  return Array.from(uniqueSet);
}
*/

function setSelectHTMLElement(selectElement: HTMLSelectElement, options: string[], doiCountList: number[], selectedValue: string | null, dontCareValueName: string, currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection){
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

    if(optionValue == selectedValue) {
      option.selected = true;
    }
    selectElement.appendChild(option);
  });
}

function renderDOICategorySelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedValue: string | null){
  const type_list = currentDOIInfoCollectionFilter.getTypes();
  type_list.sort();
  const typeDoiCountList = type_list.map(type => currentDOIInfoCollectionFilter.searchByType(type, doiNumberFilterSet, doiInfoCollection).length);
  const typeSelect = document.getElementById("type-select");
  if (typeSelect && typeSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(typeSelect, type_list, typeDoiCountList, selectedValue, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}
function renderContainerTitleSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedValue: string | null){
  const containerTitle_list = currentDOIInfoCollectionFilter.getContainerTitles();
  const containerTitleDoiCountList = containerTitle_list.map(containerTitle => currentDOIInfoCollectionFilter.searchByContainerTitle(containerTitle, doiNumberFilterSet, doiInfoCollection).length);
  const containerTitleSelect = document.getElementById("container-title-select");
  if (containerTitleSelect && containerTitleSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(containerTitleSelect, containerTitle_list, containerTitleDoiCountList, selectedValue, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

function renderMinimumYearSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedMinimumYear: number | null, selectedMaximumYear: number | null){
  const yearFromSelect = document.getElementById("year-from-select");
  const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
  const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();

  const yearList = Array.from({length: maxYear - minYear + 1}, (_, index) => minYear + index).map(year => year.toString());
  const currentMaxYear = selectedMaximumYear == null ? maxYear : selectedMaximumYear;
  const yearDoiCountListForFromYear = yearList.map(year => currentDOIInfoCollectionFilter.searchByYear(parseInt(year), currentMaxYear, doiNumberFilterSet, doiInfoCollection).length);

  const filteredYearList = [];
  const filteredYearDoiCountList = [];
  for(let i = 0; i < yearList.length; i++){
    if(i == yearList.length-1 || (yearDoiCountListForFromYear[i] - yearDoiCountListForFromYear[i+1] > 0)){
      filteredYearList.push(yearList[i]);
      filteredYearDoiCountList.push(yearDoiCountListForFromYear[i]);
    }
  }

  const selectedMinimumYearStr = selectedMinimumYear == null ? null : selectedMinimumYear.toString();
  if (yearFromSelect && yearFromSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearFromSelect, filteredYearList, filteredYearDoiCountList, selectedMinimumYearStr, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

function renderMaximumYearSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, selectedMinimumYear: number | null, selectedMaximumYear: number | null){
  const yearToSelect = document.getElementById("year-to-select");
  const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
  const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();
  const currentMinimumYear = selectedMinimumYear == null ? minYear : selectedMinimumYear;

  const yearList = Array.from({length: maxYear - minYear + 1}, (_, index) => minYear + index).map(year => year.toString());
  const selectedMaximumYearStr = selectedMaximumYear == null ? null : selectedMaximumYear.toString();
  const yearDoiCountListForToYear = yearList.map(year => currentDOIInfoCollectionFilter.searchByYear(currentMinimumYear, parseInt(year), doiNumberFilterSet, doiInfoCollection).length);

  const filteredYearList = [];
  const filteredYearDoiCountList = [];
  for(let i = 0; i < yearList.length; i++){
    if(i == 0 || (yearDoiCountListForToYear[i] - yearDoiCountListForToYear[i-1] > 0)){
      filteredYearList.push(yearList[i]);
      filteredYearDoiCountList.push(yearDoiCountListForToYear[i]);
    }
  }

  if (yearToSelect && yearToSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearToSelect, filteredYearList, filteredYearDoiCountList, selectedMaximumYearStr, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

export function renderFilterBox(browserInfo: BrowserInfo) {
  const doiInfoCollection = browserInfo.doiInfoCollection;
  if (doiInfoCollection == null) {
    throw new Error("DOIInfoCollection is not loaded");
  }
  else {
    const doiIDs = Array.from({length: doiInfoCollection.length()}, (_, index) => index);
    const foundDOIIDs : number[] = browserInfo.doiInfoSearchInput.search(new DOIFilterResult(doiIDs, doiInfoCollection), doiInfoCollection);
    const currentDOIInfoCollectionFilter = new DOIFilterResult(foundDOIIDs, doiInfoCollection);
    const doiNumberFilterSet = new Set<number>(foundDOIIDs);

    renderDOICategorySelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput.type);
    renderContainerTitleSelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput.container_title);
    renderMinimumYearSelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput.minimum_year, browserInfo.doiInfoSearchInput.maximum_year);
    renderMaximumYearSelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput.minimum_year, browserInfo.doiInfoSearchInput.maximum_year);

  }

}