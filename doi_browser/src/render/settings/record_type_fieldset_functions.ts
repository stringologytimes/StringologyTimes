import { containerTypeList, otherTypeList, paperTypeList, topContainerTypeList } from "../../doi_record";

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

        typeListContainerSpan.innerHTML = "";
        typeListPaperSpan.innerHTML = "";
        typeListOtherSpan.innerHTML = "";

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

    public static updateRecordTypes(is_primary_filter: boolean, type_to_id_count_mapper: Map<string, number>) {
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
}