import { containerTypeList, otherTypeList, paperTypeList, topContainerTypeList } from "../../../doi_record";

export class RecordTypeFieldsetFunctions {
    public static initializeRecordTypes(is_primary_filter: boolean, type_to_id_count_mapper: Map<string, number>) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";


        const typeListContainerSpan = document.getElementById(id_prefix + "-container-types-span");
        if (typeListContainerSpan == null) {
            throw new Error(id_prefix + "-container-types-span is not found");
        }
        const typeListPaperSpan = document.getElementById(id_prefix + "-paper-types-span");
        if (typeListPaperSpan == null) {
            throw new Error(id_prefix + "-paper-types-span is not found");
        }
        const typeListOtherSpan = document.getElementById(id_prefix + "-other-types-span");
        if (typeListOtherSpan == null) {
            throw new Error(id_prefix + "-other-types-span is not found");
        }

        typeListContainerSpan.replaceChildren();
        typeListPaperSpan.replaceChildren();
        typeListOtherSpan.replaceChildren();

        containerTypeList.forEach(type => {
            if (type_to_id_count_mapper.has(type)) {
                const checkbox = document.createElement("input");
                checkbox.type = "checkbox";
                checkbox.id = `${id_prefix}-checkbox_${type}`;
                checkbox.value = type;
                checkbox.checked = true;

                const count = type_to_id_count_mapper.get(type)!;
                const label = document.createElement("label");
                label.htmlFor = `${id_prefix}-checkbox_${type}`;
                label.textContent = `${type} (${count})`;

                typeListContainerSpan.appendChild(checkbox);
                typeListContainerSpan.appendChild(label);
            }
        });

        paperTypeList.forEach(type => {
            if (type_to_id_count_mapper.has(type)) {
                const checkbox = document.createElement("input");
                checkbox.type = "checkbox";
                checkbox.id = `${id_prefix}-checkbox_${type}`;
                checkbox.value = type;
                checkbox.checked = true;

                const count = type_to_id_count_mapper.get(type)!;
                const label = document.createElement("label");
                label.htmlFor = `${id_prefix}-checkbox_${type}`;
                label.textContent = `${type} (${count})`;

                typeListPaperSpan.appendChild(checkbox);
                typeListPaperSpan.appendChild(label);
            }
        });

        otherTypeList.forEach(type => {
            if (type_to_id_count_mapper.has(type)) {
                const checkbox = document.createElement("input");
                checkbox.type = "checkbox";
                checkbox.id = `${id_prefix}-checkbox_${type}`;
                checkbox.value = type;
                checkbox.checked = true;

                const count = type_to_id_count_mapper.get(type)!;
                const label = document.createElement("label");
                label.htmlFor = `${id_prefix}-checkbox_${type}`;
                label.textContent = `${type} (${count})`;
                label.setAttribute("data-record-type", type);

                typeListOtherSpan.appendChild(checkbox);
                typeListOtherSpan.appendChild(label);
            }
        });
    }

    private static updateRecordTypes(is_primary_filter: boolean, type_to_id_count_mapper: Map<string, number>) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const recordTypesFieldset = document.getElementById(`${id_prefix}-record-types-fieldset`) as HTMLElement;
        if (recordTypesFieldset == null) {
            throw new Error(`${id_prefix}-record-types-fieldset is not found`);
        } else {
            const labels = recordTypesFieldset.querySelectorAll<HTMLLabelElement>("label");
            labels.forEach(label => {
                const recordType = label.getAttribute("data-record-type") as string;
                const count = type_to_id_count_mapper.get(recordType)!;
                if (recordType != null) {
                    label.textContent = `${recordType} (${count})`;
                }
            });
        }
    }

    public static getCheckedContainerTypes(is_primary_filter: boolean): string[] {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const containerTypeSelect = document.getElementById(`${id_prefix}-container-types-span`);
        if (containerTypeSelect == null) {
          throw new Error(`${id_prefix}-container-types-span is not found`);
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
    
      private static getCheckedPaperTypes(is_primary_filter: boolean): string[] {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const paperTypeSelect = document.getElementById(`${id_prefix}-paper-types-span`);
        if (paperTypeSelect == null) {
          throw new Error(`${id_prefix}-paper-types-span is not found`);
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
    
      private static getCheckedOtherTypes(is_primary_filter: boolean): string[] {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const otherTypeSelect = document.getElementById(`${id_prefix}-other-types-span`);
        if (otherTypeSelect == null) {
          throw new Error(`${id_prefix}-other-types-span is not found`);
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
    
      public static getCheckedTypes(is_primary_filter: boolean): string[] {
        const checkedContainerTypes = this.getCheckedContainerTypes(is_primary_filter);
        const checkedPaperTypes = this.getCheckedPaperTypes(is_primary_filter);
        const checkedOtherTypes = this.getCheckedOtherTypes(is_primary_filter);
    
        if(checkedContainerTypes.length == 0 && checkedPaperTypes.length == 0 && checkedOtherTypes.length == 0) {
          return ["None"];
        }else if(checkedContainerTypes.length == 1 && checkedPaperTypes.length == 1 && checkedOtherTypes.length == 1 && checkedContainerTypes[0] == "Container-Any" && checkedPaperTypes[0] == "Paper-Any" && checkedOtherTypes[0] == "Other-Any") {
          return [];
        }else{
          return [...checkedContainerTypes, ...checkedPaperTypes, ...checkedOtherTypes];
        }
      }
}