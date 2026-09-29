import { LightWeightDOIRecord, topContainerTypeList } from "./doi_record";
import { DOIRecord} from "./doi_record";
import { load_gzip_text_lines, load_gzip_integer_list_lines, load_gzip_integer_lines } from "./gzip_loader";
import { containerTypeList, paperTypeList, otherTypeList } from "./doi_record";
import { FoundRecordSummary } from "./doi_filter/found_record_summary";

let typeList: string[] = [];


export function getDOIRecordTypeList(): string[] {
    return typeList.map(type => type);
}


export class DOIRecordCollection {
    public lightweightDOIRecords: LightWeightDOIRecord[] = [];
    public authorList: string[] = [];
    public tagList: string[] = [];
    public doiToIDMapper: Map<string, number> = new Map();
    public idToDOIChildrenIDMapper: Map<number, number[]> = new Map();
    public recordTypeToIDMapper: Map<string, number[]> = new Map();
    public recordSummary: FoundRecordSummary = new FoundRecordSummary();

    



    public length(): number {
        return this.lightweightDOIRecords.length;
    }
    public getIDByDOI(doi: string): number | null {
        if(this.doiToIDMapper.has(doi)){
            return this.doiToIDMapper.get(doi)!;
        } else {
            return null;
        }
    }
    public getDOIByID(id: number): string {
        if(id >= this.lightweightDOIRecords.length){
            console.error("id is greater than the length of lightweightDOIInfos");
            console.error("id: " + id);
            console.error("length of lightweightDOIInfos: " + this.lightweightDOIRecords.length);
            throw new Error("id is greater than the length of lightweightDOIInfos");
        }
        if(Number.isNaN(id)){
            console.error("id is NaN");
            console.error("id: " + id);
            throw new Error("id is NaN");
        }
        return this.lightweightDOIRecords[id].doi;
    }
    public getContainerTypeChildrenCount(index: number): number {
        if(this.idToDOIChildrenIDMapper.has(index)){
            return this.idToDOIChildrenIDMapper.get(index)!.filter(id => this.getDOIInfo(id).isContainerType()).length;
        }else{
            return 0;
        }
    }
    public getPrimaryDescendantCount(index: number): number {
        if(this.idToDOIChildrenIDMapper.has(index)){
            let counter = 0;
            this.idToDOIChildrenIDMapper.get(index)!.forEach(id => {
                var childRecord = this.getDOIInfo(id);
                if(childRecord.isPrimary){
                    counter++;
                }
                counter += this.getPrimaryDescendantCount(id);
            });
            return counter;

        }else{
            return 0;
        }
    }
    public getSecondaryDescendantCount(index: number): number {
        if(this.idToDOIChildrenIDMapper.has(index)){
            let counter = 0;
            this.idToDOIChildrenIDMapper.get(index)!.forEach(id => {
                var childRecord = this.getDOIInfo(id);
                if(!childRecord.isPrimary){
                    counter++;
                }
                counter += this.getSecondaryDescendantCount(id);
            });
            return counter;

        }else{
            return 0;
        }
    }

    public getDOIInfo(index: number): DOIRecord {
        let r = new DOIRecord();
        r.id = index;
        r.doi = this.lightweightDOIRecords[index].doi;
        r.title = this.lightweightDOIRecords[index].title;
        r.year = this.lightweightDOIRecords[index].year;
        r.month = this.lightweightDOIRecords[index].month;
        r.authors = this.lightweightDOIRecords[index].authorIDs.map(id => this.authorList[id]);
        r.seriesTitle = this.lightweightDOIRecords[index].seriesTitle;
        r.container_title = this.lightweightDOIRecords[index].container_title;
        r.volume_issue = this.lightweightDOIRecords[index].volume_issue;
        r.container_DOI = this.lightweightDOIRecords[index].container_DOI;
        r.doiReferences = this.lightweightDOIRecords[index].doiReferenceIDs.map(id => this.getDOIByID(id));
        r.type = this.lightweightDOIRecords[index].type;
        r.tags = this.lightweightDOIRecords[index].tags.map(tag => tag);
        r.optional_ids = this.lightweightDOIRecords[index].optional_ids.map(id => id);
        if (this.lightweightDOIRecords[index].isPrimary) {
            r.isPrimary = true;
        } else{
            r.isPrimary = false;
        }
        return r;
    }

    public ancestorCheck(doiID: number, ancestorDOI: string): boolean {
        var doiInfo = this.lightweightDOIRecords[doiID];
        if(doiInfo.container_DOI.length == 0){
            return false;
        }else{
            if(doiInfo.container_DOI == ancestorDOI){
                return true;
            }else{
                if(this.doiToIDMapper.has(doiInfo.container_DOI)){
                    var containerDOIID = this.doiToIDMapper.get(doiInfo.container_DOI)!;
                    return this.ancestorCheck(containerDOIID, ancestorDOI);
                }else{
                    return false;
                }
            }
        }
    }
    
