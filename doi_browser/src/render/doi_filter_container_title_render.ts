import { PrimarySearchResult } from "../doi_filter/primary_search_result";
import { SummaryInfo } from "../doi_filter/summary_info";
import { SearchResultViewSettings } from "../doi_filter/search_result_view_settings";

function getDisplayName(containerTitle: string, containerTitleCount: number){
    const sp = containerTitle.split("---");
    if(sp.length == 2){
        return `${sp[0]} (${sp[1]}, ${containerTitleCount} articles)`;
    }else{
        return `${containerTitle} (${containerTitleCount} articles)`;
    }

}

export function renderContainerTitleList(filterResult: PrimarySearchResult, viewSetting: SearchResultViewSettings, summaryInfo: SummaryInfo) {
    const outputDiv = document.getElementById("output");
    if (!outputDiv) {
        return;
    }

    outputDiv.replaceChildren();

    
    const containerTitleList = new Array<string>();
    const p = viewSetting.pageNumber! * viewSetting.pageSize!;
    for(let i = p; i < p + viewSetting.pageSize!; i++){
        if(i >= summaryInfo.containerTitleList.length){
            break;
        }
        containerTitleList.push(summaryInfo.containerTitleList[i]);
    }

    const ol = document.createElement('ol');
    ol.setAttribute("start", (p+1).toString());

    //const containerTitleTemplate = document.getElementById('container-title-template') as HTMLTemplateElement;

    containerTitleList.forEach((containerTitle, index) => {
        /*
        const containerTitleClone = containerTitleTemplate.content.cloneNode(true) as DocumentFragment;
        const containerTitleSpan = containerTitleClone.querySelector('.container-title');
        if (containerTitleSpan) {
            containerTitleSpan.textContent = containerTitle;
        }
        */

        const li = document.createElement('li');
        const a = document.createElement('a');
        a.textContent = containerTitle;
        a.setAttribute("href", `javascript:void(0)`);
        a.addEventListener("click", (event) => {
            event.preventDefault();
            (window as any).changeParameter("container_title", containerTitle);
          });

        //a.setAttribute("href", `javascript:changeParameter('container_title', '${encodeURIComponent(containerTitle)}')`);
        li.appendChild(a);
        const span = document.createElement('span');
        span.textContent = ` (${summaryInfo.containerTitleCountList[index]} articles)`;
        li.appendChild(span);


        //li.textContent = getDisplayName(containerTitle, summaryInfo.containerTitleCountList[index]);
        //li.setAttribute("onclick", `containerTitleLiElementClick('${containerTitle}')`);
        //li.style.cursor = "pointer";
        //li.classList.add("clickable-list-item");
        ol.appendChild(li);
   
    });
    outputDiv.appendChild(ol);



}
  