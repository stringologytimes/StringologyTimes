import { DOIRecordCollection } from "../../doi_record_collection";
import { RecordTypeFieldsetFunctions } from "./record_type_fieldset_functions";
import { YearFieldsetFunctions } from "./year_fieldset_functions";
import { RankFieldsetFunctions } from "./rank_fieldset_functions";



export class SecondarySearchFilterRender {
    public static initialize(initial_record_ids: number[], doiRecordCollection: DOIRecordCollection): void {
        const recordTypeCounters: Map<string, number> = new Map<string, number>();

        doiRecordCollection.lightweightDOIRecords.forEach(record => {
            const recordType = record.type;
            if (recordTypeCounters.has(recordType)) {
                recordTypeCounters.set(recordType, recordTypeCounters.get(recordType)! + 1);
            } else {
                recordTypeCounters.set(recordType, 1);
            }
        }
        );

        let primary_record_count = 0;
        let secondary_record_count = 0;


        const type_to_id_count_mapper = new Map<string, number>();
        initial_record_ids.forEach(id => {
            const record = doiRecordCollection.lightweightDOIRecords[id];
            const recordType = record.type;
            if (type_to_id_count_mapper.has(recordType)) {
                type_to_id_count_mapper.set(recordType, type_to_id_count_mapper.get(recordType)! + 1);
            } else {
                type_to_id_count_mapper.set(recordType, 1);
            }

            if(record.isPrimary) {
                primary_record_count++;
            } else {
                secondary_record_count++;
            }
        });

        const year_to_id_count_mapper = new Map<number, number>();
        let unknown_year_id_count = 0;
        initial_record_ids.forEach(id => {
            const record = doiRecordCollection.lightweightDOIRecords[id];
            const year = record.year;
            if (record.isUnknownYear()) {
                unknown_year_id_count++;
            } else {
                if (year_to_id_count_mapper.has(year)) {
                    year_to_id_count_mapper.set(year, year_to_id_count_mapper.get(year)! + 1);
                } else {
                    year_to_id_count_mapper.set(year, 1);
                }

            }


        });


        RecordTypeFieldsetFunctions.initializeRecordTypes(false, type_to_id_count_mapper);
        YearFieldsetFunctions.initializeYearBox(false, year_to_id_count_mapper, unknown_year_id_count);
        RankFieldsetFunctions.updateRankBox(false, primary_record_count, secondary_record_count);
        //this.initializeContainerBox(doiRecordCollection);
        //this.initializeYearBox(doiRecordCollection);
    }




