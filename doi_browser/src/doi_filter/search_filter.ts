import { containerTypeList, paperTypeList, otherTypeList, DOIRecord } from "../doi_record";
import { DOIRecordCollection } from "../doi_record_collection";
import { DOIStatus } from "../doi_record";
import { PrimarySearchResult } from "./primary_search_result";
import { AnyContainerType, AnyPaperType, AnyOtherType } from "../doi_record";


export type SortByType = "alphabetical-order-by-container-title" | "ascending-order-by-date" | "descending-order-by-date" | "article-count" | "unordered";


export class SearchFilter {
    public minimumYear: number | null = null;
    public maximumYear: number | null = null;
    public types: string[] = [];
    public authors: string[] = [];
    public tags: string[] = [];
    //public ancestorDoi: string | null = null;

    public topContainerType: string | null = null;
    public topContainerDOI: string | null = null;
    public subContainerDOI: string | null = null;

    public doiReferences: string[] = [];    
    public excludeStatus: DOIStatus[] = [];
    public keywords: string[] = [];


    public filter(collection: DOIRecordCollection, candidates: number[]): number[] {
        return candidates.filter(candidate => {
            const doiInfo = collection.getDOIInfo(candidate);
            return this.contain(doiInfo, collection);
        });
    }



    
    public search(collection: DOIRecordCollection): number[] {
        let r: number[] = [];
        for(let i = 0; i < collection.lightweightDOIRecords.length; i++){
            const doiInfo = collection.getDOIInfo(i);
            if(this.contain(doiInfo, collection)){
                r.push(i);
            }
        }
        return r;
    }

    public is_empty(): boolean {
        return this.toURLParameters(true).length == 0;
    }

    public copy(): SearchFilter {
        const urlParameters = this.toURLParameters(true);
        return SearchFilter.buildFromURLParameters(true, urlParameters);
    }

    public getHash(is_primary_filter: boolean): string {
        const urlParameters = this.toURLParameters(is_primary_filter);
        return urlParameters.map(param => param[0] + "=" + param[1]).join("&");
    }

    public getAncestorDOI(): string | null {
        if(this.subContainerDOI != null){
            return this.subContainerDOI;
        }else if(this.topContainerDOI != null){
            return this.topContainerDOI;
        }else{
            return null;
        }

    }

    public contain(doiInfo: DOIRecord, collection: DOIRecordCollection): boolean {
        if(this.minimumYear != null && doiInfo.year < this.minimumYear){
            return false;
        }
        if(this.maximumYear != null && doiInfo.year > this.maximumYear){
            return false;
        }
        if(this.types.length > 0 && !this.types.includes(doiInfo.type)){
            return false;
        }

        const ancestorDOI = this.getAncestorDOI();
        if(ancestorDOI != null && !collection.ancestorCheck(doiInfo.id, ancestorDOI)){
            return false;
        }
        if(this.topContainerType != null && !collection.topContainerTypeCheck(doiInfo.id, this.topContainerType)){
            return false;
        }
        if(this.doiReferences.length > 0 && !doiInfo.doiReferences.every(doiReference => this.doiReferences.includes(doiReference))){
            return false;
        }

        console.log("excludeStatusX: " + this.excludeStatus.length);
        if(this.excludeStatus.length > 0){
            for(let i = 0; i < this.excludeStatus.length; i++){
                console.log("excludeStatus: " + this.excludeStatus[i] + " / " + doiInfo.getStatus());
                if(this.excludeStatus.includes(doiInfo.getStatus())){
                    return false;
                }
            }
        }

        for(let i = 0; i < this.tags.length; i++){
            if(!doiInfo.tags.includes(this.tags[i])){
                return false;
            }
        }

        if(this.keywords != null){
            const bArray = [];

            for(let i = 0; i < this.keywords.length; i++){
                var keyword = this.keywords[i];
                let b = false;


                if(keyword.indexOf("@DOI:") == 0){
                    const doiKeyword = keyword.substring(5);
                    if(doiKeyword.length > 0){
                        var fstChar = doiKeyword.charAt(0);
                        if(fstChar == "="){
                            var regexPattern = doiKeyword.substring(1);
                            var regex = new RegExp(regexPattern);
                            if(regex.test(doiInfo.doi)){
                                b = true;
                            }
                        }else{
                            if(doiInfo.doi == doiKeyword){
                                b = true;
                            }        
                        }

                    }else{
                        b = true;
                    }


                }
                else if(keyword.indexOf("@CONTAINER_DOI:") == 0){
                    const containerDOIKeyword = keyword.substring(15);
                    if(doiInfo.container_DOI == containerDOIKeyword){                    
                        b = true;
                    }else if(containerDOIKeyword == "null" && doiInfo.container_DOI == ""){
                        b = true;
                    }
                }
                else if(keyword.indexOf("@CONTAINER_TITLE:") == 0){
                    const containerTitleKeyword = keyword.substring(17);
                    if(doiInfo.container_title == containerTitleKeyword){                    
                        b = true;
                    }else if(containerTitleKeyword == "null" && doiInfo.container_title == ""){
                        b = true;
                    }
                }
                else{
                    if(doiInfo.title.indexOf(keyword) != -1){
                        b = true;
                    }
                    if(doiInfo.doi.indexOf(keyword) != -1){
                        b = true;
                    }
                }
                bArray.push(b);
            }

            if(!bArray.every(b => b)){
                return false;
            }
        }
        return true;
    }

