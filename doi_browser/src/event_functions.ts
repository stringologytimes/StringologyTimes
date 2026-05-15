
import { BrowserInfo } from "./browser_info";
import { SortByType } from "./doi_filter/doi_filter_query";
import { DOIStatus } from "./doi_info";

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
    browserInfo.render();
  }
  


export function filterInputChange(inputElementName : string, browserInfo: BrowserInfo) {
    if(inputElementName == "type") {
      const type = (document.getElementById("type-select") as HTMLSelectElement).value;
      if(type == "dont-care") {
        browserInfo.currentDOIFilter.query.type = null;
        console.log("type: dont-care" + browserInfo.currentDOIFilter.query.getHash());
      }else{
        browserInfo.currentDOIFilter.query.type = type;
      }
    }
    else if(inputElementName == "container-title") {
      const containerTitle = (document.getElementById("container-title-select") as HTMLSelectElement).value;
      if(containerTitle == "dont-care") {
        browserInfo.currentDOIFilter.query.container_title = null;
      }else{
        browserInfo.currentDOIFilter.query.container_title = containerTitle;
      }
    }
    else if(inputElementName == "year-from") {
      const yearFrom = (document.getElementById("year-from-select") as HTMLSelectElement).value;
      if(yearFrom == "dont-care") {
        browserInfo.currentDOIFilter.query.minimum_year = null;
      }else{
        browserInfo.currentDOIFilter.query.minimum_year = parseInt(yearFrom);
      }
    }
    else if(inputElementName == "year-to") {
      const yearTo = (document.getElementById("year-to-select") as HTMLSelectElement).value;
      if(yearTo == "dont-care") {
        browserInfo.currentDOIFilter.query.maximum_year = null;
      }else{
        browserInfo.currentDOIFilter.query.maximum_year = parseInt(yearTo);
      }
    }
    else if(inputElementName == "sort-by") {
      const sortBy = (document.getElementById("sort-by-select") as HTMLSelectElement).value;
      if(sortBy == "dont-care") {
        browserInfo.currentDOIFilter.query.sortBy = "unordered";
      }else{
        browserInfo.currentDOIFilter.query.sortBy = sortBy as SortByType;
      }
    }
    else if(inputElementName == "tag1") {
      const tag1 = (document.getElementById("tag1-select") as HTMLSelectElement).value;
      if(tag1 == "dont-care") {
        browserInfo.currentDOIFilter.query.tags = [];
      }else{
        browserInfo.currentDOIFilter.query.tags = [tag1];
      }
    }
    else if(inputElementName == "status") {
      const checkboxPrimary = (document.getElementById("checkbox_primary") as HTMLInputElement).checked;
      const checkboxSecondary = (document.getElementById("checkbox_secondary") as HTMLInputElement).checked;
      var excludeStatus: DOIStatus[] = [];
      if(!checkboxPrimary) {
        excludeStatus.push("primary");
      }
      if(!checkboxSecondary) {
        excludeStatus.push("secondary");
      }
      browserInfo.currentDOIFilter.query.excludeStatus = excludeStatus;

    }
    else{
  
    }
    browserInfo.currentDOIFilter.viewSetting.pageNumber = 0;

    process(browserInfo);
  }

  export function ViewSettingInputChange(inputElementName : string, browserInfo: BrowserInfo) {
    if(inputElementName == "view-setting:mode-select") {
      const mode = (document.getElementById("view-setting:mode-select") as HTMLSelectElement).value;
      if(mode == "container_title_list") {
        browserInfo.currentDOIFilter.viewSetting.viewMode = "container_title_list";
      }else{
        browserInfo.currentDOIFilter.viewSetting.viewMode = "article_list";
      }
      browserInfo.currentDOIFilter.viewSetting.pageNumber = 0;
    }
    else if(inputElementName == "view-setting:page-number-select") {
      const pageNumber = (document.getElementById("view-setting:page-number-select") as HTMLSelectElement).value;
      browserInfo.currentDOIFilter.viewSetting.pageNumber = parseInt(pageNumber);
    }
    else if(inputElementName == "view-setting:page-size-select") {
      const pageSize = (document.getElementById("view-setting:page-size-select") as HTMLSelectElement).value;
      browserInfo.currentDOIFilter.viewSetting.pageSize = parseInt(pageSize);
    }
    process(browserInfo);
  }
  
  