    public topContainerTypeCheck(doiID: number, topContainerType: string): boolean {
        var doiInfo = this.lightweightDOIRecords[doiID];
        if(doiInfo.container_DOI.length == 0){
            return doiInfo.type == topContainerType;
        }else{
            if(this.doiToIDMapper.has(doiInfo.container_DOI)){
                var containerDOIID = this.doiToIDMapper.get(doiInfo.container_DOI)!;
                return this.topContainerTypeCheck(containerDOIID, topContainerType);
            }else{
                return false;
            }
        }
    }

    private getAncestorIDListHelper(doiID: number, outputList: number[]) {
        var doiInfo = this.lightweightDOIRecords[doiID];
        if(doiInfo.container_DOI.length > 0){
            if(this.doiToIDMapper.has(doiInfo.container_DOI)){
                var containerDOIID = this.doiToIDMapper.get(doiInfo.container_DOI)!;
                outputList.push(containerDOIID);
                this.getAncestorIDListHelper(containerDOIID, outputList);
            }
        }
        return outputList;
    }
    public getAncestorIDList(doiID: number): number[] {
        var ancestorIDList: number[] = [];
        this.getAncestorIDListHelper(doiID, ancestorIDList);
        return ancestorIDList;
    }



    public static async load(folderURL: string): Promise<DOIRecordCollection> {
        console.info("loading DOIInfoCollection from: " + folderURL);
        let r = new DOIRecordCollection();
        const doi_list = await load_gzip_text_lines(folderURL + "/doi.csv.gz");
        console.info("size of doi_list: " + doi_list.length);
        doi_list.forEach(line => {
            let doiInfo = new LightWeightDOIRecord();
            doiInfo.doi = line;
            r.lightweightDOIRecords.push(doiInfo);
        });

        var word_list = await load_gzip_text_lines(folderURL + "/word.csv.gz");
        var title_list = await load_gzip_integer_list_lines(folderURL + "/compressed_title.csv.gz");
        title_list.forEach((numbers, index) => {
            const title = numbers.map(numbers => word_list[numbers]).join(" ");
            r.lightweightDOIRecords[index].title = title;
        });

        const year_list = await load_gzip_integer_lines(folderURL + "/year.csv.gz");
        console.info("size of year_list: " + year_list.length);
        year_list.forEach((year, index) => {
            r.lightweightDOIRecords[index].year = year;
        });

        const month_list = await load_gzip_integer_lines(folderURL + "/month.csv.gz");
        console.info("size of month_list: " + month_list.length);
        month_list.forEach((month, index) => {
            r.lightweightDOIRecords[index].month = month;
        });

        r.authorList = await load_gzip_text_lines(folderURL + "/full_name.csv.gz");
        const author_number_list = await load_gzip_integer_list_lines(folderURL + "/compressed_full_name.csv.gz");
        console.info("size of author_number_list: " + author_number_list.length);
        author_number_list.forEach((numbers, index) => {
            r.lightweightDOIRecords[index].authorIDs = numbers;
        });

        const volume_list = await load_gzip_text_lines(folderURL + "/volume_issue.csv.gz");
        console.info("size of volume_issue_list: " + volume_list.length);
        volume_list.forEach((volume, index) => {
            r.lightweightDOIRecords[index].volume_issue = volume;
        });

        const series_title_list = await load_gzip_text_lines(folderURL + "/series_title.csv.gz");
        console.info("size of series_title_list: " + series_title_list.length);
        series_title_list.forEach((series_title, index) => {
            r.lightweightDOIRecords[index].seriesTitle = series_title;
        });

        const container_DOI_list = await load_gzip_text_lines(folderURL + "/container_DOI.csv.gz");
        console.info("size of container_DOI_list: " + container_DOI_list.length);
        container_DOI_list.forEach((container_DOI, index) => {
            r.lightweightDOIRecords[index].container_DOI = container_DOI;
        });

        const container_title_list = await load_gzip_text_lines(folderURL + "/container_title.csv.gz");
        console.info("size of container_title_list: " + container_title_list.length);
        container_title_list.forEach((container_title, index) => {
            r.lightweightDOIRecords[index].container_title = container_title;
        });

        const doi_references_list = await load_gzip_integer_list_lines(folderURL + "/compressed_doi_reference.csv.gz");
        console.info("size of doi_references_list: " + doi_references_list.length);
        doi_references_list.forEach((numbers, index) => {
            r.lightweightDOIRecords[index].doiReferenceIDs = numbers;
        });
        
        const type_list = await load_gzip_text_lines(folderURL + "/type.csv.gz");
        const type_set = new Set<string>();
        console.info("size of type_list: " + type_list.length);
        type_list.forEach((type, index) => {
            if (type.length > 0) {
                r.lightweightDOIRecords[index].type = type;
                type_set.add(type);
            } else {
                r.lightweightDOIRecords[index].type = "unknown";
                type_set.add("unknown");
            }
        });

        typeList = Array.from(type_set);
        console.info("typeList: " + typeList);

        const status_list = await load_gzip_integer_lines(folderURL + "/doi_flag.csv.gz");
        console.info("size of status_list: " + status_list.length);
        status_list.forEach((status, index) => {
            if (index >= r.lightweightDOIRecords.length) {
                console.info("status_list is longer than lightweightDOIInfos");
                throw new Error("status_list is longer than lightweightDOIInfos");
            }            
            r.lightweightDOIRecords[index].isPrimary = status == 1;
        });

        for(let i = 0; i < r.lightweightDOIRecords.length; i++){
            if(r.lightweightDOIRecords[i] === undefined){
                console.info("lightweightDOIInfos[i] is undefined");
                console.info("i: " + i);
                console.info("length of lightweightDOIInfos: " + r.lightweightDOIRecords.length);
                throw new Error("lightweightDOIInfos[i] is undefined");
            }
        }

        const tag_list = await load_gzip_text_lines(folderURL + "/tag.csv.gz");
        const tag_index_list = await load_gzip_integer_list_lines(folderURL + "/tag_of_each_element.csv.gz");
        r.tagList = tag_list;

        tag_index_list.forEach((numbers, index) => {
            numbers.forEach((number) => {
                r.lightweightDOIRecords[index].tags.push(tag_list[number]);
            });
        });

        r.doiToIDMapper = new Map<string, number>();
        r.lightweightDOIRecords.forEach((doiInfo, index) => {
            r.doiToIDMapper.set(doiInfo.doi, index);
        });

        const optional_ids_list = await load_gzip_text_lines(folderURL + "/optional_id.csv.gz");
        console.info("size of optional_ids_list: " + optional_ids_list.length);
        var optional_ids_index = 0;
        optional_ids_list.forEach((optional_ids) => {
            if(optional_ids == ""){
                optional_ids_index++;
            }else{
                r.lightweightDOIRecords[optional_ids_index].optional_ids = optional_ids.split(",");
            }
        });



        r.lightweightDOIRecords.forEach((doiInfo, index) => {
            if(doiInfo.container_DOI.length > 0){
                if(r.doiToIDMapper.has(doiInfo.container_DOI)){
                    var container_id = r.doiToIDMapper.get(doiInfo.container_DOI)!;
                    if(r.idToDOIChildrenIDMapper.has(container_id)){
                        r.idToDOIChildrenIDMapper.get(container_id)!.push(index);
                    }else{
                        r.idToDOIChildrenIDMapper.set(container_id, [index])
                    }
                }
            }
        });




        r.lightweightDOIRecords.forEach((doiInfo, index) => {
            if(r.recordTypeToIDMapper.has(doiInfo.type)){
                r.recordTypeToIDMapper.get(doiInfo.type)!.push(index);
            }else{
                r.recordTypeToIDMapper.set(doiInfo.type, [index]);
            }
        });

        

        

        Array.from(r.recordTypeToIDMapper.keys()).forEach((type) => {
            if(!containerTypeList.includes(type) && !paperTypeList.includes(type)){
                otherTypeList.push(type);
            }
        });
        
        r.recordSummary = r.buildRecordSummary(r.lightweightDOIRecords.map((record, index) => index));


        console.info("lightweightDOIInfos is loaded successfully : " + r.lightweightDOIRecords.length);



        return r;

    }

