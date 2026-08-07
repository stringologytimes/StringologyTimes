import { containerTypeList, paperTypeList, topContainerTypeList } from "../../../doi_record";
import { DOIRecordCollection } from "../../../doi_record_collection";

export class ContainerDOIFieldsetFunctions {
    public static selectTopContainerBox(is_primary_filter: boolean, selectedTopContainer: string, doiRecordCollection: DOIRecordCollection,
        removeEmptyContainers: boolean,
        idToPrimaryRecordCountMapper: Map<number, number>, idToSecondaryRecordCountMapper: Map<number, number>) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const subContainerSelect = document.getElementById(id_prefix + "-sub-container-select");
        if (subContainerSelect == null) {
            throw new Error(id_prefix + "-sub-container-select is not found");
        }


        subContainerSelect.replaceChildren();

        {
            const option = document.createElement("option");
            option.value = "Any";
            option.textContent = "Any";
            subContainerSelect.appendChild(option);
        }

        if (doiRecordCollection.doiToIDMapper.has(selectedTopContainer)) {
            const selected_doi_id = doiRecordCollection.doiToIDMapper.get(selectedTopContainer)!;
            const children_ids = doiRecordCollection.idToDOIChildrenIDMapper.get(selected_doi_id)!;

            children_ids.forEach(child_id => {
                var child_doi_record = doiRecordCollection.lightweightDOIRecords[child_id];
                const child_type = child_doi_record.type;
                if (containerTypeList.includes(child_type)) {
                    var primaryCount = idToPrimaryRecordCountMapper.get(child_id) ?? 0;
                    var secondaryCount = idToSecondaryRecordCountMapper.get(child_id) ?? 0;
                    if (!removeEmptyContainers || (primaryCount > 0 || secondaryCount > 0)) {
                        const option = document.createElement("option");
                        option.value = child_doi_record.doi;
                        option.textContent = `${child_doi_record.title} (${primaryCount} primary records, ${secondaryCount} secondary records)`;
                        subContainerSelect.appendChild(option);
                    }

                }
            });
        } else if (selectedTopContainer != "Any") {
            throw new Error("selectedTopContainer is not found");
        }
    }



    public static selectTopContainerTypeBox(is_primary_filter: boolean, selectedTopContainerType: string, doiRecordCollection: DOIRecordCollection, 
        removeEmptyContainers: boolean,
        idToPrimaryRecordCountMapper: Map<number, number>, idToSecondaryRecordCountMapper: Map<number, number>) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const topContainerTypeSelect: HTMLSelectElement = document.getElementById(id_prefix + "-top-container-type-select") as HTMLSelectElement;
        if (topContainerTypeSelect == null) {
            throw new Error(id_prefix + "-top-container-type-select is not found");
        }

        const topContainerSelect = document.getElementById(id_prefix + "-top-container-select");
        if (topContainerSelect == null) {
            throw new Error(id_prefix + "-top-container-select is not found");
        }

        const subContainerSelect = document.getElementById(id_prefix + "-sub-container-select");
        if (subContainerSelect == null) {
            throw new Error(id_prefix + "-sub-container-select is not found");
        }

        //const selectedValue : string = topContainerTypeSelect.value;
        topContainerSelect.replaceChildren();
        subContainerSelect.replaceChildren();


        {
            const anyOption = document.createElement("option");
            anyOption.value = "Any";

            if(selectedTopContainerType != "Any"){
                anyOption.textContent = "Any " + selectedTopContainerType;
            }else{
                anyOption.textContent = "Any Type";
            }

            topContainerSelect.appendChild(anyOption);
        }

        {
            const anyOption = document.createElement("option");
            anyOption.value = "Any";

            if(selectedTopContainerType == "Journal"){
                anyOption.textContent = "Any Journal Issue";
            }else if(selectedTopContainerType == "Proceedings Collection"){
                anyOption.textContent = "Any Conference Proceedings";
            }else if(selectedTopContainerType == "Preprint Repository"){
                anyOption.textContent = "Any Preprint";
            }else{
                anyOption.textContent = "Any Type";
            }


            subContainerSelect.appendChild(anyOption);
        }

        let index_counter = 0;


        if (topContainerTypeList.includes(selectedTopContainerType)) {
            doiRecordCollection.recordTypeToIDMapper.forEach((idList, recordType) => {
                if (recordType == selectedTopContainerType) {
                    idList.forEach(id => {
                        var doiRecord = doiRecordCollection.lightweightDOIRecords[id];
                        var primaryCount = idToPrimaryRecordCountMapper.get(id) ?? 0;
                        var secondaryCount = idToSecondaryRecordCountMapper.get(id) ?? 0;

                        if(!removeEmptyContainers || (primaryCount > 0 || secondaryCount > 0)) {
                            const option = document.createElement("option");
                            option.value = doiRecord.doi;
                            option.textContent = `${index_counter++}. ${doiRecord.title} (${primaryCount} primary records, ${secondaryCount} secondary records)`;
                            topContainerSelect.appendChild(option);                                
                        }
                    }
                    );
                }
            });
        }
    }

    public static initializeContainerBox(is_primary_filter: boolean, doiRecordCollection: DOIRecordCollection, removeEmptyContainers: boolean,
        idToPrimaryRecordCountMapper: Map<number, number>, idToSecondaryRecordCountMapper: Map<number, number>) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const topContainerTypeSelect = document.getElementById(id_prefix + "-top-container-type-select");
        if (topContainerTypeSelect == null) {
            throw new Error(id_prefix + "-top-container-type-select is not found");
        }
        topContainerTypeSelect.replaceChildren();

        {
            const anyOption = document.createElement("option");
            anyOption.value = "Any";
            anyOption.textContent = "Any Type";
            topContainerTypeSelect.appendChild(anyOption);
        }

        topContainerTypeList.forEach((topContainerType) => {
            const option = document.createElement("option");
            option.value = topContainerType;
            option.textContent = topContainerType;
            topContainerTypeSelect.appendChild(option);
        });

        this.selectTopContainerTypeBox(is_primary_filter, "Any", doiRecordCollection, removeEmptyContainers, idToPrimaryRecordCountMapper, idToSecondaryRecordCountMapper);
    }
    public static getTopContainerDOI(is_primary_filter: boolean): string | null {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const topContainerSelect: HTMLSelectElement = document.getElementById(id_prefix + "-top-container-select") as HTMLSelectElement;
        if (topContainerSelect == null) {
            throw new Error(id_prefix + "-top-container-select is not found");
        }
        const selectedValue = topContainerSelect.value;
        if (selectedValue == "Any") {
            return null;
        } else {
            return selectedValue;
        }
    }
    public static getTopContainerType(is_primary_filter: boolean): string | null {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const topContainerTypeSelect: HTMLSelectElement = document.getElementById(id_prefix + "-top-container-type-select") as HTMLSelectElement;
        if (topContainerTypeSelect == null) {
            throw new Error(id_prefix + "-top-container-type-select is not found");
        }
        const selectedValue = topContainerTypeSelect.value;
        if (selectedValue == "Any") {
            return null;
        }
        return selectedValue;
    }

    public static getSubContainerDOI(is_primary_filter: boolean): string | null {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const subContainerSelect: HTMLSelectElement = document.getElementById(id_prefix + "-sub-container-select") as HTMLSelectElement;
        if (subContainerSelect == null) {
            throw new Error(id_prefix + "-sub-container-select is not found");
        }
        const selectedValue = subContainerSelect.value;
        if (selectedValue == "Any") {
            return null;
        }
        return selectedValue;
    }

    public static convertInputToURLParameters(isPrimaryFilter: boolean): [string, string][] {
        const prefix = isPrimaryFilter ? "psf-" : "ssf-";
        const r: [string, string][] = [];
        const subContainerDOI = this.getSubContainerDOI(isPrimaryFilter);
        if (subContainerDOI != null) {
            r.push([prefix + "ancestor-doi", subContainerDOI]);
        }else{
            const topContainerDOI = this.getTopContainerDOI(isPrimaryFilter);
            if (topContainerDOI != null) {
                r.push([prefix + "ancestor-doi", topContainerDOI]);
            }else{
                const topContainerType = this.getTopContainerType(isPrimaryFilter);
                if (topContainerType != null) {
                    r.push([prefix + "top-container-type", topContainerType]);
                }        
            } 
        }
        return r;
    }



}
