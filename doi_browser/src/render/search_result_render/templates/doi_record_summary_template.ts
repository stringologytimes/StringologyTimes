import { DOIRecordCollection } from "../../../doi_record_collection";
import { addIconToSpan, setIconToLink, setIconToSpan } from "../../../svg_icon";
import { DOIRecord } from "../../../doi_record";
import { DOIRecordDetailsTemplate } from "./doi_record_details_template";

export class DOIRecordSummaryTemplate {
    private static getDateStr(doiInfo: DOIRecord): string {
        const yearStr = doiInfo.year <= 0 ? "?" : doiInfo.year.toString();
        let monthStr = "?";
        if (doiInfo.month > 0 && doiInfo.month < 10) {
            monthStr = `0${doiInfo.month}`;
        } else if (doiInfo.month >= 10) {
            monthStr = doiInfo.month.toString();
        }
        const dataStr = `${yearStr}-${monthStr}`;
        return dataStr;
    }
    private static getSummaryInfoText(doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection): string {
        //const dataStr = `${doiInfo.year}-${doiInfo.month <= 0 ? "?" : doiInfo.month}`;
        let containerTitle = doiInfo.container_title;

        if(doiInfo.container_DOI.length > 0){
            const containerID = doiInfoCollection.getIDByDOI(doiInfo.container_DOI);
            if(containerID != null){
                const containerDOIInfo = doiInfoCollection.getDOIInfo(containerID);
                containerTitle = containerDOIInfo.title;
            }
        }


        //const volumStr = doiInfo.volume_issue;
        //const seriesTitle = doiInfo.seriesTitle;




        if(doiInfo.isContainerType()){
            const containerTypeChildrenCount = doiInfoCollection.getContainerTypeChildrenCount(doiInfo.id);
            const primaryDescendantCount = doiInfoCollection.getPrimaryDescendantCount(doiInfo.id);
            const secondaryDescendantCount = doiInfoCollection.getSecondaryDescendantCount(doiInfo.id);
            return `${containerTitle}(${containerTypeChildrenCount} containers, ${primaryDescendantCount} primary records, ${secondaryDescendantCount} secondary records)`;
        }else{
            if(containerTitle.length > 0){
                return `${containerTitle}`;
            }else{
                return `Unknown container`;
            }
    
        }


    }

