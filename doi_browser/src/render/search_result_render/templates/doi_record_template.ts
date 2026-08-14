import { DOIRecordCollection } from "../../../doi_record_collection";
import { addIconToSpan, setIconToLink, setIconToSpan } from "../../../svg_icon";
import { DOIRecord } from "../../../doi_record";


export class DOIRecordTemplate {
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
        const containerTitle = doiInfo.container_title;
        const volumStr = doiInfo.volume_issue;
        const seriesTitle = doiInfo.seriesTitle;




        if(doiInfo.isContainerType()){
            const containerTypeChildrenCount = doiInfoCollection.getContainerTypeChildrenCount(doiInfo.id);
            const primaryDescendantCount = doiInfoCollection.getPrimaryDescendantCount(doiInfo.id);
            const secondaryDescendantCount = doiInfoCollection.getSecondaryDescendantCount(doiInfo.id);
            return `${seriesTitle}(${containerTypeChildrenCount} containers, ${primaryDescendantCount} primary records, ${secondaryDescendantCount} secondary records)`;
        }else{
            if(seriesTitle.length > 0){
                return `${seriesTitle}(${containerTitle})`;
            }else{
                return `${containerTitle}`;
            }
    
        }


    }

    private static renderOptionalIconSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){
        const optionalIconSpan = article.querySelector('.optional-icon-span');
        if (optionalIconSpan && optionalIconSpan instanceof HTMLSpanElement) {
            optionalIconSpan.replaceChildren();
            var containerDOI = doiInfo.container_DOI;
            var containerDOIID = doiInfoCollection.getIDByDOI(containerDOI);

            if(containerDOI.length > 0 && containerDOIID == null){
                var iconText = "InvalidContainerDOI";
                addIconToSpan(optionalIconSpan, iconText, 14, "red", "white");
            }
            if(doiInfo.container_DOI.length == 0){
                var iconText = "NoContainerDOI";
                addIconToSpan(optionalIconSpan, iconText, 14, "gray", "white");
            }

            if(doiInfo.doi == containerDOI){
                var iconText = "SelfContainerDOI";
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

    private static renderTitleText(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){
        const titleSpan = article.querySelector('.title-text');
        if (titleSpan) {
            const titleStr = doiInfo.title || '';
            titleSpan.textContent = titleStr;
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
    private static renderContainerDOISpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const containerDOISpan = article.querySelector('.container_DOI');
        if (containerDOISpan) {
            const labelSpan = document.createElement('span');
            labelSpan.textContent = "Container DOI: ";
            containerDOISpan.appendChild(labelSpan);

            if(doiInfo.container_DOI.length > 0){
                const link = document.createElement('a');
                link.href = `#`;
                link.textContent = doiInfo.container_DOI;
                link.addEventListener("click", (event) => {
                    event.preventDefault();
                    (window as any).initializeParameter([["keyword", `@DOI:${doiInfo.container_DOI}`]]);
                });
                containerDOISpan.appendChild(link);    
            }else{
                const labelSpan = document.createElement('span');
                labelSpan.textContent = "null";
                containerDOISpan.appendChild(labelSpan);

            }
        } else {
            throw new Error("containerDOISpan is not found");
        }

    }
    private static renderSeriesTitleSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const seriesTitleSpan = article.querySelector('.series_title');
        if (seriesTitleSpan) {
            seriesTitleSpan.textContent = `Series Title: ${doiInfo.seriesTitle}`;
        } else {
            throw new Error("seriesTitleSpan is not found");
        }

    }
    private static renderDateSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const dateLi = article.querySelector('.date');
        if (dateLi) {
            if (doiInfo.year >= 0) {
                if (doiInfo.month >= 0) {
                    dateLi.textContent = `Date: ${doiInfo.year}-${doiInfo.month}`;
                } else {
                    dateLi.textContent = `Date: s${doiInfo.year}`;
                }
            } else {
                dateLi.textContent = `Date: Unknown`;
            }
        } else {
            throw new Error("dateLi is not found")
        }
    }

    private static renderContainerTitleSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const containerTitleSpan = article.querySelector('.container_title');
        if (containerTitleSpan) {
            containerTitleSpan.textContent = "Container Title: " + (doiInfo.container_title || '');
        } else {
            throw new Error("containerTitleSpan is not found");
        }
    }

    private static renderVolumeSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const volumeSpan = article.querySelector('.volume');
        if (volumeSpan && volumeSpan instanceof HTMLLIElement) {
            if (doiInfo.volume_issue.length > 0) {
                volumeSpan.textContent = `Volume: ${doiInfo.volume_issue}`;
            } else {
                volumeSpan.style.display = 'none';
            }
        } else {
            throw new Error("volumeSpan is not found");
        }
    }
    private static renderOptionalIDs(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const optionalIdsSpan = article.querySelector('.optional_ids');
        if (optionalIdsSpan) {
            optionalIdsSpan.textContent = "Optional IDs: " + doiInfo.optional_ids.join(", ");
        } else {
            throw new Error("optionalIdsSpan is not found");
        }
    }
    private static renderAuthors(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){
        const authorTemplate = document.getElementById('author-template') as HTMLTemplateElement;

        // Authorsを設定
        const authorsDiv = article.querySelector('.authors');
        if (authorsDiv && doiInfo.authors && doiInfo.authors.length > 0) {
            authorsDiv.innerHTML = '';
            doiInfo.authors.forEach((author, index) => {
                const authorClone = authorTemplate.content.cloneNode(true) as DocumentFragment;
                const authorSpan = authorClone.querySelector('.author');
                if (authorSpan) {
                    authorSpan.textContent = author;
                }
                authorsDiv.appendChild(authorClone);
                // 最後の要素以外はカンマを追加
                if (index < doiInfo.authors.length - 1) {
                    const comma = document.createTextNode(', ');
                    authorsDiv.appendChild(comma);
                }
            });
        }
    }
    private static renderDoiReferences(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const doiReferenceTemplate = document.getElementById('doi-reference-template') as HTMLTemplateElement;
        // DOI Referencesを設定
        const doiReferencesDiv = article.querySelector('.doi_references');
        if (doiReferencesDiv && doiInfo.doiReferences && doiInfo.doiReferences.length > 0) {
            doiReferencesDiv.innerHTML = '';
            doiInfo.doiReferences.forEach((doiRef, index) => {
                const doiRefClone = doiReferenceTemplate.content.cloneNode(true) as DocumentFragment;
                const doiRefSpan = doiRefClone.querySelector('.doi-reference');
                if (doiRefSpan) {
                    const link = document.createElement('a');
                    link.href = `https://doi.org/${encodeURIComponent(doiRef)}`;
                    link.target = '_blank';
                    link.textContent = doiRef;
                    doiRefSpan.appendChild(link);
                }
                doiReferencesDiv.appendChild(doiRefClone);
                // 最後の要素以外は改行を追加
                if (index < doiInfo.doiReferences.length - 1) {
                    const br = document.createElement('br');
                    doiReferencesDiv.appendChild(br);
                }
            });

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
    private static renderChildrenSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const childrenSpan = article.querySelector('.children');
        if (childrenSpan) {
            if(doiInfo.type == "Book" || doiInfo.type == "ConferenceProceeding" || doiInfo.type == "ProceedingsCollection" || doiInfo.type == "Journal-Issue" || doiInfo.type == "ReferenceBook" || doiInfo.type == "Monograph"){
                const link = document.createElement('a');
                link.href = `#`;
                link.textContent = "Articles";
                link.addEventListener("click", (event) => {
                    event.preventDefault();
                    (window as any).initializeParameter([["keyword", `@CONTAINER_DOI:${doiInfo.doi}`]]);
                });
                childrenSpan.appendChild(link);    
            }else{
                childrenSpan.innerHTML = '';
            }
        } else {
            throw new Error("childrenSpan is not found");
        }
    }
    private static renderDOILi(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const doiLi = article.querySelector('.doi');
        if (doiLi) {
            doiLi.textContent = doiInfo.doi;
        } else {
            throw new Error("doiLi is not found");
        }
    }


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

        const detailsDiv = article.querySelector(".details_div");
        if (detailsDiv) {
            detailsDiv.appendChild(detailsDivClone);
        } else {
            throw new Error("detailsDiv is not found");
        }

        // 基本情報を設定
        article.setAttribute("id", `article_${doiInfo.id}`);


        this.renderTitleNumberSpan(article, doiInfo, doiInfoCollection);
        this.renderTitleText(article, doiInfo, doiInfoCollection);
        this.renderDOILink(article, doiInfo, doiInfoCollection);
        this.renderStatusIconSpan(article, doiInfo, doiInfoCollection);
        this.renderTypeIconSpan(article, doiInfo, doiInfoCollection);
        this.renderYearIconSpan(article, doiInfo, doiInfoCollection);
        this.renderOptionalIconSpan(article, doiInfo, doiInfoCollection);
        this.renderSummaryInfoText(article, doiInfo, doiInfoCollection);
        this.renderDOILi(article, doiInfo, doiInfoCollection);

        this.renderContainerDOISpan(article, doiInfo, doiInfoCollection);
        this.renderSeriesTitleSpan(article, doiInfo, doiInfoCollection);
        this.renderDateSpan(article, doiInfo, doiInfoCollection);
        this.renderContainerTitleSpan(article, doiInfo, doiInfoCollection);
        this.renderVolumeSpan(article, doiInfo, doiInfoCollection);
        this.renderOptionalIDs(article, doiInfo, doiInfoCollection);
        this.renderAuthors(article, doiInfo, doiInfoCollection);
        this.renderDoiReferences(article, doiInfo, doiInfoCollection);
        this.renderTags(article, doiInfo, doiInfoCollection);
        this.renderChildrenSpan(article, doiInfo, doiInfoCollection);
        



        outputDiv.appendChild(article);
    }
}