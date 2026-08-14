import { PrimarySearchResult } from "../../doi_filter/primary_search_result";
import { SummaryInfo } from "../../doi_filter/summary_info";
import { SearchResultViewSettings } from "../../doi_filter/search_result_view_settings";
import { DOIRecordCollection } from "../../doi_record_collection";
import { DOIFilterStandardRender } from "./doi_filter_standard_render";


export class ContainerRecordRender {
    public static render(filterResult: PrimarySearchResult, viewSetting: SearchResultViewSettings, summaryInfo: SummaryInfo, doiInfoCollection: DOIRecordCollection) {

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
                        DOIFilterStandardRender.renderDOIRecord(li2, childrenID, doiInfoCollection);
                        ol2.appendChild(li2);
                    });
                    li.appendChild(ol2);
                }
                ol.appendChild(li);

            });
            outputDiv.appendChild(ol);

        }

    }
}
