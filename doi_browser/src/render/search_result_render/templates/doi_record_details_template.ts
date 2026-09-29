import { DOIRecordCollection } from "../../../doi_record_collection";
import { DOIRecord } from "../../../doi_record";


export class DOIRecordDetailsTemplate {
    private static renderContainerTitleSpan(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const containerTitleSpan = article.querySelector('#details-dialog-container-title');
        
        if (containerTitleSpan) {
            containerTitleSpan.innerHTML = '';
            if(doiInfo.container_DOI.length > 0){
                const parentID = doiInfoCollection.getIDByDOI(doiInfo.container_DOI);
                if(parentID != null){
                    const parentDOIInfo = doiInfoCollection.getDOIInfo(parentID);
                    const parentContainerTitle = parentDOIInfo.title;
                    const link = document.createElement('a');
                    link.href = `#`;
                    link.textContent = parentContainerTitle;
                    containerTitleSpan.appendChild(link);    
                }else if(doiInfo.container_title.length > 0){
                    containerTitleSpan.textContent = doiInfo.container_title;
                }else{
                    containerTitleSpan.textContent = "null";
                }
            }else if(doiInfo.container_title.length > 0){
                containerTitleSpan.textContent = doiInfo.container_title;
            }else{
                containerTitleSpan.textContent = "null";
            }
        } else {
            throw new Error("containerTitleSpan is not found");
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

        const dateSpan = article.querySelector('#details-dialog-date');
        if (dateSpan) {
            if (doiInfo.year >= 0) {
                if (doiInfo.month >= 1) {
                    dateSpan.textContent = `${doiInfo.year}-${doiInfo.month}`;
                } else {
                    dateSpan.textContent = `${doiInfo.year}`;
                }
            } else {
                dateSpan.textContent = `Unknown`;
            }
        } else {
            throw new Error("dateLi is not found")
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

        const optionalIdsSpan = article.querySelector('#details-dialog-optional-ids');
        if (optionalIdsSpan) {
            optionalIdsSpan.innerHTML = '';
            doiInfo.optional_ids.forEach((optionalId, index) => {
                const optionalIdSpan = document.createElement('span');
                optionalIdSpan.textContent = optionalId;
                optionalIdsSpan.appendChild(optionalIdSpan);
                if (index < doiInfo.optional_ids.length - 1) {
                    const comma = document.createTextNode(', ');
                    optionalIdsSpan.appendChild(comma);
                }
            });
        } else {
            throw new Error("optionalIdsSpan is not found");
        }
    }
    private static renderAuthors(article: HTMLElement, doiInfo: DOIRecord, doiInfoCollection: DOIRecordCollection){

        const authorsSpan = article.querySelector('#details-dialog-authors');
        if (authorsSpan) {
            authorsSpan.textContent = doiInfo.authors.join(", ");
        } else {
            throw new Error("authorsSpan is not found");
        }
        const authorTemplate = document.getElementById('author-template') as HTMLTemplateElement;

        // Authorsを設定
        if (authorsSpan && doiInfo.authors && doiInfo.authors.length > 0) {
            authorsSpan.innerHTML = '';
            doiInfo.authors.forEach((author, index) => {
                const authorClone = authorTemplate.content.cloneNode(true) as DocumentFragment;
                const authorSpan = authorClone.querySelector('.author');
                if (authorSpan) {
                    authorSpan.textContent = author;
                }
                authorsSpan.appendChild(authorClone);
                // 最後の要素以外はカンマを追加
                if (index < doiInfo.authors.length - 1) {
                    const comma = document.createTextNode(', ');
                    authorsSpan.appendChild(comma);
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

        const doiSpan = article.querySelector('#details-dialog-doi');
        if (doiSpan) {
            doiSpan.innerHTML = '';
            const aLink = document.createElement('a');
            aLink.href = `https://doi.org/${encodeURIComponent(doiInfo.doi)}`;
            aLink.target = '_blank';
            aLink.textContent = doiInfo.doi;
            doiSpan.appendChild(aLink);
        } else {
            throw new Error("doiSpan is not found");
        }
    }

    public static renderDOIRecordDetails(outputDiv: HTMLElement, doiID: number, doiInfoCollection: DOIRecordCollection){
        const doiInfo = doiInfoCollection.getDOIInfo(doiID);
        const article = outputDiv.querySelector('article');

        if (!article) return;

        const detailsDialogTitle = document.getElementById('details-dialog-title');
        if (detailsDialogTitle) {
            detailsDialogTitle.textContent = doiInfo.title;
        } else {
            throw new Error("detailsDialogTitle is not found");
        }

        this.renderDOILi(article, doiInfo, doiInfoCollection);
        this.renderDateSpan(article, doiInfo, doiInfoCollection);
        this.renderAuthors(article, doiInfo, doiInfoCollection);
        this.renderContainerTitleSpan(article, doiInfo, doiInfoCollection);
        this.renderOptionalIDs(article, doiInfo, doiInfoCollection);

    }


}
