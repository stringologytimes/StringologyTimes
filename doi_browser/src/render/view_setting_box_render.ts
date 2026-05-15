import { DOIFilterViewSetting } from "../doi_filter/doi_filter_view_setting";
import { ViewModeType } from "../doi_filter/doi_filter_view_setting";
import { SummaryInfo } from "../doi_filter/summary_info";
import { setRadioBoxes } from "./doi_filter_box_render";

function getMaxPageNumber(viewSetting: DOIFilterViewSetting, summary_info: SummaryInfo): number {
    if(viewSetting.viewMode == "article_list"){
        if(summary_info.doiCount == 0){
            return 0;
        }else{
            return Math.ceil(summary_info.doiCount / viewSetting.pageSize!) - 1;
        }
    }else if(viewSetting.viewMode == "container_title_list"){
        if(summary_info.containerTitleCountList.length == 0){
            return 0;
        }else{
            return Math.ceil(summary_info.containerTitleCountList.length / viewSetting.pageSize!) - 1;
        }
    }else{
        throw new Error("Unknown view mode");
    }
}

function setModeSelectHTMLElement(selectedValue: ViewModeType) {
    const viewModeList = ["article_list", "container_title_list"];
    const viewModeValues = ["article_list", "container_title_list"];
    setRadioBoxes("view-mode-list-div", "view-mode-template", selectedValue, viewModeList, viewModeValues);

    /*

    const selectElement = document.getElementById("view-setting:mode-select");
    if (selectElement && selectElement instanceof HTMLSelectElement) {
        selectElement.innerHTML = "";
        //const defaultOption = document.createElement("option");
        //defaultOption.value = "dont-care";
        //defaultOption.textContent = "article_list";
        //selectElement.appendChild(defaultOption);
        const options = ["article_list", "container_title_list"];
      
      
        options.forEach((optionValue, index) => {
          const option = document.createElement("option");
          option.value = optionValue;
          option.textContent = `${optionValue}`;
      
          if (optionValue == selectedValue) {
            option.selected = true;
          }
          selectElement.appendChild(option);
        });
    
    }else{
        throw new Error("selectElement is not found");
    }
    */

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


export function renderViewSettingBox(filterResult: DOIFilterViewSetting, summary_info: SummaryInfo) {
    setModeSelectHTMLElement(filterResult.viewMode);
    setPageNumberSelectHTMLElement(filterResult.pageNumber!, getMaxPageNumber(filterResult, summary_info));
    setPageSizeSelectHTMLElement(filterResult.pageSize!);
}
  