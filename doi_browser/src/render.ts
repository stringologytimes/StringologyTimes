import {  DOIInfo } from "./browser";
import { BrowserInfo } from "./browser_info";


export class Render {
    public static render(browserInfo: BrowserInfo) {
        const outputDiv = document.getElementById("output");
        if (!outputDiv) {
            return;
        }

        outputDiv.innerHTML = "";

        if (browserInfo.doiInfoCollection == null) {
            outputDiv.innerHTML = "<p>Loading...</p>";
        } else if (browserInfo.foundDOIList == null) {
            outputDiv.innerHTML = "<p>No articles found.</p>";
        } else if (browserInfo.foundDOIList.length === 0) {
            outputDiv.innerHTML = "<p>No articles found.</p>";
        } else {
            const doiInfoTemplate = document.getElementById('doi-info-template') as HTMLTemplateElement;
            const authorTemplate = document.getElementById('author-template') as HTMLTemplateElement;
            const doiReferenceTemplate = document.getElementById('doi-reference-template') as HTMLTemplateElement;

            if (!doiInfoTemplate || !authorTemplate || !doiReferenceTemplate) {
                outputDiv.innerHTML = "<p>Error: Templates not found.</p>";
                return;
            }

            const currentDOIListPart = browserInfo.getCurrentDOIListPart();

            currentDOIListPart.forEach((doiInfo, index) => {
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

                const titleSpan = article.querySelector('.title');
                if (titleSpan){
                    const ith = (browserInfo.pageNumber * browserInfo.pageSize) + index + 1;
                    const titleStr = doiInfo.title || '';
                    const viewStr = `${ith}: ${titleStr}`;
                    titleSpan.textContent = viewStr;
                }

                const dateLi = article.querySelector('.date');
                if(dateLi){
                    if(doiInfo.year >= 0){
                        if(doiInfo.month >= 0){
                            dateLi.textContent = `Date: ${doiInfo.year}-${doiInfo.month}`;
                        }else{
                            dateLi.textContent = `Date: s${doiInfo.year}`;
                        }
                    }else{
                        dateLi.textContent = `Date: Unknown`;
                    }    
                }


                const containerTitleSpan = article.querySelector('.container_title');
                if (containerTitleSpan) containerTitleSpan.textContent = doiInfo.container_title || '';

                const volumeSpan = article.querySelector('.volume');
                if (volumeSpan) volumeSpan.textContent = doiInfo.volume || '';

                const statusSpan = article.querySelector('.status');
                if (statusSpan) statusSpan.textContent = doiInfo.status || '';

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

                outputDiv.appendChild(article);
            });
        }

    }
    public static setFoundDOIList(browserInfo: BrowserInfo, list: DOIInfo[]) {
        browserInfo.foundDOIList = list;
        this.render(browserInfo);
    }
    public static renderAll(browserInfo: BrowserInfo){
        browserInfo.foundDOIList = [];
        browserInfo.pageNumber = 0; // 最初のページにリセット
        const maxSize = browserInfo.doiInfoCollection!.length();
        for(let i = 0; i < maxSize; i ++){
            browserInfo.foundDOIList.push(browserInfo.doiInfoCollection!.getDOIInfo(i));
        }
        this.render(browserInfo);
    }
    public static getArticleElementByChild(e : HTMLElement) : HTMLElement | null{
        if(e.tagName == "ARTICLE"){
            return e;
        }else{
            if(e.parentElement){
                return this.getArticleElementByChild(e.parentElement);
            }else{
                return null;
            }
        }

    }
}
