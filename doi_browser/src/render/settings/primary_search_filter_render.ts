import { PrimarySearchResult } from "../../doi_filter/primary_search_result";
import { DOIRecordCollection } from "../../doi_record_collection";
import { PrimarySearchFilter } from "../../doi_filter/primary_search_filter";
import { SummaryInfo } from "../../doi_filter/summary_info";
import { SortByType } from "../../doi_filter/primary_search_filter";
import { getDOIRecordTypeList } from "../../doi_record_collection";
import { containerTypeList, paperTypeList, topContainerTypeList } from "../../doi_record";


let topContainerCategories: string[] = ["Any","Journal",  "Proceedings", "Preprint"];
let containerSelect2Items: [string, string][] = [];

export class PrimarySearchFilterRender {
  public static initialize(doiRecordCollection: DOIRecordCollection): void {
    const recordTypeCounters: Map<string, number> = new Map<string, number>();

    doiRecordCollection.lightweightDOIRecords.forEach(record => {
      const recordType = record.type;
      if (recordTypeCounters.has(recordType)) {
        recordTypeCounters.set(recordType, recordTypeCounters.get(recordType)! + 1);
      } else {
        recordTypeCounters.set(recordType, 1);
      }
    }
    );
    this.initializeRecordTypes(recordTypeCounters, doiRecordCollection);
    this.initializeContainerBox(doiRecordCollection);
  }

  public static initializeRecordTypes(recordTypeCounters: Map<string, number>, doiRecordCollection: DOIRecordCollection) {
    const typeListContainerSpan = document.getElementById("psf-container-types-span");
    if (typeListContainerSpan == null) {
      throw new Error("psf-container-types-span is not found");
    }
    const typeListPaperSpan = document.getElementById("psf-paper-types-span");
    if (typeListPaperSpan == null) {
      throw new Error("psf-paper-types-span is not found");
    }
    const typeListOtherSpan = document.getElementById("psf-other-types-span");
    if (typeListOtherSpan == null) {
      throw new Error("typeListOtherDiv is not found");
    }





    getDOIRecordTypeList().forEach(type => {
      let count = 0;
      if (recordTypeCounters.has(type)) {
        count = recordTypeCounters.get(type)!;
      }
      const labelName = `${type} (${count})`;

      const checkbox = document.createElement("input");
      checkbox.type = "checkbox";
      checkbox.id = "psf-checkbox_" + type;
      checkbox.value = type;
      checkbox.checked = true;

      const label = document.createElement("label");
      label.htmlFor = "psf-checkbox_" + type;
      label.textContent = labelName;

      if (containerTypeList.includes(type)) {
        typeListContainerSpan.appendChild(checkbox);
        typeListContainerSpan.appendChild(label);
      } else if (paperTypeList.includes(type)) {
        typeListPaperSpan.appendChild(checkbox);
        typeListPaperSpan.appendChild(label);
      } else {
        typeListOtherSpan.appendChild(checkbox);
        typeListOtherSpan.appendChild(label);
      }
    });
  }

  public static selectTopContainerBox(selectedTopContainer: string, doiRecordCollection: DOIRecordCollection) {
    const subContainerSelect = document.getElementById("psf-sub-container-select");
    if (subContainerSelect == null) {
      throw new Error("psf-sub-container-select is not found");
    }    


    subContainerSelect.innerHTML = "";

    if(selectedTopContainer == "Any") {
      const option = document.createElement("option");
      option.value = "Any";
      option.textContent = "Any";
      subContainerSelect.appendChild(option);
    }else if(doiRecordCollection.doiToIDMapper.has(selectedTopContainer)){
      var selected_doi_id = doiRecordCollection.doiToIDMapper.get(selectedTopContainer)!;
      console.log("selected_doi_id/" + selected_doi_id + " / " + doiRecordCollection.idToDOIChildrenIDMapper.get(selected_doi_id)?.length);
      doiRecordCollection.idToDOIChildrenIDMapper.get(selected_doi_id)?.forEach(child_id => {
        var child_doi_record = doiRecordCollection.lightweightDOIRecords[child_id];
        var primaryCount = doiRecordCollection.idToPrimaryRecordCountMapper.get(child_id) ?? 0;
        var secondaryCount = doiRecordCollection.idToSecondaryRecordCountMapper.get(child_id) ?? 0;

        const option = document.createElement("option");
        option.value = child_doi_record.doi;
        option.textContent = `${child_doi_record.title} (${primaryCount} primary records, ${secondaryCount} secondary records)`;
        subContainerSelect.appendChild(option);
      });
    }else{
      throw new Error("selectedTopContainer is not found");
    }
  }


