import { DOIRecord } from "../../doi_record";
import { DOIRecordCollection } from "../../doi_record_collection";
import { DOIRecordTemplate } from "./templates/doi_record_template";




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

            if (!doiInfoTemplate || !authorTemplate || !doiReferenceTemplate) {
                outputDiv.innerHTML = "<p>Error: Templates not found.</p>";
                return;
            }

            const ol = document.createElement('ol');
            ol.setAttribute("start", (doiIndex+1).toString());

            //const currentDOIListPart = browserInfo.getCurrentDOIListPart();

            foundRecordIDs.forEach((doiID, index) => {
                var li = document.createElement('li');
                DOIRecordTemplate.renderDOIRecord(li, doiID, doiInfoCollection);
                ol.appendChild(li);
            });
            outputDiv.appendChild(ol);

        }

    }
}
