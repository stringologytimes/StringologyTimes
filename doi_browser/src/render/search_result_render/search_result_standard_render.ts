import { containerTypeList, DOIRecord } from "../../doi_record";
import { DOIRecordCollection } from "../../doi_record_collection";
import { DOIRecordTemplate } from "./templates/doi_record_template";
import { ContainerRecordRender } from "./container_record_render";
import { DOIRecordDetailsTemplate } from "./templates/doi_record_details_template";




export class SearchResultStandardRender {




    public static render(foundRecordIDs: number[], doiIndex: number, doiInfoCollection: DOIRecordCollection) {

        const outputDiv = document.getElementById("output");
        if (!outputDiv) {
            return;
        }

        outputDiv.replaceChildren();

        if (foundRecordIDs.length == 0) {
            outputDiv.innerHTML = "<p>No articles found.</p>";
        } else {
            const doiInfoTemplate = document.getElementById('doi-record-template') as HTMLTemplateElement;
            const authorTemplate = document.getElementById('author-template') as HTMLTemplateElement;
            const doiReferenceTemplate = document.getElementById('doi-reference-template') as HTMLTemplateElement;
            //const detailsDivTemplate = document.getElementById('details-div-standard-template') as HTMLTemplateElement;

            if (!doiInfoTemplate) {
                throw new Error("doiInfoTemplate not found.");
            }
            if (!authorTemplate) {
                throw new Error("authorTemplate not found.");
            }
            if (!doiReferenceTemplate) {
                throw new Error("doiReferenceTemplate not found.");
            }


            const ol = document.createElement('ol');
            ol.setAttribute("start", (doiIndex+1).toString());

            //const currentDOIListPart = browserInfo.getCurrentDOIListPart();

            foundRecordIDs.forEach((doiID, index) => {
                const doiInfo = doiInfoCollection.getDOIInfo(doiID);
                var li = document.createElement('li');

                const isContainerRecord = containerTypeList.includes(doiInfo.type);
                DOIRecordTemplate.setArticleTemplate(li, doiInfoTemplate, isContainerRecord);
                DOIRecordTemplate.renderDOIRecordSummary(li, doiID, doiInfoCollection);
                
                if(isContainerRecord){
                    ContainerRecordRender.renderDOISub(li, doiID, doiInfoCollection);
                }else{
                    //DOIRecordDetailsTemplate.renderDOIRecordDetails(li, doiID, doiInfoCollection, detailsDivTemplate);
                }

                ol.appendChild(li);
            });
            outputDiv.appendChild(ol);

        }

    }
}