    private static renderOptionalIconSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){
        const optionalIconSpan = article.querySelector('.optional-icon-span');
        if (optionalIconSpan && optionalIconSpan instanceof HTMLSpanElement) {
            optionalIconSpan.replaceChildren();
            var containerDOI = doiInfo.container_DOI;
            var containerDOIID = doiInfoCollection.getIDByDOI(containerDOI);

            if(doiInfo.isPrimary && containerDOI.length > 0 && containerDOIID == null){
                var iconText = "Container is not found (InvalidRegistrationDataError)";
                addIconToSpan(optionalIconSpan, iconText, 14, "red", "white");
            }
            if(doiInfo.container_DOI.length == 0){
                var iconText = "No container";
                addIconToSpan(optionalIconSpan, iconText, 14, "gray", "white");
            }

            if(doiInfo.doi == containerDOI){
                var iconText = "Self-container (InvalidRegistrationDataError)";
                addIconToSpan(optionalIconSpan, iconText, 14, "green", "white");
            }
        } else {
            throw new Error("optionalIconSpan is not found");
        }
    }

    private static renderYearIconSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){
        const yearIconSpan = article.querySelector('.year-icon-span');
        if (yearIconSpan && yearIconSpan instanceof HTMLSpanElement) {
            setIconToSpan(yearIconSpan, `${this.getDateStr(doiInfo)}`, 14, "brown", "white");
        } else {
            throw new Error("yearIconSpan is not found");
        }
    }

    private static renderTypeIconSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){
        const typeIconSpan = article.querySelector('.type-icon-span');
        if (typeIconSpan && typeIconSpan instanceof HTMLSpanElement) {
            setIconToSpan(typeIconSpan, doiInfo.type, 14, "random", "random");
        } else {
            throw new Error("typeIconSpan is not found");
        }
    }
    private static renderStatusIconSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const statusIconSpan = article.querySelector('.status-icon-span');
        if (statusIconSpan && statusIconSpan instanceof HTMLSpanElement) {
            if (doiInfo.isPrimary) {
                setIconToSpan(statusIconSpan, "Primary", 14, "green", "white");
            } else {
                setIconToSpan(statusIconSpan, "Secondary", 14, "gray", "white");
            }
        } else {
            throw new Error("statusIconSpan is not found");
        }
    }
    private static renderDOILink(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){
        const doiLink = article.querySelector('.doi-link');
        if (doiLink && doiLink instanceof HTMLAnchorElement) {
            setIconToLink(doiLink, "DOI", `https://doi.org/${encodeURIComponent(doiInfo.doi)}`, 14, "blue", "white");
        } else {
            throw new Error("doiLink is not found");
        }
    }

    private static renderTitleText(article: HTMLElement, doiInfo: DOIRecord, doiID: number, doiInfoCollection: DOIRecordCollection){
        const titleSpan = article.querySelector('.title-text');
        if (titleSpan) {
            const titleStr = doiInfo.title || '';
            titleSpan.textContent = titleStr;
            titleSpan.setAttribute("data-doi-id", doiID.toString());
        } else {
            throw new Error("titleSpan is not found");
        }
    }
    private static renderSummaryInfoText(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const summaryInfoSpan = article.querySelector('.summary-info-text');
        if (summaryInfoSpan) {
            summaryInfoSpan.textContent = this.getSummaryInfoText(doiInfo, doiInfoCollection);
        } else {
            throw new Error("summaryInfoSpan is not found");
        }
    }

    private static renderTitleNumberSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const titleNumberSpan = article.querySelector('.title-number-text');
        if (titleNumberSpan) {
            titleNumberSpan.textContent = ``;
        } else {
            throw new Error("titleNumberSpan is not found");
        }
    }

    private static renderTags(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const tagsSpan = article.querySelector('.tags-text');
        if (tagsSpan) {
            doiInfo.tags.forEach((tag, index) => {
                const tagSpan = document.createElement('span');
                setIconToSpan(tagSpan, tag, 14, "random", "random");
                tagsSpan.appendChild(tagSpan);
            });
        } else {
            throw new Error("tagsSpan is not found");
        }
    }
    public static renderDOIRecordSummary(outputDiv: HTMLElement, doiID: number, doiInfoCollection: DOIRecordCollection){
        const article = outputDiv.querySelector('article');
        if (!article) return;
        const doiInfo = doiInfoCollection.getDOIInfo(doiID);
        this.renderTitleNumberSpan(article, doiInfo, doiInfoCollection);
        this.renderTitleText(article, doiInfo, doiID, doiInfoCollection);
        this.renderDOILink(article, doiInfo, doiInfoCollection);
        this.renderStatusIconSpan(article, doiInfo, doiInfoCollection);
        this.renderTypeIconSpan(article, doiInfo, doiInfoCollection);
        this.renderYearIconSpan(article, doiInfo, doiInfoCollection);
        this.renderOptionalIconSpan(article, doiInfo, doiInfoCollection);
        this.renderSummaryInfoText(article, doiInfo, doiInfoCollection);
        this.renderTags(article, doiInfo, doiInfoCollection);

    }

    public static setArticleTemplate(outputDiv: HTMLElement, doiInfoTemplate: HTMLTemplateElement, openDetail : boolean){
        const doiInfoClone = doiInfoTemplate.content.cloneNode(true) as DocumentFragment;
        const article = doiInfoClone.querySelector('article');
        if (!article) return;
        if (openDetail) {
            const details = article.querySelector('details');
            if (details) {
                details.open = true;
            }
        }
        outputDiv.appendChild(article);
    }

    /*

    public static renderDOIRecord(outputDiv: HTMLElement, doiID: number, doiInfoCollection: DOIRecordCollection){
        const doiInfoTemplate = document.getElementById('doi-record-template') as HTMLTemplateElement;
        const detailsDivTemplate = document.getElementById('details-div-standard-template') as HTMLTemplateElement;

        if (!doiInfoTemplate || !detailsDivTemplate) {
            outputDiv.innerHTML = "<p>Error: Templates not found.</p>";
            return;
        }

        const doiInfo = doiInfoCollection.getDOIInfo(doiID);
        // DOIInfoテンプレートをクローン
        const doiInfoClone = doiInfoTemplate.content.cloneNode(true) as DocumentFragment;
        const detailsDivClone = detailsDivTemplate.content.cloneNode(true) as DocumentFragment;
        const article = doiInfoClone.querySelector('article');

        if (!article) return;

        const detailsDiv = article.querySelector(".details_div") as HTMLElement;
        if (detailsDiv) {
            detailsDiv.appendChild(detailsDivClone);
        } else {
            throw new Error("detailsDiv is not found");
        }

        // 基本情報を設定
        article.setAttribute("id", `article_${doiInfo.id}`);

        this.renderDOIRecordSummary(article, doiID, doiInfoCollection);
        DOIRecordDetailsTemplate.renderDOIRecordDetails(detailsDiv, doiID, doiInfoCollection);

        



        outputDiv.appendChild(article);
    }
    */
}