    public buildRecordSummary(initial_record_ids: number[]): FoundRecordSummary {
        let found_record_summary = new FoundRecordSummary();

        initial_record_ids.forEach(id => {
            const record = this.lightweightDOIRecords[id];
            const recordType = record.type;
            if (found_record_summary.type_to_id_count_mapper.has(recordType)) {
                found_record_summary.type_to_id_count_mapper.set(recordType, found_record_summary.type_to_id_count_mapper.get(recordType)! + 1);
            } else {
                found_record_summary.type_to_id_count_mapper.set(recordType, 1);
            }

            if (record.isPrimary) {
                found_record_summary.primary_record_count++;
            } else {
                found_record_summary.secondary_record_count++;
            }

            const year = record.year;
            if (record.isUnknownYear()) {
                found_record_summary.unknown_year_id_count++;
            } else {
                if (found_record_summary.year_to_id_count_mapper.has(year)) {
                    found_record_summary.year_to_id_count_mapper.set(year, found_record_summary.year_to_id_count_mapper.get(year)! + 1);
                } else {
                    found_record_summary.year_to_id_count_mapper.set(year, 1);
                }
            }

            const ancestorIDList = this.getAncestorIDList(id);
            if (record.isPrimary) {
                ancestorIDList.forEach(ancestorID => {
                    if (found_record_summary.idToPrimaryRecordCountMapper.has(ancestorID)) {
                        found_record_summary.idToPrimaryRecordCountMapper.set(ancestorID, found_record_summary.idToPrimaryRecordCountMapper.get(ancestorID)! + 1);
                    } else {
                        found_record_summary.idToPrimaryRecordCountMapper.set(ancestorID, 1);
                    }
                });
            } else {
                ancestorIDList.forEach(ancestorID => {
                    if (found_record_summary.idToSecondaryRecordCountMapper.has(ancestorID)) {
                        found_record_summary.idToSecondaryRecordCountMapper.set(ancestorID, found_record_summary.idToSecondaryRecordCountMapper.get(ancestorID)! + 1);
                    } else {
                        found_record_summary.idToSecondaryRecordCountMapper.set(ancestorID, 1);
                    }
                });
            }
        });

        return found_record_summary;
    }
}


