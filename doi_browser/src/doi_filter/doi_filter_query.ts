import { DOIRecord } from "../doi_record";
import { DOIInfoCollection } from "../doi_record_collection";
import { DOIStatus } from "../doi_record";
import { DOIFilterResult } from "./doi_filter_result";

export type SortByType = "alphabetical-order-by-container-title" | "ascending-order-by-date" | "descending-order-by-date" | "article-count" | "unordered";


export class DOIFilterQuery {
    public minimum_year: number | null = null;
    public maximum_year: number | null = null;
    public type: string | null = null;
    public authors: string[] = [];
    public tags: string[] = [];
    public volume: string | null = null;
    public container_title: string | null = null;
    public series_title: string | null = null;
    public doiReferences: string[] = [];    
    public excludeStatus: DOIStatus[] = [];
    public sortBy: SortByType = "unordered";
    public keywords: string[] = [];

    /*

    public static buildFromURLParameters(): DOIFilterQuery {
        let r = new DOIFilterQuery();
        const sp = new URL(location.href).searchParams;

        for (const [k, v] of sp.entries()) {
            if(k == "minimum_year"){
                r.minimum_year = parseInt(v);
            }
            else if(k == "maximum_year"){
                r.maximum_year = parseInt(v);
            }
            else if(k == "author"){
                r.authors.push(v);
            }
            else if(k == "tag"){
                r.tags.push(v);
            }else if(k == "type"){
                r.type = v;
            }else if(k == "volume"){
                r.volume = v;
            }else if(k == "container_title"){
                r.container_title = v;
            }else if(k == "doi_reference"){
                r.doiReferences.push(v);
            }else if(k == "tag1"){
                r.tags.push(v);
            }else if(k == "tag2"){
                r.tags.push(v);
            }else if(k == "tag3"){
                r.tags.push(v);
            }
        }
        return r;
    }
    */

    
    private filter(collection: DOIInfoCollection, candidates: number[]): number[] {
        return candidates.filter(candidate => {
            const doiInfo = collection.getDOIInfo(candidate);
            return this.contain(doiInfo);
        });
    }

    public search(doiInfoCollectionFilter: DOIFilterResult, collection: DOIInfoCollection): number[] {
        let r: number[] = doiInfoCollectionFilter.doiIDs.map(doiID => doiID);
        return this.filter(collection, r);
    }
    public is_empty(): boolean {
        return this.minimum_year == null && this.maximum_year == null && this.type == null && 
        this.authors.length == 0 && this.tags.length == 0 && this.volume == null && this.container_title == null 
        && this.keywords.length == 0 && this.series_title == null
        && this.excludeStatus.length == 0 && this.doiReferences.length == 0;
    }
    public copy(): DOIFilterQuery {
        const r = new DOIFilterQuery();
        r.minimum_year = this.minimum_year;
        r.maximum_year = this.maximum_year;
        r.type = this.type;
        r.authors = this.authors.map(author => author);
        r.tags = this.tags.map(tag => tag);
        r.volume = this.volume;
        r.container_title = this.container_title;
        r.series_title = this.series_title;
        r.doiReferences = this.doiReferences.map(doiReference => doiReference);
        r.excludeStatus = this.excludeStatus.map(excludeStatus => excludeStatus);
        r.sortBy = this.sortBy;
        r.keywords = this.keywords.map(keyword => keyword);
        return r;
    }

    public get_parents() : DOIFilterQuery[] {
        var r = new Array<DOIFilterQuery>();
        if(this.minimum_year != null){
            var copy = this.copy();
            copy.minimum_year = null;
            r.push(copy);
        }
        if(this.maximum_year != null){
            var copy = this.copy();
            copy.maximum_year = null;
            r.push(copy);
        }
        if(this.type != null){
            var copy = this.copy();
            copy.type = null;
            r.push(copy);
        }

        if(this.authors.length > 0){
            var copy = this.copy();
            copy.authors = [];
            r.push(copy);
        }
        if(this.tags.length > 0){
            var copy = this.copy();
            copy.tags = [];
            r.push(copy);
        }

        if(this.volume != null){
            var copy = this.copy();
            copy.volume = null;
            r.push(copy);
        }
        if(this.container_title != null){
            var copy = this.copy();
            copy.container_title = null;
            r.push(copy);
        }
        if(this.series_title != null){
            var copy = this.copy();
            copy.series_title = null;
            r.push(copy);
        }
        if(this.doiReferences.length > 0){
            var copy = this.copy();
            copy.doiReferences = [];
            r.push(copy);
        }
        if(this.excludeStatus.length > 0){
            var copy = this.copy();
            copy.excludeStatus = [];
            r.push(copy);
        }
        if(this.sortBy != "unordered"){
            var copy = this.copy();
            copy.sortBy = "unordered";
            r.push(copy);
        }
        if(this.keywords.length > 0){
            var copy = this.copy();
            copy.keywords = [];
            r.push(copy);
        }
        return r;
    }