    public toURLParameters(is_primary_filter: boolean): [string, string][] {
        const prefix = is_primary_filter ? "psf-" : "ssf-";
        const r: [string, string][] = [];

        if(this.minimumYear != null){
            r.push([prefix + "minimum-year", this.minimumYear.toString()]);
        }
        if(this.maximumYear != null){
            r.push([prefix + "maximum-year", this.maximumYear.toString()]);
        }
        if(this.types.length > 0){
            const containContainerTypeAll = containerTypeList.every(type => this.types.includes(type));
            const containPaperTypeAll = paperTypeList.every(type => this.types.includes(type));
            const containOtherTypeAll = otherTypeList.every(type => this.types.includes(type));


            if(containContainerTypeAll && containPaperTypeAll && containOtherTypeAll){
                /*
                containerTypeList.concat(paperTypeList).concat(otherTypeList).forEach(type => {
                    r.push([prefix + "type", type]);
                });        
                */
            }else{
                if(containContainerTypeAll){
                    r.push([prefix + "type", AnyContainerType]);
                }else{
                    this.types.filter(type => containerTypeList.includes(type)).forEach(type => {
                        r.push([prefix + "type", type]);
                    });
                }
    
                if(containPaperTypeAll){
                    r.push([prefix + "type", AnyPaperType]);
                }else{
                    this.types.filter(type => paperTypeList.includes(type)).forEach(type => {
                        r.push([prefix + "type", type]);
                    });
                }

                if(containOtherTypeAll){
                    r.push([prefix + "type", AnyOtherType]);
                }else{
                    this.types.filter(type => otherTypeList.includes(type)).forEach(type => {
                        r.push([prefix + "type", type]);
                    });
                }    
            }
        }else{
            r.push([prefix + "type", "Empty"]);
        }
        if(this.authors.length > 0){
            this.authors.forEach(author => {
                r.push([prefix + "author", author]);
            });
        }
        if(this.tags.length > 0){
            this.tags.forEach(tag => {
                r.push([prefix + "tag", tag]);
            });
        }
        if(this.subContainerDOI != null){
            r.push([prefix + "sub-container-doi", this.subContainerDOI]);
        }
        if(this.topContainerDOI != null){
            r.push([prefix + "top-container-doi", this.topContainerDOI]);
        }
        if(this.topContainerType != null){
            r.push([prefix + "top-container-type", this.topContainerType]);
        }
        if(this.doiReferences.length > 0){
            this.doiReferences.forEach(doiReference => {
                r.push([prefix + "doi-reference", doiReference]);
            });
        }
        if(this.excludeStatus.length > 0){
            this.excludeStatus.forEach(excludeStatus => {
                r.push([prefix + "excluded-status", excludeStatus]);
            });
        }
        if(this.keywords.length > 0){
            this.keywords.forEach(keyword => {
                r.push([prefix + "keyword", keyword]);
            });
        }

        r.sort((a, b) => {
            const aKey = a[0];
            const bKey = b[0];
            if(aKey == bKey){
                const aValue = a[1];
                const bValue = b[1];
                if(aValue == bValue){
                    return 0;
                }else{
                    return aValue.localeCompare(bValue);
                }
            }else{
                return aKey.localeCompare(bKey);
            }
        } );


        return r;
    }

    public static buildFromURLParameters(is_primary_filter: boolean, urlParameters: [string, string][]): SearchFilter {
        const prefix = is_primary_filter ? "psf-" : "ssf-";
        const r = new SearchFilter();
        let typeCounter = 0;

        for(let i = 0; i < urlParameters.length; i++){
            const key = urlParameters[i][0];
            const value = urlParameters[i][1];
            if(key == prefix + "minimum-year"){
                r.minimumYear = parseInt(value);
            }else if(key == prefix + "maximum-year"){
                r.maximumYear = parseInt(value);
            }else if(key == prefix + "type"){
                typeCounter++;
                if(value == AnyContainerType){
                    containerTypeList.forEach(type => {
                        r.types.push(type);
                    });
                }else if(value == AnyPaperType){
                    paperTypeList.forEach(type => {
                        r.types.push(type);
                    });
                }else if(value == AnyOtherType){
                    otherTypeList.forEach(type => {
                        r.types.push(type);
                    });
                }else if(value != "Empty"){
                    r.types.push(value);
                }
            }else if(key == prefix + "author"){
                r.authors.push(value);
            }else if(key == prefix + "tag"){
                r.tags.push(value);
            }else if(key == prefix + "top-container-doi"){
                r.topContainerDOI = value;
            }else if(key == prefix + "sub-container-doi"){
                r.subContainerDOI = value;
            }else if(key == prefix + "top-container-type"){
                r.topContainerType = value;
            }else if(key == prefix + "doi-reference"){
                r.doiReferences.push(value);
            }else if(key == prefix + "excluded-status"){
                r.excludeStatus.push(value as DOIStatus);
            }else if(key == prefix + "keyword"){
                r.keywords.push(value);
            }            
        }

        if(typeCounter == 0){
            containerTypeList.concat(paperTypeList).concat(otherTypeList).forEach(type => {
                r.types.push(type);
            });
        }



        return r;
    }

}
