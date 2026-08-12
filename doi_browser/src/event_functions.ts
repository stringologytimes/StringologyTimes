
import { BrowserInfo } from "./browser_info";
//import { DOIStatus } from "./doi_record";
//import { PrimarySearchFilterRender } from "./render/settings/primary_search_filter_render";
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

/*
export async function process(browserInfo: BrowserInfo) {
}
*/

/*
export async function buildFromURLParameters(browserInfo: BrowserInfo){
  if(browserInfo.doiInfoCollection == null) {
    throw new Error("doiInfoCollection is null");
  }else{
    const primarySearchFilter = URLProcessor.buildSearchFilterFromURL(true);
    //SearchFilterBoxFunctions.setURLParameters(true, primarySearchFilter);


    throw new Error("not implemented");  
  }
}
*/


export async function clickPrimarySearchFilterButton(browserInfo: BrowserInfo) {
  await browserInfo.rebuildByChangingPrimarySearchFilterBox();
}

export function primarySearchFilterChange(inputElementName: string, browserInfo: BrowserInfo) {
  if (browserInfo.doiInfoCollection == null) {
    throw new Error("doiInfoCollection is null");
  } else {
    if (inputElementName == "psf-top-container-type") {
      const selectedTopContainerType = (document.getElementById("psf-top-container-type-select") as HTMLSelectElement).value;
      console.log("primarySearchFilterChange: " + inputElementName + " / " + selectedTopContainerType);
      const v = selectedTopContainerType == "Any" ? null : selectedTopContainerType;
      ContainerDOIFieldsetFunctions.selectTopContainerTypeBox(true, v, null, false, browserInfo.doiInfoCollection.recordSummary.idToPrimaryRecordCountMapper, browserInfo.doiInfoCollection.recordSummary.idToSecondaryRecordCountMapper, browserInfo.doiInfoCollection);
    }
    else if (inputElementName == "psf-top-container") {
      const selectedTopContainer = (document.getElementById("psf-top-container-select") as HTMLSelectElement).value;
      const v = selectedTopContainer == "Any" ? null : selectedTopContainer;
      ContainerDOIFieldsetFunctions.selectTopContainerBox(true, v, null, false, 
        browserInfo.doiInfoCollection.recordSummary.idToPrimaryRecordCountMapper, browserInfo.doiInfoCollection.recordSummary.idToSecondaryRecordCountMapper, browserInfo.doiInfoCollection);
    }
  }
}

export async function secondarySearchFilterChange(inputElementName: string, browserInfo: BrowserInfo) {
  if (browserInfo.doiInfoCollection == null) {
    throw new Error("doiInfoCollection is null");
  } else {
    if (inputElementName == "ssf-top-container-type") {
      const selectedTopContainerType = (document.getElementById("ssf-top-container-type-select") as HTMLSelectElement).value;
      const v = selectedTopContainerType == "Any" ? null : selectedTopContainerType;
      ContainerDOIFieldsetFunctions.selectTopContainerTypeBox(false, v, null, true, 
        browserInfo.getCurrentPrimarySummary().idToPrimaryRecordCountMapper, browserInfo.getCurrentPrimarySummary().idToSecondaryRecordCountMapper, 
        browserInfo.doiInfoCollection);
    }
    else if (inputElementName == "ssf-top-container") {
      const selectedTopContainer = (document.getElementById("ssf-top-container-select") as HTMLSelectElement).value;
      const v = selectedTopContainer == "Any" ? null : selectedTopContainer;
      ContainerDOIFieldsetFunctions.selectTopContainerBox(false, v, null, true,  
        browserInfo.getCurrentPrimarySummary().idToPrimaryRecordCountMapper, browserInfo.getCurrentPrimarySummary().idToSecondaryRecordCountMapper, 
        browserInfo.doiInfoCollection);
    }


    await browserInfo.rebuildByChangingSecondarySearchFilterBox();

  }

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
  //void process(browserInfo);
}