    public getHash(): string {
        var obj: any = {};

        if(this.minimum_year != null){
            obj.minimum_year = this.minimum_year;
        }
        if(this.maximum_year != null){
            obj.maximum_year = this.maximum_year;
        }
        if(this.type != null){
            obj.type = this.type;
        }
        if(this.authors.length > 0){
            obj.authors = this.authors;
        }
        if(this.tags.length > 0){
            obj.tags = this.tags;
        }
        if(this.volume != null){
            obj.volume = this.volume;
        }
        if(this.container_title != null){
            obj.container_title = this.container_title;
        }
        if(this.series_title != null){
            obj.series_title = this.series_title;
        }
        if(this.doiReferences.length > 0){
            obj.doiReferences = this.doiReferences;
        }
        if(this.excludeStatus.length > 0){
            obj.excludeStatus = this.excludeStatus;
        }
        if(this.sortBy != "unordered"){
            obj.sortBy = this.sortBy;
        }
        if(this.keywords.length > 0){
            obj.keywords = this.keywords;
        }

        return JSON.stringify(obj);
    }

    public static buildFromJSON(json: string): DOIFilterQuery {
        var obj: any = JSON.parse(json);
        var r = new DOIFilterQuery();

        if(obj.minimum_year != null){
            r.minimum_year = obj.minimum_year;
        }
        if(obj.maximum_year != null){
            r.maximum_year = obj.maximum_year;
        }
        if(obj.type != null){
            r.type = obj.type;
        }
        if(obj.authors.length > 0){
            r.authors = obj.authors.map((v: any) => v as string);
        }
        if(obj.tags.length > 0){
            r.tags = obj.tags.map((v: any) => v as string);
        }
        if(obj.volume != null){
            r.volume = obj.volume;
        }
        if(obj.container_title != null){
            r.container_title = obj.container_title;
        }
        if(obj.series_title != null){
            r.series_title = obj.series_title;
        }
        if(obj.doiReferences.length > 0){
            r.doiReferences = obj.doiReferences.map((v: any) => v as string);
        }
        if(obj.excludeStatus.length > 0){
            r.excludeStatus = obj.excludeStatus.map((v: any) => v as DOIStatus);
        }
        if(obj.sortBy != "unordered"){
            r.sortBy = obj.sortBy;
        }
        if(obj.keywords.length > 0){
            r.keywords = obj.keywords.map((v: any) => v as string);
        }
        return r;
    }
    public contain(doiInfo: DOIRecord): boolean {
        if(this.minimum_year != null && doiInfo.year < this.minimum_year){
            return false;
        }
        if(this.maximum_year != null && doiInfo.year > this.maximum_year){
            return false;
        }
        if(this.type != null && doiInfo.type != this.type){
            return false;
        }
        if(this.container_title != null && doiInfo.container_title != this.container_title){
            return false;
        }
        if(this.series_title != null && doiInfo.seriesTitle != this.series_title){
            return false;
        }


        if(this.doiReferences.length > 0 && !doiInfo.doiReferences.every(doiReference => this.doiReferences.includes(doiReference))){
            return false;
        }
        if(this.excludeStatus.length > 0){
            for(let i = 0; i < this.excludeStatus.length; i++){
                if(doiInfo.getStatus() == this.excludeStatus[i]){
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

    public isIncluded(item : DOIFilterQuery): boolean {
        if(this.minimum_year != null && item.minimum_year != null){            
            if(this.minimum_year < item.minimum_year){
                return false;
            }
        }
        if(this.maximum_year != null && item.maximum_year != null){
            if(this.maximum_year > item.maximum_year){
                return false;
            }
        }
        if(item.type != null){
            if(this.type == null){
                return false;
            }
            else{
                if(this.type != item.type){
                    return false;
                }
            }
        }
        if(this.authors.length > 0){
            return false;
        }
        if(this.tags.length > 0){
            return false;
        }
        if(this.volume != null && item.volume != null){
            if(this.volume != item.volume){
                return false;
            }
        }
        if(item.excludeStatus.length > 0){
            for(let i = 0; i < item.excludeStatus.length; i++){
                if(!this.excludeStatus.includes(item.excludeStatus[i])){
                    return false;
                }
            }
        }
        if(this.container_title != null && item.container_title != null){
            if(this.container_title != item.container_title){
                return false;
            }
        }
        if(this.series_title != null && item.series_title != null){
            if(this.series_title != item.series_title){
                return false;
            }
        }


        if(this.doiReferences.length > 0){
            return false;
        }

        if(this.sortBy != item.sortBy){
            return false;
        }

        for(let i = 0; i < item.tags.length; i++){
            if(!this.tags.includes(item.tags[i])){
                return false;
            }
        }

        if(this.keywords.length > 0){
            if(item.keywords.length > 0){
                return false;
            }
        }

        return true;
    }


}
