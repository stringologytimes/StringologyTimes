import { BrowserInfo } from "../browser_info";
import { DOIFilterResult } from "../doi_filter/doi_filter_result";
import { DOIInfoCollection } from "../doi_info";
import { DOIFilterInput } from "../doi_filter/doi_filter_input";
import { SummaryInfo } from "../doi_filter/summary_cache";
/*
function getUniqueStringSet(items: string[]): string[] {
  const uniqueSet = new Set<string>();
  items.forEach(item => {
    uniqueSet.add(item);
  });
  return Array.from(uniqueSet);
}
*/





function setSelectHTMLElement(selectElement: HTMLSelectElement, options: string[], doiCountList: number[], selectedValue: string | null, dontCareValueName: string) {
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

function renderDOICategorySelectBox(summaryInfo: SummaryInfo, selectedValue: string | null) {
  
  const typeSelect = document.getElementById("type-select");
  if (typeSelect && typeSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(typeSelect, summaryInfo.doiCategoryList, summaryInfo.doiCategoryCountList, selectedValue, "Any");
  }
}
function renderContainerTitleSelectBox(summaryInfo: SummaryInfo, selectedValue: string | null) {
  const containerTitleSelect = document.getElementById("container-title-select");
  if (containerTitleSelect && containerTitleSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(containerTitleSelect, summaryInfo.containerTitleList, summaryInfo.containerTitleCountList, selectedValue, "Any");
  }
}

function renderMinimumYearSelectBox(summaryInfo: SummaryInfo, selectedMinimumYear: number | null, selectedMaximumYear: number | null) {
  const yearFromSelect = document.getElementById("year-from-select");
  const selectedMinimumYearStr = selectedMinimumYear == null ? null : selectedMinimumYear.toString();
  if (yearFromSelect && yearFromSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearFromSelect, summaryInfo.yearFromList, summaryInfo.yearFromCountList, selectedMinimumYearStr, "Any");
  }
}

function renderMaximumYearSelectBox(summaryInfo: SummaryInfo, selectedMinimumYear: number | null, selectedMaximumYear: number | null) {
  const yearToSelect = document.getElementById("year-to-select");
  const selectedMaximumYearStr = selectedMaximumYear == null ? null : selectedMaximumYear.toString();

  if (yearToSelect && yearToSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(yearToSelect, summaryInfo.yearToList, summaryInfo.yearToCountList, selectedMaximumYearStr, "Any");
  }
}

export function renderFilterBox(filterResult: DOIFilterResult, filterInput: DOIFilterInput, doiInfoCollection: DOIInfoCollection, summaryInfo: SummaryInfo) {
  renderDOICategorySelectBox(summaryInfo, filterInput.type);
  renderContainerTitleSelectBox(summaryInfo, filterInput.container_title);
  renderMinimumYearSelectBox(summaryInfo, filterInput.minimum_year, filterInput.maximum_year);
  renderMaximumYearSelectBox(summaryInfo, filterInput.minimum_year, filterInput.maximum_year);

}