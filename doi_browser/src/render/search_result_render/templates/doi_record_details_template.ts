import { DOIRecordCollection } from "../../../doi_record_collection";
import { addIconToSpan, setIconToLink, setIconToSpan } from "../../../svg_icon";
import { DOIRecord } from "../../../doi_record";


export class DOIRecordDetailsTemplate {

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
            console.log(article.outerHTML);
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

    public static renderDOIRecordDetails(outputDiv: HTMLElement, doiID: number, doiInfoCollection: DOIRecordCollection){
        console.log("renderDOIRecordDetails/" + doiID);
        console.log(outputDiv.outerHTML);
        const doiInfo = doiInfoCollection.getDOIInfo(doiID);
        //const detailsTemplateFragment = detailesTemplateElement.content.cloneNode(true) as DocumentFragment;
        const article = outputDiv.querySelector('article');

        if (!article) return;

        /*
        const detailsDiv = article.querySelector(".details_div") as HTMLElement;
        if (detailsDiv) {
            detailsDiv.appendChild(detailsTemplateFragment);
        } else {
            console.log(article.outerHTML);
            throw new Error("details_div is not found");
        }
        console.log(detailsDiv.innerHTML);
        */



        this.renderContainerDOISpan(article, doiInfo, doiInfoCollection);
        this.renderSeriesTitleSpan(article, doiInfo, doiInfoCollection);
        this.renderDateSpan(article, doiInfo, doiInfoCollection);
        this.renderContainerTitleSpan(article, doiInfo, doiInfoCollection);
        this.renderVolumeSpan(article, doiInfo, doiInfoCollection);
        this.renderOptionalIDs(article, doiInfo, doiInfoCollection);
        this.renderAuthors(article, doiInfo, doiInfoCollection);
        this.renderDoiReferences(article, doiInfo, doiInfoCollection);
        this.renderChildrenSpan(article, doiInfo, doiInfoCollection);
        this.renderDOILi(article, doiInfo, doiInfoCollection);

    }


}
