
import { BrowserInfo } from "./browser_info";
import { Render } from "./doi_filter_result_render";
import { renderFilterBox } from "./doi_filter_box_render";
import { DOIFilterResult } from "./doi_filter_result";


export function updatePaginationControls(browserInfo: BrowserInfo) {
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
  }

export function process(browserInfo: BrowserInfo){
    console.log("process");
    if(browserInfo.doiInfoCollection != null){
      const doiIDs = Array.from({length: browserInfo.doiInfoCollection!.length()}, (_, index) => index);
      if(browserInfo.doiInfoSearchInput.is_empty()){
        browserInfo.foundDOIList = Array.from({length: browserInfo.doiInfoCollection!.length()}, (_, index) => browserInfo.doiInfoCollection!.getDOIInfo(index));
      }else{
        const foundDOIIDs = browserInfo.doiInfoSearchInput.search(new DOIFilterResult(doiIDs, browserInfo.doiInfoCollection!), browserInfo.doiInfoCollection!);
        browserInfo.foundDOIList = foundDOIIDs.map(doiID => browserInfo.doiInfoCollection!.getDOIInfo(doiID));
      }
      browserInfo.pageNumber = 0; // 検索結果が変わったら最初のページに戻る  
  
  
      renderFilterBox(browserInfo);
      Render.render(browserInfo);
      updatePaginationControls(browserInfo);
    }
  }
  


export function filterInputChange(inputElementName : string, browserInfo: BrowserInfo) {
    if(inputElementName == "type") {
      const type = (document.getElementById("type-select") as HTMLSelectElement).value;
      if(type == "dont-care") {
        browserInfo.doiInfoSearchInput.type = null;
      }else{
        browserInfo.doiInfoSearchInput.type = type;
      }
    }
    else if(inputElementName == "container-title") {
      const containerTitle = (document.getElementById("container-title-select") as HTMLSelectElement).value;
      if(containerTitle == "dont-care") {
        browserInfo.doiInfoSearchInput.container_title = null;
      }else{
        browserInfo.doiInfoSearchInput.container_title = containerTitle;
      }
    }
    else if(inputElementName == "year-from") {
      const yearFrom = (document.getElementById("year-from-select") as HTMLSelectElement).value;
      if(yearFrom == "dont-care") {
        browserInfo.doiInfoSearchInput.minimum_year = null;
      }else{
        browserInfo.doiInfoSearchInput.minimum_year = parseInt(yearFrom);
      }
    }
    else if(inputElementName == "year-to") {
      const yearTo = (document.getElementById("year-to-select") as HTMLSelectElement).value;
      if(yearTo == "dont-care") {
        browserInfo.doiInfoSearchInput.maximum_year = null;
      }else{
        browserInfo.doiInfoSearchInput.maximum_year = parseInt(yearTo);
      }
    }
    else{
  
    }
    process(browserInfo);
  }
  
  