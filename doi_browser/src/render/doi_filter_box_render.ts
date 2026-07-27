import { BrowserInfo } from "../browser_info";
import { DOIFilterResult } from "../doi_filter/doi_filter_result";
import { DOIInfoCollection } from "../doi_info";
import { DOIFilterQuery } from "../doi_filter/doi_filter_query";
import { SummaryInfo } from "../doi_filter/summary_info";
import { SortByType } from "../doi_filter/doi_filter_query";
import { getDOIInfoTypeList } from "../doi_info";
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

  var max_children_count = 300;
  var max_index = Math.min(options.length, max_children_count);

  for(var index = 0; index < max_index; index++){
    const optionValue = options[index];
    const option = document.createElement("option");
    const doiCount = doiCountList[index];
    option.value = optionValue;
    option.textContent = `${optionValue} (${doiCount})`;

    if (optionValue == selectedValue) {
      option.selected = true;
    }

    selectElement.appendChild(option);
  }

  if(max_index < options.length){
    const option = document.createElement("option");
    option.value = "more";
    option.textContent = "More";
    selectElement.appendChild(option);
  }

  /*

  options.forEach((optionValue, index) => {
    const option = document.createElement("option");
    const doiCount = doiCountList[index];
    option.value = optionValue;
    option.textContent = `${optionValue} (${doiCount})`;

    if (optionValue == selectedValue) {
      option.selected = true;
    }

    if(index < max_children_count){
      selectElement.appendChild(option);
    }
  });
  */
}

export function setRadioBoxes(divID: string, templateName: string, selectedValue: string | null, itemNames: string[], itemValues: string[]) {
  const typeListDiv = document.getElementById(divID);
  if (typeListDiv && typeListDiv instanceof HTMLElement) {
    typeListDiv.innerHTML = "";

    const template = document.getElementById(templateName) as HTMLTemplateElement;

    itemNames.forEach((itemName, index) => {
      const itemValue = itemValues[index];
      const typeClone = template.content.cloneNode(true) as DocumentFragment;
      const typeLabel = typeClone.querySelector('label');
      if (typeLabel && typeLabel instanceof HTMLLabelElement) {
        typeLabel.textContent = itemName;
        if (itemValue == "dissabled") {
          typeLabel.style.color = "gray";
        }
      } else {
        throw new Error("typeLabel is not found");
      }

      const typeInput = typeClone.querySelector('input');
      if (typeInput && typeInput instanceof HTMLInputElement) {
        typeInput.value = itemValue;
        if (selectedValue == itemValue) {
          typeInput.checked = true;
        } else {
          typeInput.checked = false;
        }
        if (itemValue == "dissabled") {
          typeInput.disabled = true;
        }
      } else {
        throw new Error("typeInput is not found");
      }
      typeListDiv.appendChild(typeClone);
    });
  }
}


function renderDOICategoryBox(summaryInfo: SummaryInfo, selectedValue: string | null) {
  const typeList = ["Any"];
  const typeValues = ["Any"];
  getDOIInfoTypeList().forEach(type => {
    //typeList.push(type);
    var p = summaryInfo.doiCategoryList.indexOf(type);
    if (p != -1) {
      const count = summaryInfo.doiCategoryCountList[p];
      typeList.push(`${type} (${count})`);
      typeValues.push(type);
    } else {
      typeList.push(type);
      typeValues.push("dissabled");
    }

  });
  setRadioBoxes("type-list-div", "type-template", selectedValue == null ? "Any" : selectedValue, typeList, typeValues);

  /*
  const typeListDiv = document.getElementById("type-list-div");
  if (typeListDiv && typeListDiv instanceof HTMLDivElement) {
    typeListDiv.innerHTML = "";

    const typeTemplate = document.getElementById('type-template') as HTMLTemplateElement;



    typeList.forEach((type, index) => {
      const typeClone = typeTemplate.content.cloneNode(true) as DocumentFragment;
      const typeLabel = typeClone.querySelector('label');
      if (typeLabel && typeLabel instanceof HTMLLabelElement) {
        typeLabel.textContent = type;
      } else {
        throw new Error("typeLabel is not found");
      }

      const typeInput = typeClone.querySelector('input');
      if (typeInput && typeInput instanceof HTMLInputElement) {
        typeInput.value = type;
        if (selectedValue == null && type == "Any") {
          typeInput.checked = true;
        }
        else {
          typeInput.checked = selectedValue == type;
        }

        if (type != "Any") {
          var p = summaryInfo.doiCategoryList.indexOf(type);
          if (p != -1) {
            const count = summaryInfo.doiCategoryCountList[p];
            typeLabel.textContent = `${type} (${count})`;
          } else {
            typeInput.disabled = true;
            typeLabel.style.color = "gray";
          }
        }


      } else {
        throw new Error("typeInput is not found");
      }
      console.log("typeClone: " + type);
      typeListDiv.appendChild(typeClone);
    });
  }
  */
  /*
  const typeSelect = document.getElementById("type-select");
  if (typeSelect && typeSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(typeSelect, summaryInfo.doiCategoryList, summaryInfo.doiCategoryCountList, selectedValue, "Any");
  }
  */
}
function renderContainerTitleSelectBox(summaryInfo: SummaryInfo, selectedValue: string | null) {
  const containerTitleSelect = document.getElementById("container-title-select");
  if (containerTitleSelect && containerTitleSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(containerTitleSelect, summaryInfo.containerTitleList, summaryInfo.containerTitleCountList, selectedValue, "Any");
  }
}

