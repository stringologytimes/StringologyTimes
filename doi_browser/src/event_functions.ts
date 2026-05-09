
import { BrowserInfo } from "./browser_info";
import { Render } from "./render/doi_filter_result_render";
import { renderFilterBox } from "./render/doi_filter_box_render";

import { DOIFilterResult } from "./doi_filter/doi_filter_result";
import { DOIFilterPartialResult } from "./doi_filter/doi_filter_partial_result";
import { DOIFilterStandardRender } from "./render/doi_filter_standard_render";


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

export function process(browserInfo: BrowserInfo){
    console.log("process");
    browserInfo.processCurrentDOIFilterInput();
    if(browserInfo.doiInfoCollection != null){

      const currentDOIFilterPartialResult = browserInfo.getCurrentDOIFilterPartialResult();
      const currentDOIFilterWithViewSetting = browserInfo.getCurrentDOIFilterWithViewSetting();      
      const currentDOIFilterResult = browserInfo.getCurrentDOIFilterResult();
      const currentSummaryInfo = browserInfo.getCurrentSummaryInfo();
      renderFilterBox(currentDOIFilterResult, currentDOIFilterWithViewSetting.query, browserInfo.doiInfoCollection!, currentSummaryInfo);
      if(currentDOIFilterWithViewSetting.viewSetting.viewMode == "article_list"){
        DOIFilterStandardRender.render(currentDOIFilterPartialResult, browserInfo.doiInfoCollection!);
      }else{
        throw new Error("Unknown view mode");
      }
  
      //Render.render(browserInfo);
      //updatePaginationControls(browserInfo);
    }
  }
  


export function filterInputChange(inputElementName : string, browserInfo: BrowserInfo) {
    if(inputElementName == "type") {
      const type = (document.getElementById("type-select") as HTMLSelectElement).value;
      if(type == "dont-care") {
        browserInfo.currentDOIFilterWithViewSetting.query.type = null;
      }else{
        browserInfo.currentDOIFilterWithViewSetting.query.type = type;
      }
    }
    else if(inputElementName == "container-title") {
      const containerTitle = (document.getElementById("container-title-select") as HTMLSelectElement).value;
      if(containerTitle == "dont-care") {
        browserInfo.currentDOIFilterWithViewSetting.query.container_title = null;
      }else{
        browserInfo.currentDOIFilterWithViewSetting.query.container_title = containerTitle;
      }
    }
    else if(inputElementName == "year-from") {
      const yearFrom = (document.getElementById("year-from-select") as HTMLSelectElement).value;
      if(yearFrom == "dont-care") {
        browserInfo.currentDOIFilterWithViewSetting.query.minimum_year = null;
      }else{
        browserInfo.currentDOIFilterWithViewSetting.query.minimum_year = parseInt(yearFrom);
      }
    }
    else if(inputElementName == "year-to") {
      const yearTo = (document.getElementById("year-to-select") as HTMLSelectElement).value;
      if(yearTo == "dont-care") {
        browserInfo.currentDOIFilterWithViewSetting.query.maximum_year = null;
      }else{
        browserInfo.currentDOIFilterWithViewSetting.query.maximum_year = parseInt(yearTo);
      }
    }
    else{
  
    }
    process(browserInfo);
  }
  
  