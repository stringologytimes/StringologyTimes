import { containerTypeList, paperTypeList, topContainerTypeList } from "../../../doi_record";
import { DOIRecordCollection } from "../../../doi_record_collection";

export class ContainerDOIFieldsetFunctions {
    public static selectTopContainerBox(is_primary_filter: boolean, selectedTopContainer: string | null, selectedSubContainerDOI: string | null, 
        removeEmptyContainers: boolean,
        idToPrimaryRecordCountMapper: Map<number, number>, idToSecondaryRecordCountMapper: Map<number, number>, 
        doiRecordCollection: DOIRecordCollection) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const subContainerSelect : HTMLSelectElement = document.getElementById(id_prefix + "-sub-container-select") as HTMLSelectElement;
        if (subContainerSelect == null) {
            throw new Error(id_prefix + "-sub-container-select is not found");
        }

        let selectedIndex = 0;


        subContainerSelect.replaceChildren();

        {
            const option = document.createElement("option");
            option.value = "Any";
            option.textContent = "Any";
            subContainerSelect.appendChild(option);
        }

        let index_counter = 0;

        if (selectedTopContainer != null && doiRecordCollection.doiToIDMapper.has(selectedTopContainer!)) {
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
                        if (child_doi_record.doi == selectedSubContainerDOI) {
                            selectedIndex = index_counter + 1;
                        }
                        index_counter++;
                    }

                }
            });
        } else if (selectedTopContainer != null) {
            throw new Error("selectedTopContainer is not found / selectedTopContainer: " + selectedTopContainer);
        }
        subContainerSelect.selectedIndex = selectedIndex;
    }



    public static selectTopContainerTypeBox(is_primary_filter: boolean, selectedTopContainerType: string | null, selectedTopContainerDOI: string | null,
        removeEmptyContainers: boolean, idToPrimaryRecordCountMapper: Map<number, number>, idToSecondaryRecordCountMapper: Map<number, number>, 
    doiRecordCollection: DOIRecordCollection) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const topContainerTypeSelect = document.getElementById(id_prefix + "-top-container-type-select");
        if (topContainerTypeSelect == null) {
            throw new Error(id_prefix + "-top-container-type-select is not found");
        }

        const topContainerSelect : HTMLSelectElement = document.getElementById(id_prefix + "-top-container-select") as HTMLSelectElement;
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

        let selectedIndex = 0;


        {
            const anyOption = document.createElement("option");
            anyOption.value = "Any";

            if (selectedTopContainerType != null) {
                anyOption.textContent = "Any " + selectedTopContainerType;
            } else {
                anyOption.textContent = "Any Type";
            }

            topContainerSelect.appendChild(anyOption);
        }

        {
            const anyOption = document.createElement("option");
            anyOption.value = "Any";

            if (selectedTopContainerType == "Journal") {
                anyOption.textContent = "Any Journal Issue";
            } else if (selectedTopContainerType == "Proceedings Collection") {
                anyOption.textContent = "Any Conference Proceedings";
            } else if (selectedTopContainerType == "Preprint Repository") {
                anyOption.textContent = "Any Preprint";
            } else {
                anyOption.textContent = "Any Type";
            }


            subContainerSelect.appendChild(anyOption);
        }

        let index_counter = 0;


        if (selectedTopContainerType != null && topContainerTypeList.includes(selectedTopContainerType!)) {
            doiRecordCollection.recordTypeToIDMapper.forEach((idList, recordType) => {
                if (recordType == selectedTopContainerType) {
                    idList.forEach(id => {
                        var doiRecord = doiRecordCollection.lightweightDOIRecords[id];
                        var primaryCount = idToPrimaryRecordCountMapper.get(id) ?? 0;
                        var secondaryCount = idToSecondaryRecordCountMapper.get(id) ?? 0;

                        const addFlag = !removeEmptyContainers || (primaryCount > 0 || secondaryCount > 0);

                        if (addFlag) {

                            const option = document.createElement("option");
                            option.value = doiRecord.doi;
                            option.textContent = `${index_counter}. ${doiRecord.title} (${primaryCount} primary records, ${secondaryCount} secondary records)`;
                            topContainerSelect.appendChild(option);

                            if (doiRecord.doi == selectedTopContainerDOI) {
                                selectedIndex = index_counter + 1;
                            }
                            index_counter++;
                        }
                    }
                    );
                }
            });
        }
        topContainerSelect.selectedIndex = selectedIndex;
    }

    public static initializeTopContainerTypeBox(is_primary_filter: boolean, selectedTopContainerType: string | null) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const topContainerTypeSelect : HTMLSelectElement = document.getElementById(id_prefix + "-top-container-type-select") as HTMLSelectElement;
        if (topContainerTypeSelect == null) {
            throw new Error(id_prefix + "-top-container-type-select is not found");
        }
        topContainerTypeSelect.replaceChildren();
        let selectedIndex = 0;

        {
            const anyOption = document.createElement("option");
            anyOption.value = "Any";
            anyOption.textContent = "Any Type";
            topContainerTypeSelect.appendChild(anyOption);
        }

        topContainerTypeList.forEach((topContainerType, index) => {
            const option = document.createElement("option");
            option.value = topContainerType;
            option.textContent = topContainerType;
            topContainerTypeSelect.appendChild(option);

            if (topContainerType == selectedTopContainerType) {
                selectedIndex = index + 1;
            }
        });

        topContainerTypeSelect.selectedIndex = selectedIndex;
    }





    public static initialize(is_primary_filter: boolean, removeEmptyContainers: boolean, 
        idToPrimaryRecordCountMapper: Map<number, number>, idToSecondaryRecordCountMapper: Map<number, number>, 
        doiRecordCollection: DOIRecordCollection, 
        selectedTopContainerType: string | null, selectedTopContainerDOI: string | null, selectedSubContainerDOI: string | null) {
        this.initializeTopContainerTypeBox(is_primary_filter, selectedTopContainerType);
        this.selectTopContainerTypeBox(is_primary_filter, selectedTopContainerType, selectedTopContainerDOI, removeEmptyContainers, idToPrimaryRecordCountMapper, idToSecondaryRecordCountMapper, doiRecordCollection);
        this.selectTopContainerBox(is_primary_filter, selectedTopContainerDOI, selectedSubContainerDOI, 
            removeEmptyContainers, idToPrimaryRecordCountMapper, idToSecondaryRecordCountMapper, doiRecordCollection);
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
            r.push([prefix + "sub-container-doi", subContainerDOI]);
        } 
        const topContainerDOI = this.getTopContainerDOI(isPrimaryFilter);
        if (topContainerDOI != null) {
            r.push([prefix + "top-container-doi", topContainerDOI]);
        } 
        
        const topContainerType = this.getTopContainerType(isPrimaryFilter);
        if (topContainerType != null) {
            r.push([prefix + "top-container-type", topContainerType]);
        }


        return r;
    }



}
