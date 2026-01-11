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

function renderDOICategorySelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, doiInfoSearchInput: DOIFilterInput){
  const type_list = currentDOIInfoCollectionFilter.getTypes();
  type_list.sort();
  const typeDoiCountList = type_list.map(type => currentDOIInfoCollectionFilter.searchByType(type, doiNumberFilterSet, doiInfoCollection).length);
  const typeSelect = document.getElementById("type-select");
  if (typeSelect && typeSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(typeSelect, type_list, typeDoiCountList, doiInfoSearchInput.type, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}
function renderContainerTitleSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, doiInfoSearchInput: DOIFilterInput){
  const containerTitle_list = currentDOIInfoCollectionFilter.getContainerTitles();
  const containerTitleDoiCountList = containerTitle_list.map(containerTitle => currentDOIInfoCollectionFilter.searchByContainerTitle(containerTitle, doiNumberFilterSet, doiInfoCollection).length);
  const containerTitleSelect = document.getElementById("container-title-select");
  if (containerTitleSelect && containerTitleSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(containerTitleSelect, containerTitle_list, containerTitleDoiCountList, doiInfoSearchInput.container_title, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

function renderMinimumYearSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, doiInfoSearchInput: DOIFilterInput){
  const yearFromSelect = document.getElementById("year-from-select");
  const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
  const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();

  const yearList = Array.from({length: maxYear - minYear + 1}, (_, index) => minYear + index).map(year => year.toString());
  const currentMaxYear = doiInfoSearchInput.maximum_year == null ? maxYear : doiInfoSearchInput.maximum_year;

  const yearDoiCountListForFromYear = yearList.map(year => currentDOIInfoCollectionFilter.searchByYear(parseInt(year), currentMaxYear, doiNumberFilterSet, doiInfoCollection).length);
  const minimumYear = doiInfoSearchInput.minimum_year == null ? null : doiInfoSearchInput.minimum_year.toString();
  if (yearFromSelect && yearFromSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearFromSelect, yearList, yearDoiCountListForFromYear, minimumYear as string | null, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
  }
}

function renderMaximumYearSelectBox(currentDOIInfoCollectionFilter: DOIFilterResult, doiNumberFilterSet: Set<number>, doiInfoCollection: DOIInfoCollection, doiInfoSearchInput: DOIFilterInput){
  const yearToSelect = document.getElementById("year-to-select");
  const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
  const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();
  const currentMinimumYear = doiInfoSearchInput.minimum_year == null ? minYear : doiInfoSearchInput.minimum_year;

  const yearList = Array.from({length: maxYear - minYear + 1}, (_, index) => minYear + index).map(year => year.toString());
  const maximumYear = doiInfoSearchInput.maximum_year == null ? null : doiInfoSearchInput.maximum_year.toString();
  const yearDoiCountListForToYear = yearList.map(year => currentDOIInfoCollectionFilter.searchByYear(currentMinimumYear, parseInt(year), doiNumberFilterSet, doiInfoCollection).length);

  if (yearToSelect && yearToSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearToSelect, yearList, yearDoiCountListForToYear, maximumYear as string | null, "Any", currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection);
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

    renderDOICategorySelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput);
    renderContainerTitleSelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput);
    renderMinimumYearSelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput);
    renderMaximumYearSelectBox(currentDOIInfoCollectionFilter, doiNumberFilterSet, doiInfoCollection, browserInfo.doiInfoSearchInput);

  }

}