import { SearchResultViewSettings } from "../../doi_filter/search_result_view_settings";
import { ViewModeType } from "../../doi_filter/search_result_view_settings";

function getMaxPageNumber(viewSetting: SearchResultViewSettings, foundRecordCount: number): number {
    if(foundRecordCount == 0){
        return 0;
    }else{
        return Math.ceil(foundRecordCount / viewSetting.pageSize!) - 1;
    }
}


function setPageNumberSelectHTMLElement(selectedValue: number, maxPageNumber: number) {
    const selectElement = document.getElementById("view-setting:page-number-select");
    if (selectElement && selectElement instanceof HTMLSelectElement) {
        selectElement.innerHTML = "";
        for(let i = 0; i <= maxPageNumber; i++){
            const option = document.createElement("option");
            option.value = i.toString();
            option.textContent = (i+1).toString();
            if(i == selectedValue){
                option.selected = true;
            }
            selectElement.appendChild(option);
        }
    }else{
        throw new Error("selectElement is not found");
    }
}

function setPageSizeSelectHTMLElement(selectedValue: number) {
    const selectElement = document.getElementById("view-setting:page-size-select");
    if (selectElement && selectElement instanceof HTMLSelectElement) {
        selectElement.innerHTML = "";
        const options = [10, 20, 30, 40, 50, 100, 200, 500, 1000];
        options.forEach((optionValue, index) => {
            const option = document.createElement("option");
            option.value = optionValue.toString();
            option.textContent = optionValue.toString();
            if(optionValue == selectedValue){
                option.selected = true;
            }
            selectElement.appendChild(option);
        });
    }else{
        throw new Error("selectElement is not found");
    }
}


export function renderViewSettingBox(filterResult: SearchResultViewSettings, foundRecordCount: number) {
    setPageNumberSelectHTMLElement(filterResult.pageNumber!, getMaxPageNumber(filterResult, foundRecordCount));
    setPageSizeSelectHTMLElement(filterResult.pageSize!);
}

  