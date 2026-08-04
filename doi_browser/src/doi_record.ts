



import { load_gzip_text_lines, load_gzip_integer_list_lines, load_gzip_integer_lines } from "./gzip_loader";

export type DOIStatus = "primary" | "secondary" | "unknown";
//let ContainerTypeList: string[] = ["Book", "Proceedings", "ConferenceProceeding", "ProceedingsSeries", "Journal", 
//    "Journal-Issue", "PreprintRepository", "ReferenceBook", "EditedBook", "Monograph"];




export let topContainerTypeList: string[] = ["Journal", "Proceedings Series", "Preprint Repository"];
export let subContainerTypeList: string[] = ["Book", "Journal Issue", "Proceedings Series", "Edited Book", "Reference Book", "Monograph"];


export let containerTypeList: string[] = ["Book", "Conference Proceedings", "Journal", "Journal Issue", "Proceedings Series", "Edited Book", "Reference Book", "Preprint Repository", "Monograph"];
export let paperTypeList: string[] = ["Proceedings Article", "Book Chapter", "Conference Paper", "Journal Article", "Report", "Preprint", "Monograph Chapter", "Posted Content"];


export class LightWeightDOIRecord {
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authorIDs: number[] = [];
    public isPrimary: boolean = false;
    public type: string = "Unknown";
    public seriesTitle: string = "";
    public container_DOI: string = "";
    public container_title: string = "";
    public volume_issue: string = "";
    public tags: string[] = [];    
    public doiReferenceIDs: number[] = [];
    public optional_ids: string[] = [];

    public isTopContainerType(): boolean {
        return topContainerTypeList.includes(this.type);
    }

}
export class DOIRecord {
    public id: number = -1;
    public doi: string = "";
    public title: string = "";
    public year: number = 0;
    public month: number = 0;
    public authors: string[] = [];
    public seriesTitle: string = "";
    public container_DOI: string = "";
    public container_title: string = "";
    public volume_issue: string = "";
    public tags: string[] = [];
    public doiReferences: string[] = [];
    public keywords: string[] = [];
    public type: string = "Unknown";
    public isPrimary: boolean = false;
    public optional_ids: string[] = [];
    public doi_children: string[] = [];


    public getStatus(): DOIStatus {
        if (this.isPrimary) {
            return "primary";
        } else {
            return "secondary";
        }
    }

    public isContainerType(): boolean {
        return containerTypeList.includes(this.type);
    }
}




//export let doiInfoCollection: DOIInfoCollection = new DOIInfoCollection();