  public static selectTopContainerTypeBox(selectedTopContainerType: string, doiRecordCollection: DOIRecordCollection) {
    const topContainerTypeSelect : HTMLSelectElement = document.getElementById("psf-top-container-type-select") as HTMLSelectElement;
    if (topContainerTypeSelect == null) {
      throw new Error("psf-top-container-type-select is not found");
    }

    const topContainerSelect = document.getElementById("psf-top-container-select");
    if (topContainerSelect == null) {
      throw new Error("psf-top-container-select is not found");
    }

    //const selectedValue : string = topContainerTypeSelect.value;
    topContainerSelect.innerHTML = "";

    const mapper: Map<string, string> = new Map<string, string>();
    mapper.set("Journal", "Journal");
    mapper.set("Proceedings", "Proceedings Series");
    mapper.set("Preprint", "Preprint Repository");

    if(mapper.has(selectedTopContainerType)) {
      const key = mapper.get(selectedTopContainerType);
      doiRecordCollection.recordTypeToIDMapper.forEach((idList, recordType) => {
        if(recordType == key) {
          idList.forEach(id => {
            var doiRecord = doiRecordCollection.lightweightDOIRecords[id];
            var primaryCount = doiRecordCollection.idToPrimaryRecordCountMapper.get(id) ?? 0;
            var secondaryCount = doiRecordCollection.idToSecondaryRecordCountMapper.get(id) ?? 0;
            
            const option = document.createElement("option");
            option.value = doiRecord.doi;
            option.textContent = `${doiRecord.title} (${primaryCount} primary records, ${secondaryCount} secondary records)`;
            topContainerSelect.appendChild(option);
          }
        );
        }
      });
    }else{
      const option = document.createElement("option");
      option.value = "Any";
      option.textContent = "Any";
      topContainerSelect.appendChild(option);
    }    
  }




  public static initializeContainerBox(doiRecordCollection: DOIRecordCollection) {
    const topContainerTypeSelect = document.getElementById("psf-top-container-type-select");
    if (topContainerTypeSelect == null) {
      throw new Error("psf-top-container-type-select is not found");
    }
    /*
    const topContainerSelect = document.getElementById("psf-top-container-select");
    if (topContainerSelect == null) {
      throw new Error("psf-top-container-select is not found");
    }
    const subContainerSelect = document.getElementById("psf-sub-container-select");
    if (subContainerSelect == null) {
      throw new Error("psf-sub-container-select is not found");
    }
    */

    topContainerCategories.forEach((topContainerType, index) => {
      const option = document.createElement("option");
      option.value = topContainerType;
      option.textContent = topContainerType;
      topContainerTypeSelect.appendChild(option);
    });


  }

}


