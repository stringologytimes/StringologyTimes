import { BrowserInfo } from "../browser_info";
import { DOIFilterResult } from "../doi_filter/doi_filter_result";
import { DOIInfoCollection } from "../doi_info";
import { DOIFilterQuery } from "../doi_filter/doi_filter_query";
import { SummaryInfo } from "../doi_filter/summary_info";
import { DOIFilterViewSetting } from "../doi_filter/doi_filter_view_setting";

function getDisplayName(containerTitle: string, containerTitleCount: number){
    const sp = containerTitle.split("---");
    if(sp.length == 2){
        return `${sp[0]} (${sp[1]}, ${containerTitleCount} articles)`;
    }else{
        return `${containerTitle} (${containerTitleCount} articles)`;
    }

}

export function renderContainerTitleList(filterResult: DOIFilterResult, viewSetting: DOIFilterViewSetting, summaryInfo: SummaryInfo) {
    const outputDiv = document.getElementById("output");
    if (!outputDiv) {
        return;
    }

    outputDiv.innerHTML = "";

    
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
  