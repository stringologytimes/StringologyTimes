import { PrimarySearchResult } from "../../doi_filter/primary_search_result";
import { SummaryInfo } from "../../doi_filter/summary_info";
import { SearchResultViewSettings } from "../../doi_filter/search_result_view_settings";


export function renderSeriesTitleList(filterResult: PrimarySearchResult, viewSetting: SearchResultViewSettings, summaryInfo: SummaryInfo) {
    const outputDiv = document.getElementById("output");
    if (!outputDiv) {
        return;
    }

    outputDiv.replaceChildren();

    
    const seriesTitleList = new Array<string>();
    const p = viewSetting.pageNumber! * viewSetting.pageSize!;
    for(let i = p; i < p + viewSetting.pageSize!; i++){
        if(i >= summaryInfo.seriesTitleList.length){
            break;
        }
        seriesTitleList.push(summaryInfo.seriesTitleList[i]);
    }

    const ol = document.createElement('ol');
    ol.setAttribute("start", (p+1).toString());

    console.log(seriesTitleList);
    console.log(summaryInfo.seriesTitleCountList);


    seriesTitleList.forEach((seriesTitle, index) => {


        const li = document.createElement('li');
        const a = document.createElement('a');
        a.textContent = seriesTitle;
        a.setAttribute("href", `javascript:void(0)`);
        a.addEventListener("click", (event) => {
            event.preventDefault();
            (window as any).changeParameters([["series_title", seriesTitle], ["view_mode", "container_title_list"], ["page_number", "0"]]);
          });

        li.appendChild(a);
        const span = document.createElement('span');
        span.textContent = ` (${summaryInfo.seriesTitleCountList[index]} articles)`;
        li.appendChild(span);


        ol.appendChild(li);
   
    });
    outputDiv.appendChild(ol);



}
  