function setSelectHTMLElement(selectElement: HTMLSelectElement, options: string[], doiCountList: number[], selectedValue: string | null, dontCareValueName: string) {
  selectElement.innerHTML = "";
  const defaultOption = document.createElement("option");
  defaultOption.value = "dont-care";
  defaultOption.textContent = dontCareValueName;
  selectElement.appendChild(defaultOption);

  var max_children_count = 300;
  var max_index = Math.min(options.length, max_children_count);

  for (var index = 0; index < max_index; index++) {
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

  if (max_index < options.length) {
    const option = document.createElement("option");
    option.value = "more";
    option.textContent = "More";
    selectElement.appendChild(option);
  }

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

export function getSelectedTypeValues(): string[] {
  const typeListContainerDiv = document.getElementById("type-list-container-span");
  if (typeListContainerDiv == null) {
    throw new Error("typeListContainerDiv is not found");
  }
  const typeListPaperDiv = document.getElementById("type-list-paper-span");
  if (typeListPaperDiv == null) {
    throw new Error("typeListPaperDiv is not found");
  }
  const typeListOtherDiv = document.getElementById("type-list-other-span");
  if (typeListOtherDiv == null) {
    throw new Error("typeListOtherDiv is not found");
  }


  const typeListContainer = typeListContainerDiv as HTMLSpanElement;
  const inputElementsForContainers = typeListContainer.querySelectorAll("input");
  const checkedValuesForContainers = Array.from(inputElementsForContainers).filter(element => (element as HTMLInputElement).checked).map(element => (element as HTMLInputElement).value);
  const uncheckedValuesForContainers = Array.from(inputElementsForContainers).filter(element => !(element as HTMLInputElement).checked).map(element => (element as HTMLInputElement).value);

  const inputElementsForPapers = typeListPaperDiv.querySelectorAll("input");
  const checkedValuesForPapers = Array.from(inputElementsForPapers).filter(element => (element as HTMLInputElement).checked).map(element => (element as HTMLInputElement).value);
  const uncheckedValuesForPapers = Array.from(inputElementsForPapers).filter(element => !(element as HTMLInputElement).checked).map(element => (element as HTMLInputElement).value);

  const inputElementsForOthers = typeListOtherDiv.querySelectorAll("input");
  const checkedValuesForOthers = Array.from(inputElementsForOthers).filter(element => (element as HTMLInputElement).checked).map(element => (element as HTMLInputElement).value);
  const uncheckedValuesForOthers = Array.from(inputElementsForOthers).filter(element => !(element as HTMLInputElement).checked).map(element => (element as HTMLInputElement).value);


  let result: string[] = [];
  if (uncheckedValuesForContainers.length == 0 && uncheckedValuesForPapers.length == 0 && uncheckedValuesForOthers.length == 0) {
    result = [];
  }
  else if (checkedValuesForContainers.length == 0 && checkedValuesForPapers.length == 0 && checkedValuesForOthers.length == 0) {
    result = ["Null"];
  }
  else {
    if (uncheckedValuesForContainers.length == 0) {
      result.push("Container-Any");
    } else {
      checkedValuesForContainers.forEach(type => {
        result.push(type);
      });
    }

    if (uncheckedValuesForPapers.length == 0) {
      result.push("Paper-Any");
    } else {
      checkedValuesForPapers.forEach(type => {
        result.push(type);
      });
    }

    checkedValuesForOthers.forEach(type => {
      result.push(type);
    });

    /*
    if(uncheckedElementsForOthers.length == 0){
      result.push("Other-Any");
    }else{
    }
    */
  }

  return result;
}

export function setTypeListBoxes(selectedValues: string[], itemNames: string[], itemValues: string[]) {
  var containerSpan = document.getElementById("type-list-container-span");
  var paperSpan = document.getElementById("type-list-paper-span");
  var otherSpan = document.getElementById("type-list-other-span");

  if (containerSpan == null) {
    throw new Error("containerSpan is not found");
  }
  containerSpan.innerHTML = "";
  if (paperSpan == null) {
    throw new Error("paperSpan is not found");
  }
  paperSpan.innerHTML = "";
  if (otherSpan == null) {
    throw new Error("otherSpan is not found");
  }
  otherSpan.innerHTML = "";

  console.log("setTypeListBoxes")

  console.log(selectedValues);
  console.log(containerTypeList)
  console.log(paperTypeList)
  console.log(itemValues)
  console.log(itemNames)


  itemValues.forEach((itemValue, index) => {
    var itemName = itemNames[index];
    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.id = "checkbox_" + itemValue;
    checkbox.value = itemValue;
    checkbox.checked = selectedValues.includes(itemValue);
    const label = document.createElement("label");
    label.htmlFor = "checkbox_type";
    label.textContent = itemName;



    if (containerTypeList.includes(itemValue)) {
      containerSpan!.appendChild(checkbox);
      containerSpan!.appendChild(label);
    } else if (paperTypeList.includes(itemValue)) {
      paperSpan!.appendChild(checkbox);
      paperSpan!.appendChild(label);
    } else {
      otherSpan!.appendChild(checkbox);
      otherSpan!.appendChild(label);
    }

  });
}


function renderDOICategoryBox(summaryInfo: SummaryInfo, selectedValues: string[]) {
  const typeList: string[] = [];
  const typeValues: string[] = [];

  //var selectedValue = selectedValues.length > 0 ? selectedValues[0] : null;


  getDOIRecordTypeList().forEach(type => {
    //typeList.push(type);
    var p = summaryInfo.doiCategoryList.indexOf(type);
    if (p != -1) {
      const count = summaryInfo.doiCategoryCountList[p];
      typeList.push(`${type} (${count})`);
      typeValues.push(type);
    } else {
      typeList.push(`${type} (0)`);
      typeValues.push(type);
    }

  });



  //setRadioBoxes("type-list-div", "type-template", selectedValue == null ? "Any" : selectedValue, typeList, typeValues);
  setTypeListBoxes(selectedValues, typeList, typeValues);

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


export function renderFilterBox(filterResult: PrimarySearchResult, filterInput: PrimarySearchFilter, doiInfoCollection: DOIRecordCollection, summaryInfo: SummaryInfo) {
  console.log("renderFilterBox (size: " + filterResult.doiIDs.length + ")");

  const renderStartTime1 = performance.now();
  renderDOICategoryBox(summaryInfo, filterInput.types);

  const renderStartTime2 = performance.now();
  renderContainerTitleSelectBox(summaryInfo, filterInput.container_title);
  const renderStartTime3 = performance.now();
  renderSeriesTitleSelectBox(summaryInfo, filterInput.series_title);
  const renderStartTime4 = performance.now();
  renderMinimumYearSelectBox(summaryInfo, filterInput.minimum_year, filterInput.maximum_year);
  const renderStartTime5 = performance.now();
  renderMaximumYearSelectBox(summaryInfo, filterInput.minimum_year, filterInput.maximum_year);
  const renderStartTime6 = performance.now();
  //renderSortBySelectBox(filterInput.sortBy);
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
