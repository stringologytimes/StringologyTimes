import { PrimarySearchResult } from "../../doi_filter/primary_search_result";
import { SummaryInfo } from "../../doi_filter/summary_info";
import { SearchResultViewSettings } from "../../doi_filter/search_result_view_settings";
import { DOIRecordCollection } from "../../doi_record_collection";
import { DOIRecordTemplate } from "./templates/doi_record_template";
import { DOIRecordDetailsTemplate } from "./templates/doi_record_details_template";


export class ContainerRecordRender {
    public static renderDOISub(li: HTMLElement, doiID: number, doiInfoCollection: DOIRecordCollection) {
        //var doiRecord = doiInfoCollection.getDOIInfo(doiID);
        const detailsDiv = li.querySelector('.details_div') as HTMLDivElement;
        if (!detailsDiv) {
            return;
        }
        const doiInfoTemplate = document.getElementById('doi-record-template') as HTMLTemplateElement;
        if (!doiInfoTemplate) {
            throw new Error("doiInfoTemplate is not found");
        }
        /*
        const detailsDivTemplate = document.getElementById('details-div-standard-template') as HTMLTemplateElement;
        if (!detailsDivTemplate) {
            throw new Error("detailsDivTemplate is not found");
        }
        */

        if(doiInfoCollection.idToDOIChildrenIDMapper.has(doiID)){
            const childrenIDs = doiInfoCollection.idToDOIChildrenIDMapper.get(doiID)!;
            const ol2 = document.createElement('ol');            
            ol2.setAttribute("start", (1).toString());
            ol2.setAttribute("class", "children-ol");
            childrenIDs.forEach((childrenID, index) => {
                const li2 = document.createElement('li');
                DOIRecordTemplate.setArticleTemplate(li2, doiInfoTemplate, false);
                DOIRecordTemplate.renderDOIRecordSummary(li2, childrenID, doiInfoCollection);
                //DOIRecordDetailsTemplate.renderDOIRecordDetails(li2, childrenID, doiInfoCollection, detailsDivTemplate);
                ol2.appendChild(li2);
            });
            detailsDiv.appendChild(ol2);
        }
    }

    /*
    public static renderDOIRecord(li: HTMLElement, doiID: number, doiInfoCollection: DOIRecordCollection) {
        var doiRecord = doiInfoCollection.getDOIInfo(doiID);
        const titleSpan = document.createElement('span');
        titleSpan.textContent = doiRecord.title;
        li.appendChild(titleSpan);

        if(doiInfoCollection.idToDOIChildrenIDMapper.has(doiID)){
            const childrenIDs = doiInfoCollection.idToDOIChildrenIDMapper.get(doiID)!;
            const ol2 = document.createElement('ol');
            ol2.setAttribute("start", (1).toString());
            childrenIDs.forEach((childrenID, index) => {
                const li2 = document.createElement('li');
                DOIRecordTemplate.renderDOIRecord(li2, childrenID, doiInfoCollection);
                ol2.appendChild(li2);
            });
            li.appendChild(ol2);
        }
    }
    */

    public static render(filterResult: PrimarySearchResult, viewSetting: SearchResultViewSettings, summaryInfo: SummaryInfo, doiInfoCollection: DOIRecordCollection) {
        throw new Error("ContainerRecordRender.render is not implemented");

        /*

        const outputDiv = document.getElementById("output");
        if (!outputDiv) {
            return;
        }


        outputDiv.replaceChildren();

        if (filterResult.doiIDs.length == 0) {
            outputDiv.innerHTML = "<p>No articles found.</p>";
        } else {
            const ol = document.createElement('ol');
            const p = viewSetting.pageNumber! * viewSetting.pageSize!;
            ol.setAttribute("start", (p+1).toString());

            filterResult.doiIDs.forEach((doiID, index) => {
                const li = document.createElement('li');
                //this.renderDOIRecord(li, doiID, doiInfoCollection);
                ol.appendChild(li);

            });
            outputDiv.appendChild(ol);

        }
        */

    }
}