function renderSeriesTitleSelectBox(summaryInfo: SummaryInfo, selectedValue: string | null) {
  const seriesTitleSelect = document.getElementById("series-title-select");
  if (seriesTitleSelect && seriesTitleSelect instanceof HTMLSelectElement) {
    setSelectHTMLElement(seriesTitleSelect, summaryInfo.seriesTitleList, summaryInfo.seriesTitleCountList, selectedValue, "Any");
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

function renderTag1SelectBox(summaryInfo: SummaryInfo, selectedValue: string | null) {
  const tag1Select = document.getElementById("tag1-select");
  if (tag1Select && tag1Select instanceof HTMLSelectElement) {
    setSelectHTMLElement(tag1Select, summaryInfo.tagList, summaryInfo.tagCountList, selectedValue, "Any");
  }
}

function renderStatusSelectBox(excludeStatus: string[]) {
  const checkboxPrimary = document.getElementById("checkbox_primary");
  const checkboxSecondary = document.getElementById("checkbox_secondary");
  if (checkboxPrimary && checkboxSecondary && checkboxPrimary instanceof HTMLInputElement && checkboxSecondary instanceof HTMLInputElement) {
    checkboxPrimary.checked = !excludeStatus.includes("primary");
    checkboxSecondary.checked = !excludeStatus.includes("secondary");
  }
}

function renderSortBySelectBox(selectedValue: SortByType) {
  const sortBySelect = document.getElementById("sort-by-select");
  if (sortBySelect && sortBySelect instanceof HTMLSelectElement) {
    sortBySelect.innerHTML = "";
    const options = ["alphabetical-order-by-container-title", "ascending-order-by-date", "descending-order-by-date", "article-count", "unordered"];
    const optionNames = ["Alphabetical Order by Container Title", "Ascending Order by Date", "Descending Order by Date", "Article Count", "Unordered"];


    options.forEach((optionValue, index) => {
      const option = document.createElement("option");
      option.value = optionValue;
      option.textContent = `${optionNames[index]}`;

      if (optionValue == selectedValue) {
        option.selected = true;
      }
      sortBySelect.appendChild(option);
    });

  }
}

function renderKeywordBox(keywords: string[]) {
  const keywordsInput = document.getElementById("keywords-input");
  if (keywordsInput && keywordsInput instanceof HTMLInputElement) {
    keywordsInput.value = keywords.length > 0 ? keywords[0] : "";
  }
}


export function renderFilterBox(filterResult: DOIFilterResult, filterInput: DOIFilterQuery, doiInfoCollection: DOIInfoCollection, summaryInfo: SummaryInfo) {
  console.log("renderFilterBox (size: " + filterResult.doiIDs.length + ")");

  const renderStartTime1 = performance.now();  
  renderDOICategoryBox(summaryInfo, filterInput.type);
  const renderStartTime2 = performance.now();  
  renderContainerTitleSelectBox(summaryInfo, filterInput.container_title);
  const renderStartTime3 = performance.now();  
  renderSeriesTitleSelectBox(summaryInfo, filterInput.series_title);
  const renderStartTime4 = performance.now();  
  renderMinimumYearSelectBox(summaryInfo, filterInput.minimum_year, filterInput.maximum_year);
  const renderStartTime5 = performance.now();  
  renderMaximumYearSelectBox(summaryInfo, filterInput.minimum_year, filterInput.maximum_year);
  const renderStartTime6 = performance.now();  
  renderSortBySelectBox(filterInput.sortBy);
  const renderStartTime7 = performance.now();  
  renderTag1SelectBox(summaryInfo, filterInput.tags[0]);
  const renderStartTime8 = performance.now();  
  renderStatusSelectBox(filterInput.excludeStatus);
  const renderStartTime9 = performance.now();  
  renderKeywordBox(filterInput.keywords);
  const renderStartTime10 = performance.now();  

  var time1 = renderStartTime2 - renderStartTime1;
  var time2 = renderStartTime3 - renderStartTime2;
  var time3 = renderStartTime4 - renderStartTime3;
  var time4 = renderStartTime5 - renderStartTime4;
  var time5 = renderStartTime6 - renderStartTime5;
  var time6 = renderStartTime7 - renderStartTime6;
  var time7 = renderStartTime8 - renderStartTime7;
  var time8 = renderStartTime9 - renderStartTime8;
  var time9 = renderStartTime10 - renderStartTime9;
  console.log("renderFilterBox time: " + time1 + " ms, " + time2 + " ms, " + time3 + " ms, " + time4 + " ms, " + time5 + " ms, " + time6 + " ms, " + time7 + " ms, " + time8 + " ms, " + time9 + " ms");
  
}
