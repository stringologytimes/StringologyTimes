import { DOIInfo } from "../doi_info";
import { DOIInfoCollection } from "../doi_info";
import { DOIFilterResult } from "../doi_filter/doi_filter_result";
import { setIconToLink, setIconToSpan } from "../svg_icon";


export class DOIFilterStandardRender {
    public static getDateStr(doiInfo: DOIInfo): string {
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
    public static getSummaryInfoText(doiInfo: DOIInfo): string {
        //const dataStr = `${doiInfo.year}-${doiInfo.month <= 0 ? "?" : doiInfo.month}`;
        const containerTitle = doiInfo.container_title;
        const volumStr = doiInfo.volume;

        if (volumStr.length > 0) {
            return `${containerTitle}(Volume: ${volumStr})`;
        } else {
            return `${containerTitle}`;
        }
    }
    public static render(doiFilterResult: DOIFilterResult, doiIndex: number, doiCount: number, doiInfoCollection: DOIInfoCollection) {
        const doiIDs = new Array<number>();
        for (let i = doiIndex; i < doiIndex + doiCount; i++) {
            if (i >= doiFilterResult.doiIDs.length) {
                break;
            }
            doiIDs.push(doiFilterResult.doiIDs[i]);
        }

        const outputDiv = document.getElementById("output");
        if (!outputDiv) {
            return;
        }

        outputDiv.innerHTML = "";

        if (doiIDs.length == 0) {
            outputDiv.innerHTML = "<p>No articles found.</p>";
        } else {
            const doiInfoTemplate = document.getElementById('doi-info-template') as HTMLTemplateElement;
            const authorTemplate = document.getElementById('author-template') as HTMLTemplateElement;
            const doiReferenceTemplate = document.getElementById('doi-reference-template') as HTMLTemplateElement;

            if (!doiInfoTemplate || !authorTemplate || !doiReferenceTemplate) {
                outputDiv.innerHTML = "<p>Error: Templates not found.</p>";
                return;
            }

            //const currentDOIListPart = browserInfo.getCurrentDOIListPart();

            doiIDs.forEach((doiID, index) => {
                const doiInfo = doiInfoCollection.getDOIInfo(doiID);
                // DOIInfoテンプレートをクローン
                const doiInfoClone = doiInfoTemplate.content.cloneNode(true) as DocumentFragment;
                const article = doiInfoClone.querySelector('article');

                if (!article) return;

                // 基本情報を設定
                article.setAttribute("id", `article_${doiInfo.id}`);
                /*
                const doiSpan = article.querySelector('.doi');
                if (doiSpan) {
                    const link = document.createElement('a');
                    link.href = `https://doi.org/${encodeURIComponent(doiInfo.doi)}`;
                    link.target = '_blank';
                    link.textContent = doiInfo.doi;
                    doiSpan.textContent = '';
                    doiSpan.appendChild(link);
                }
                */

                const titleNumberSpan = article.querySelector('.title-number-text');
                if (titleNumberSpan) {
                    const ith = doiIndex + index + 1;
                    titleNumberSpan.textContent = `${ith}: `;
                } else {
                    throw new Error("titleNumberSpan is not found");
                }
                const titleSpan = article.querySelector('.title-text');
                if (titleSpan) {
                    const titleStr = doiInfo.title || '';
                    titleSpan.textContent = titleStr;
                } else {
                    throw new Error("titleSpan is not found");
                }

                const doiLink = article.querySelector('.doi-link');
                if (doiLink && doiLink instanceof HTMLAnchorElement) {
                    setIconToLink(doiLink, "DOI", `https://doi.org/${encodeURIComponent(doiInfo.doi)}`, 14, "blue", "white");
                } else {
                    throw new Error("doiLink is not found");
                }

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

                const typeIconSpan = article.querySelector('.type-icon-span');
                if (typeIconSpan && typeIconSpan instanceof HTMLSpanElement) {
                    setIconToSpan(typeIconSpan, doiInfo.type, 14, "random", "random");

                    /*

                    if (doiInfo.type == "Book") {
                        setIconToSpan(typeIconSpan, "Book", 14, "purple", "white");
                    } else if (doiInfo.type == "BookChapter") {
                        setIconToSpan(typeIconSpan, "BookChapter", 14, "purple", "white");
                    } else if (doiInfo.type == "BookSeries") {
                        setIconToSpan(typeIconSpan, "BookSeries", 14, "purple", "white");
                    } else if (doiInfo.type == "Misc") {
                        setIconToSpan(typeIconSpan, "Misc", 14, "gray", "white");
                    } else if (doiInfo.type == "Dataset") {
                        setIconToSpan(typeIconSpan, "Dataset", 14, "gray", "white");
                    } else if (doiInfo.type == "Dissertation") {
                        setIconToSpan(typeIconSpan, "Dissertation", 14, "gray", "white");
                    } else if (doiInfo.type == "Dissertation") {
                        setIconToSpan(typeIconSpan, "Dissertation", 14, "gray", "white");
                    } else if (doiInfo.type == "JournalArticle") {
                        setIconToSpan(typeIconSpan, "JournalArticle", 14, "purple", "white");
                    }
                    else if (doiInfo.type == "JournalIssue") {
                        setIconToSpan(typeIconSpan, "JournalIssue", 14, "purple", "white");
                    } else if (doiInfo.type == "Other") {
                        setIconToSpan(typeIconSpan, "Other", 14, "gray", "white");
                    } else if (doiInfo.type == "PostedContent") {
                        setIconToSpan(typeIconSpan, "PostedContent", 14, "purple", "white");
                    }
                    else if (doiInfo.type == "Preprint") {
                        setIconToSpan(typeIconSpan, "Preprint", 14, "gray", "white");
                    } else if (doiInfo.type == "Proceedings") {
                        setIconToSpan(typeIconSpan, "Proceedings", 14, "gray", "white");
                    } else if (doiInfo.type == "ProceedingsArticle") {
                        setIconToSpan(typeIconSpan, "ProceedingsArticle", 14, "purple", "white");
                    } else if (doiInfo.type == "Report") {
                        setIconToSpan(typeIconSpan, "Report", 14, "gray", "white");
                    } else if (doiInfo.type == "DataCite:ConferencePaper") {
                        setIconToSpan(typeIconSpan, "DataCite:ConferencePaper", 14, "purple", "white");
                    } else if (doiInfo.type == "Software") {
                        setIconToSpan(typeIconSpan, "Software", 14, "gray", "white");
                    } else {
                        setIconToSpan(typeIconSpan, "Unknown", 14, "gray", "white");
                    }
                    */
                } else {
                    throw new Error("typeIconSpan is not found");
                }

                const yearIconSpan = article.querySelector('.year-icon-span');
                if (yearIconSpan && yearIconSpan instanceof HTMLSpanElement) {
                    setIconToSpan(yearIconSpan, `${this.getDateStr(doiInfo)}`, 14, "brown", "white");
                } else {
                    throw new Error("yearIconSpan is not found");
                }


                /*
                const doiLink = article.querySelector('.doi-link');
                if (doiLink){
                    doiLink.setAttribute('href', `https://doi.org/${encodeURIComponent(doiInfo.doi)}`);
                }
                */
                const summaryInfoSpan = article.querySelector('.summary-info-text');
                if (summaryInfoSpan) {
                    summaryInfoSpan.textContent = this.getSummaryInfoText(doiInfo);
                } else {
                    throw new Error("summaryInfoSpan is not found");
                }

                const doiLi = article.querySelector('.doi');
                if (doiLi) {
                    doiLi.textContent = doiInfo.doi;
                } else {
                    throw new Error("doiLi is not found");
                }

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




                const containerTitleSpan = article.querySelector('.container_title');
                if (containerTitleSpan) {
                    containerTitleSpan.textContent = doiInfo.container_title || '';
                } else {
                    throw new Error("containerTitleSpan is not found");
                }

                const volumeSpan = article.querySelector('.volume');
                if (volumeSpan && volumeSpan instanceof HTMLLIElement) {
                    if (doiInfo.volume.length > 0) {
                        volumeSpan.textContent = `Volume: ${doiInfo.volume}`;
                    } else {
                        volumeSpan.style.display = 'none';
                    }
                } else {
                    throw new Error("volumeSpan is not found");
                }

                /*
                const statusSpan = article.querySelector('.status');
                if (statusSpan) statusSpan.textContent = doiInfo.status || '';
                */

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

                    // クリックイベントを追加して表示/非表示を切り替え
                    /*
                    doiReferencesDiv.addEventListener('click', (e) => {
                        const target = e.target as HTMLElement;
                        console.log("Click!");
                        // リンクやその親要素（.doi-reference）をクリックした場合は、divのクリックイベントを発火させない
                        if (target.tagName === 'A' || target.closest('.doi-reference')) {
                            return;
                        }
                        e.stopPropagation();
                        if (doiReferencesDiv.classList.contains('expanded')) {
                            doiReferencesDiv.classList.remove('expanded');
                        } else {
                            doiReferencesDiv.classList.add('expanded');
                        }
                    });
                    */
                }



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

                outputDiv.appendChild(article);
            });
        }

    }
}
