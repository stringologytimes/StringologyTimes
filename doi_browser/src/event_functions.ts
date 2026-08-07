
import { BrowserInfo } from "./browser_info";
import { DOIStatus } from "./doi_record";
import { PrimarySearchFilterRender } from "./render/settings/primary_search_filter_render";
import { URLProcessor } from "./url_processor";
import { ContainerDOIFieldsetFunctions } from "./render/settings/fieldset/container_doi_fieldset_functions";
import { SearchFilterBoxFunctions } from "./render/settings/fieldset/search_filter_box_functions";

export function updatePaginationControls(browserInfo: BrowserInfo) {
  /*
    const prevButton = document.getElementById('prevPageButton');
    const nextButton = document.getElementById('nextPageButton');
    const pageInfo = document.getElementById('pageInfo');
  
    if (!browserInfo.foundDOIList || browserInfo.foundDOIList.length === 0) {
      if (prevButton) (prevButton as HTMLButtonElement).disabled = true;
      if (nextButton) (nextButton as HTMLButtonElement).disabled = true;
      if (pageInfo) pageInfo.textContent = '';
      return;
    }
  
    const totalPages = Math.ceil(browserInfo.foundDOIList.length / browserInfo.pageSize);
    const currentPage = browserInfo.pageNumber + 1;
  
    if (prevButton) {
      (prevButton as HTMLButtonElement).disabled = browserInfo.pageNumber === 0;
    }
    if (nextButton) {
      (nextButton as HTMLButtonElement).disabled = browserInfo.pageNumber >= totalPages - 1;
    }
    if (pageInfo) {
      const startIndex = browserInfo.pageNumber * browserInfo.pageSize + 1;
      const endIndex = Math.min(startIndex + browserInfo.pageSize - 1, browserInfo.foundDOIList.length);
      pageInfo.textContent = `ページ ${currentPage}/${totalPages} (${startIndex}-${endIndex} / ${browserInfo.foundDOIList.length}件)`;
    }
    */
}

export async function process(browserInfo: BrowserInfo) {
  const url = new URL(window.location.href);
  const primarySearchFilter = URLProcessor.buildPrimarySearchFilterFromURL();

  


  //browserInfo.processURLParameters();
  //browserInfo.processCurrentDOIFilterInput();
  await browserInfo.render(primarySearchFilter);
}

export function clickPrimarySearchFilterButton(browserInfo: BrowserInfo) {
  if(browserInfo.doiInfoCollection == null) {
    throw new Error("doiInfoCollection is null");
  }else{
    const newParameters = SearchFilterBoxFunctions.convertInputToURLParameters(true);
    SearchFilterBoxFunctions.setURLParameters(true, newParameters);
  }
}

export function primarySearchFilterChange(inputElementName: string, browserInfo: BrowserInfo) {
  if(browserInfo.doiInfoCollection == null) {
    throw new Error("doiInfoCollection is null");
  }else{
    console.log("primarySearchFilterChange/" + inputElementName);
    if (inputElementName == "psf-top-container-type") {
      const selectedTopContainerType = (document.getElementById("psf-top-container-type-select") as HTMLSelectElement).value;
      ContainerDOIFieldsetFunctions.selectTopContainerTypeBox(true, selectedTopContainerType, browserInfo.doiInfoCollection, false, browserInfo.doiInfoCollection.recordSummary.idToPrimaryRecordCountMapper, browserInfo.doiInfoCollection.recordSummary.idToSecondaryRecordCountMapper);
    }
    else if (inputElementName == "psf-top-container") {
      const selectedTopContainer = (document.getElementById("psf-top-container-select") as HTMLSelectElement).value;      
      ContainerDOIFieldsetFunctions.selectTopContainerBox(true, selectedTopContainer, browserInfo.doiInfoCollection, false, browserInfo.doiInfoCollection.recordSummary.idToPrimaryRecordCountMapper, browserInfo.doiInfoCollection.recordSummary.idToSecondaryRecordCountMapper);
    }
  }


}