    /*
  
    public static selectTopContainerBox(selectedTopContainer: string, doiRecordCollection: DOIRecordCollection) {
      const subContainerSelect = document.getElementById("psf-sub-container-select");
      if (subContainerSelect == null) {
        throw new Error("psf-sub-container-select is not found");
      }
  
  
      subContainerSelect.innerHTML = "";
  
      {
        const option = document.createElement("option");
        option.value = "Any";
        option.textContent = "Any";
        subContainerSelect.appendChild(option);
      }
  
      if (selectedTopContainer == "Any") {
  
      } else if (doiRecordCollection.doiToIDMapper.has(selectedTopContainer)) {
        {
          const option = document.createElement("option");
          option.value = "Any";
          option.textContent = "Any";
          subContainerSelect.appendChild(option);
        }
  
  
        var selected_doi_id = doiRecordCollection.doiToIDMapper.get(selectedTopContainer)!;
        console.log("selected_doi_id/" + selected_doi_id + " / " + doiRecordCollection.idToDOIChildrenIDMapper.get(selected_doi_id)?.length);
        doiRecordCollection.idToDOIChildrenIDMapper.get(selected_doi_id)?.forEach(child_id => {
          var child_doi_record = doiRecordCollection.lightweightDOIRecords[child_id];
          const child_type = child_doi_record.type;
          if (containerTypeList.includes(child_type)) {
            var primaryCount = doiRecordCollection.idToPrimaryRecordCountMapper.get(child_id) ?? 0;
            var secondaryCount = doiRecordCollection.idToSecondaryRecordCountMapper.get(child_id) ?? 0;
    
            const option = document.createElement("option");
            option.value = child_doi_record.doi;
            option.textContent = `${child_doi_record.title} (${primaryCount} primary records, ${secondaryCount} secondary records)`;
            subContainerSelect.appendChild(option);  
          }
        });
      } else {
        throw new Error("selectedTopContainer is not found");
      }
    }
  
  
    public static selectTopContainerTypeBox(selectedTopContainerType: string, doiRecordCollection: DOIRecordCollection) {
      const topContainerTypeSelect: HTMLSelectElement = document.getElementById("psf-top-container-type-select") as HTMLSelectElement;
      if (topContainerTypeSelect == null) {
        throw new Error("psf-top-container-type-select is not found");
      }
  
      const topContainerSelect = document.getElementById("psf-top-container-select");
      if (topContainerSelect == null) {
        throw new Error("psf-top-container-select is not found");
      }
  
      const subContainerSelect = document.getElementById("psf-sub-container-select");
      if (subContainerSelect == null) {
        throw new Error("psf-sub-container-select is not found");
      }
  
      //const selectedValue : string = topContainerTypeSelect.value;
      topContainerSelect.innerHTML = "";
      subContainerSelect.innerHTML = "";
  
  
      {
        const anyOption = document.createElement("option");
        anyOption.value = "Any";
        anyOption.textContent = "Any";
        topContainerSelect.appendChild(anyOption);
      }
  
      {
        const anyOption = document.createElement("option");
        anyOption.value = "Any";
        anyOption.textContent = "Any";
        subContainerSelect.appendChild(anyOption);
      }
  
  
      if (topContainerTypeList.includes(selectedTopContainerType)) {
  
        //const key = mapper.get(selectedTopContainerType);
        doiRecordCollection.recordTypeToIDMapper.forEach((idList, recordType) => {
          if (recordType == selectedTopContainerType) {
            idList.forEach(id => {
              var doiRecord = doiRecordCollection.lightweightDOIRecords[id];
              var primaryCount = doiRecordCollection.idToPrimaryRecordCountMapper.get(id) ?? 0;
              var secondaryCount = doiRecordCollection.idToSecondaryRecordCountMapper.get(id) ?? 0;
  
              const option = document.createElement("option");
              option.value = doiRecord.doi;
              option.textContent = `${doiRecord.title} (${primaryCount} primary records, ${secondaryCount} secondary records)`;
              topContainerSelect.appendChild(option);
            }
            );
          }
        });
      }
    }
  
  
    
  
  
    public static initializeContainerBox(doiRecordCollection: DOIRecordCollection) {
      const topContainerTypeSelect = document.getElementById("psf-top-container-type-select");
      if (topContainerTypeSelect == null) {
        throw new Error("psf-top-container-type-select is not found");
      }
  
      {
        const anyOption = document.createElement("option");
        anyOption.value = "Any";
        anyOption.textContent = "Any";
        topContainerTypeSelect.appendChild(anyOption);
      }
  
      topContainerTypeList.forEach((topContainerType) => {
        const option = document.createElement("option");
        option.value = topContainerType;
        option.textContent = topContainerType;
        topContainerTypeSelect.appendChild(option);
      });
  
      this.selectTopContainerTypeBox("Any", doiRecordCollection);
    }
  
    public static getCheckedContainerTypes(): string[] {
      const containerTypeSelect = document.getElementById("psf-container-types-span");
      if (containerTypeSelect == null) {
        throw new Error("psf-container-types-span is not found");
      }
      const containerTypeSelectOptions = containerTypeSelect.querySelectorAll("input");
      const checkedContainerTypes = Array.from(containerTypeSelectOptions).filter(option => (option as HTMLInputElement).checked).map(option => option.value);
      const uncheckedContainerTypes = Array.from(containerTypeSelectOptions).filter(option => !(option as HTMLInputElement).checked).map(option => option.value);
  
      if (uncheckedContainerTypes.length > 0) {
        return checkedContainerTypes;
      } else {
        return ["Container-Any"];
      }
    }
  
    public static getCheckedPaperTypes(): string[] {
      const paperTypeSelect = document.getElementById("psf-paper-types-span");
      if (paperTypeSelect == null) {
        throw new Error("psf-paper-types-span is not found");
      }
      const paperTypeSelectOptions = paperTypeSelect.querySelectorAll("input");
      const checkedPaperTypes = Array.from(paperTypeSelectOptions).filter(option => (option as HTMLInputElement).checked).map(option => option.value);
      const uncheckedPaperTypes = Array.from(paperTypeSelectOptions).filter(option => !(option as HTMLInputElement).checked).map(option => option.value);
  
      if (uncheckedPaperTypes.length > 0) {
        return checkedPaperTypes;
      } else {
        return ["Paper-Any"];
      }
    }
  
    public static getCheckedOtherTypes(): string[] {
      const otherTypeSelect = document.getElementById("psf-other-types-span");
      if (otherTypeSelect == null) {
        throw new Error("psf-other-types-span is not found");
      }
      const otherTypeSelectOptions = otherTypeSelect.querySelectorAll("input");
      const checkedOtherTypes = Array.from(otherTypeSelectOptions).filter(option => (option as HTMLInputElement).checked).map(option => option.value);
      const uncheckedOtherTypes = Array.from(otherTypeSelectOptions).filter(option => !(option as HTMLInputElement).checked).map(option => option.value);
  
      if (uncheckedOtherTypes.length > 0) {
        return checkedOtherTypes;
      } else {
        return ["Other-Any"];
      }
    }
  
    public static getCheckedTypes(): string[] {
      const checkedContainerTypes = this.getCheckedContainerTypes();
      const checkedPaperTypes = this.getCheckedPaperTypes();
      const checkedOtherTypes = this.getCheckedOtherTypes();
      return [...checkedContainerTypes, ...checkedPaperTypes, ...checkedOtherTypes];
    }
  
    public static getTopContainerDOI(): string | null {
      const topContainerSelect: HTMLSelectElement = document.getElementById("psf-top-container-select") as HTMLSelectElement;
      if (topContainerSelect == null) {
        throw new Error("psf-top-container-select is not found");
      }
      const selectedValue = topContainerSelect.value;
      if (selectedValue == "Any") {
        return null;
      } else {
        return selectedValue;
      }
    }
    public static getTopContainerType(): string | null {
      const topContainerTypeSelect: HTMLSelectElement = document.getElementById("psf-top-container-type-select") as HTMLSelectElement;
      if (topContainerTypeSelect == null) {
        throw new Error("psf-top-container-type-select is not found");
      }
      const selectedValue = topContainerTypeSelect.value;
      if (selectedValue == "Any") {
        return null;
      }
      return selectedValue;
    }
  
    public static getSubContainerDOI(): string | null {
      const subContainerSelect: HTMLSelectElement = document.getElementById("psf-sub-container-select") as HTMLSelectElement;
      if (subContainerSelect == null) {
        throw new Error("psf-sub-container-select is not found");
      }
      const selectedValue = subContainerSelect.value;
      if (selectedValue == "Any") {
        return null;
      }
      return selectedValue;
    }
  
    public static getYearFrom(): string | null {
      const yearFromSelect: HTMLSelectElement = document.getElementById("psf-year-from-select") as HTMLSelectElement;
      if (yearFromSelect == null) {
        throw new Error("psf-year-from-select is not found");
      }
      const selectedValue = yearFromSelect.value;
      if (selectedValue == "Any") {
        return null;
      }
      return selectedValue;
    }
  
    public static getYearTo(): string | null {
      const yearToSelect: HTMLSelectElement = document.getElementById("psf-year-to-select") as HTMLSelectElement;
      if (yearToSelect == null) {
        throw new Error("psf-year-to-select is not found");
      }
      const selectedValue = yearToSelect.value;
      if (selectedValue == "Any") {
        return null;
      }
      return selectedValue;
    }
  
    public static getExcludedStatus(): string[] {
      const excludedStatus = [];
      const primaryRecordCheckbox: HTMLInputElement = document.getElementById("psf-primary-record-checkbox") as HTMLInputElement;
      if (primaryRecordCheckbox == null) {
        throw new Error("psf-primary-record-checkbox is not found");
      }
      const b1 = primaryRecordCheckbox.checked;
      if (!b1) {
        excludedStatus.push("primary");
      }
  
      const secondaryRecordCheckbox: HTMLInputElement = document.getElementById("psf-secondary-record-checkbox") as HTMLInputElement;
      if (secondaryRecordCheckbox == null) {
        throw new Error("psf-secondary-record-checkbox is not found");
      }
      const b2 = secondaryRecordCheckbox.checked;
      if (!b2) {
        excludedStatus.push("secondary");
      }
      return excludedStatus;
    }
  
    public static setURLParameters(doiRecordCollection: DOIRecordCollection): void {
      const url = new URL(window.location.href);
      url.searchParams.delete("psf-type");
      const newTypes = this.getCheckedTypes();
      newTypes.forEach(type => {
        url.searchParams.append("psf-type", type);
      });
  
      const newSubContainerDOI = this.getSubContainerDOI();
  
      url.searchParams.delete("ancestor-doi");
      url.searchParams.delete("top-container-type");
  
  
      if (newSubContainerDOI != null) {
        url.searchParams.set("ancestor-doi", newSubContainerDOI);
      } else {
        const newTopContainerDOI = this.getTopContainerDOI();
        if (newTopContainerDOI != null) {
          url.searchParams.set("ancestor-doi", newTopContainerDOI);
        } else {
          const newTopContainerType = this.getTopContainerType();
          if (newTopContainerType != null) {
            url.searchParams.set("top-container-type", newTopContainerType);
          }
        }
      }
  
  
  
  
      const newYearFrom = this.getYearFrom();
      url.searchParams.delete("psf-year-from");
      if (newYearFrom != null) {
        url.searchParams.append("psf-year-from", newYearFrom);
      }
      const newYearTo = this.getYearTo();
      url.searchParams.delete("psf-year-to");
      if (newYearTo != null) {
        url.searchParams.append("psf-year-to", newYearTo);
      }
  
      const newExcludedStatus = this.getExcludedStatus();
      console.log("newExcludedStatus: " + newExcludedStatus);
      url.searchParams.delete("psf-excluded-status");
      if (newExcludedStatus.length > 0) {
        newExcludedStatus.forEach(status => {
          url.searchParams.append("psf-excluded-status", status);
        });
      }
  
      window.history.replaceState(null, "", url.toString());
  
  
    }
    */

}