export function filterInputChange(inputElementName: string, browserInfo: BrowserInfo) {
  const url = new URL(window.location.href);

  /*
  
  if (inputElementName == "type") {
    const selectedTypeValues = getSelectedTypeValues();
    console.log("selectedTypeValues", selectedTypeValues);


    const existingTypes = url.searchParams.getAll("type");
    const removedTypesSet = new Set(existingTypes);

    selectedTypeValues.forEach(type => {
      if(existingTypes.includes(type)){
        removedTypesSet.delete(type);
      }else{
        url.searchParams.append("type", type);
      }
    });

    removedTypesSet.forEach(type => {
      url.searchParams.delete("type", type);
    });
  }
  else if (inputElementName == "container-title") {
    const containerTitle = (document.getElementById("container-title-select") as HTMLSelectElement).value;
    if (containerTitle == "dont-care") {
      //browserInfo.currentDOIFilter.query.container_title = null;
      url.searchParams.delete("container_title");
    } else {
      //browserInfo.currentDOIFilter.query.container_title = containerTitle;
      url.searchParams.set("container_title", containerTitle);
    }
  }
  else if (inputElementName == "series-title") {
    const seriesTitle = (document.getElementById("series-title-select") as HTMLSelectElement).value;
    if (seriesTitle == "dont-care") {
      url.searchParams.delete("series_title");
    } else {
      url.searchParams.set("series_title", seriesTitle);
    }
  }
  else if (inputElementName == "year-from") {
    const yearFrom = (document.getElementById("year-from-select") as HTMLSelectElement).value;
    if (yearFrom == "dont-care") {
      //browserInfo.currentDOIFilter.query.minimum_year = null;
      url.searchParams.delete("minimum_year");
    } else {
      //browserInfo.currentDOIFilter.query.minimum_year = parseInt(yearFrom);
      url.searchParams.set("minimum_year", yearFrom);
    }
  }
  else if (inputElementName == "year-to") {
    const yearTo = (document.getElementById("year-to-select") as HTMLSelectElement).value;
    if (yearTo == "dont-care") {
      //browserInfo.currentDOIFilter.query.maximum_year = null;
      url.searchParams.delete("maximum_year");
    } else {
      //browserInfo.currentDOIFilter.query.maximum_year = parseInt(yearTo);
      url.searchParams.set("maximum_year", yearTo);
    }
  }
  else if (inputElementName == "sort-by") {
    const sortBy = (document.getElementById("sort-by-select") as HTMLSelectElement).value;
    if (sortBy == "dont-care") {
      //browserInfo.currentDOIFilter.query.sortBy = "unordered";
      url.searchParams.delete("sort_by");
    } else {
      //browserInfo.currentDOIFilter.query.sortBy = sortBy as SortByType;
      url.searchParams.set("sort_by", sortBy);
    }
  }
  else if (inputElementName == "tag") {
    const tag1 = (document.getElementById("tag1-select") as HTMLSelectElement).value;
    url.searchParams.delete("tag");
    if (tag1 != "dont-care") {
      url.searchParams.append("tag", tag1);

      //browserInfo.currentDOIFilter.query.tags = [];
    }
  }
  else if (inputElementName == "status") {
    const checkboxPrimary = (document.getElementById("checkbox_primary") as HTMLInputElement).checked;
    const checkboxSecondary = (document.getElementById("checkbox_secondary") as HTMLInputElement).checked;

    console.log("checkboxPrimary", checkboxPrimary);
    console.log("checkboxSecondary", checkboxSecondary);
    var excludeStatus: DOIStatus[] = [];
    if (!checkboxPrimary) {
      excludeStatus.push("primary");
    }
    if (!checkboxSecondary) {
      excludeStatus.push("secondary");
    }

    url.searchParams.delete("exclude_status");
    excludeStatus.forEach(status => {
      url.searchParams.append("exclude_status", status);
    });
  //browserInfo.currentDOIFilter.query.excludeStatus = excludeStatus;
  }
  else if (inputElementName == "keywords") {
    const keyword = (document.getElementById("keywords-input") as HTMLInputElement).value;
    url.searchParams.set("keyword", keyword);
  }
  else {

  }
  url.searchParams.set("page_number", "0");
  //browserInfo.currentDOIFilter.viewSetting.pageNumber = 0;
  history.pushState({}, "", url);

  void process(browserInfo);
  */
}

export function ViewSettingInputChange(inputElementName: string, browserInfo: BrowserInfo) {
  const url = new URL(window.location.href);
  if (inputElementName == "view-mode") {
    const selected = document.querySelector('input[name="view-mode-checkbox"]:checked');
    if (selected) {
      var value = (selected as HTMLInputElement).value;
      url.searchParams.set("view_mode", value);
    } else {
      url.searchParams.delete("view_mode");
    }
    url.searchParams.set("page_number", "0");
  }
  else if (inputElementName == "page-number") {
    const pageNumber = (document.getElementById("view-setting:page-number-select") as HTMLSelectElement).value;
    url.searchParams.set("page_number", pageNumber);
    console.log("pageNumber", pageNumber);
  }
  else if (inputElementName == "page-size") {
    const pageSize = (document.getElementById("view-setting:page-size-select") as HTMLSelectElement).value;
    url.searchParams.set("page_size", pageSize);
  }
  history.pushState({}, "", url);
  void process(browserInfo);
}

