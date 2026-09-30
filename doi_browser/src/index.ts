import { DOIRecordCollection } from "./doi_record_collection";
import { BrowserInfo } from "./browser_info";
import * as EventFunctions from "./event_functions";
import { hideLoading, showLoading } from "./loading_overlay";
import { DOIRecordDetailsTemplate } from "./render/search_result_render/templates/doi_record_details_template";

let browserInfo = new BrowserInfo();
(window as any).browserInfo = browserInfo;

console.info("index.ts loaded");



async function initialize() {
  await new Promise(resolve => setTimeout(resolve, 1000));
  if (browserInfo.doiInfoCollection == null) {
    browserInfo.doiInfoCollection = await DOIRecordCollection.load("./lightweight_doi_info");
  }



}



async function primarySearchFilterChange(inputElementName: string) {
  await EventFunctions.primarySearchFilterChange(inputElementName, browserInfo);
}
async function secondarySearchFilterChange(inputElementName: string) {
  await EventFunctions.secondarySearchFilterChange(inputElementName, browserInfo);
}

async function clickResetButtonOfPrimarySearchFilterBox(){
  await EventFunctions.clickResetButtonOfPrimarySearchFilterBox(browserInfo);
}
async function clickResetButtonOfSecondarySearchFilterBox(){
  await EventFunctions.clickResetButtonOfSecondarySearchFilterBox(browserInfo);
}

async function sortOrderInputChange(inputElementName: string) {
  await EventFunctions.sortOrderInputChange(inputElementName, browserInfo);
}

async function clickPrimarySearchFilterButton(){
  EventFunctions.clickPrimarySearchFilterButton(browserInfo);
  //await EventFunctions.process(browserInfo);
}

function viewSettingInputChange(inputElementName: string) {
  EventFunctions.ViewSettingInputChange(inputElementName, browserInfo);
}

function openDetailsDialog(e: Event) {
  e.preventDefault();
  const targetElement = e.target as HTMLElement;
  const doiIDStr = targetElement.getAttribute("data-doi-id");
  const doiID = parseInt(doiIDStr!);
  const dialog = document.getElementById('details-dialog') as HTMLDialogElement;

  DOIRecordDetailsTemplate.renderDOIRecordDetails(dialog, doiID, browserInfo.doiInfoCollection!);

  //const scrollX = window.scrollX;
  //const scrollY = window.scrollY;
  
  dialog.showModal();
  
  //window.scrollTo(scrollX, scrollY);

}
function closeDetailsDialog() {
  const dialog = document.getElementById('details-dialog') as HTMLDialogElement;
  dialog.close();
}

function addDOIToDebugList(event: Event) {
  const button = event.currentTarget as HTMLButtonElement;
  const doi = button.dataset.doi;
  const debugModeListBox = document.getElementById("debug-mode-listbox") as HTMLSelectElement | null;
  const downloadButton = document.getElementById("download-doi-list-button") as HTMLButtonElement | null;

  if (doi == null || debugModeListBox == null || downloadButton == null) {
    throw new Error("DOI or debug mode controls are not found");
  }

  const existingOption = Array.from(debugModeListBox.options).find(option => option.value === doi);
  if (existingOption != null) {
    existingOption.selected = true;
    downloadButton.disabled = false;
    return;
  }

  const option = new Option(doi, doi, false, true);
  debugModeListBox.add(option);
  downloadButton.disabled = false;
}

function downloadDOIList() {
  const debugModeListBox = document.getElementById("debug-mode-listbox") as HTMLSelectElement | null;
  if (debugModeListBox == null) {
    throw new Error("Debug mode list box is not found");
  }

  const dois = Array.from(debugModeListBox.options, option => option.value);
  if (dois.length === 0) {
    return;
  }

  const escapeTSVValue = (value: string) => {
    return /[\t\r\n"]/.test(value) ? `"${value.replace(/"/g, '""')}"` : value;
  };
  const tsv = ["DOI", ...dois.map(escapeTSVValue)].join("\n") + "\n";
  const blob = new Blob([tsv], { type: "text/tab-separated-values;charset=utf-8" });
  const downloadURL = URL.createObjectURL(blob);
  const downloadLink = document.createElement("a");
  downloadLink.href = downloadURL;
  downloadLink.download = "dois.tsv";
  downloadLink.hidden = true;
  document.body.appendChild(downloadLink);
  downloadLink.click();
  downloadLink.remove();
  window.setTimeout(() => URL.revokeObjectURL(downloadURL), 0);
}

function resetDOIList() {
  const debugModeListBox = document.getElementById("debug-mode-listbox") as HTMLSelectElement | null;
  const downloadButton = document.getElementById("download-doi-list-button") as HTMLButtonElement | null;

  if (debugModeListBox == null || downloadButton == null) {
    throw new Error("Debug mode controls are not found");
  }

  debugModeListBox.replaceChildren();
  downloadButton.disabled = true;
}

function initializeDebugModeListBox() {
  const debugModeCheckbox = document.getElementById("opt-debug-mode-checkbox") as HTMLInputElement | null;
  const debugModeListBoxContainer = document.getElementById("debug-mode-listbox-container");

  if (debugModeCheckbox == null || debugModeListBoxContainer == null) {
    throw new Error("Debug mode controls are not found");
  }

  const updateVisibility = () => {
    const debugModeEnabled = debugModeCheckbox.checked;
    debugModeListBoxContainer.hidden = !debugModeEnabled;
    document.body.classList.toggle("debug-mode", debugModeEnabled);
  };

  debugModeCheckbox.addEventListener("change", updateVisibility);
  updateVisibility();
}


function containerTitleLiElementClick(containerTitle: string) {
  /*
  browserInfo.currentDOIFilter.query.container_title = containerTitle;
  browserInfo.currentDOIFilter.viewSetting.pageNumber = 0;
  browserInfo.currentDOIFilter.viewSetting.viewMode = "article_list";
  browserInfo.processCurrentDOIFilterInput();
  browserInfo.render();
  */
}

/*
function resetFilter() {
  const url = new URL(window.location.href);
  url.search = "";
  history.pushState({}, "", url);
  void EventFunctions.process(browserInfo);
}

function changeParameter(parameterName: string, parameterValue: string) {
  const url = new URL(window.location.href);
  url.searchParams.set(parameterName, parameterValue);
  url.searchParams.set("page_number", "0");
  url.searchParams.set("view_mode", "article_list");
  history.pushState({}, "", url);
  void EventFunctions.process(browserInfo);
}
function changeParameters(parameterList: [string, string][]) {
  const url = new URL(window.location.href);
  parameterList.forEach(([parameterName, parameterValue]) => {
    url.searchParams.set(parameterName, parameterValue);
  });
  history.pushState({}, "", url);
  void EventFunctions.process(browserInfo);
}



function initializeParameter(parameterList: [string, string][]) {
  const url = new URL(window.location.href);
  url.search = "";
  parameterList.forEach(([parameterName, parameterValue]) => {
    url.searchParams.set(parameterName, parameterValue);
  });

  history.pushState({}, "", url);
  void EventFunctions.process(browserInfo);
}
*/



// グローバルスコープに公開（onchange属性からアクセスできるようにする）
//(window as any).filterInputChange = filterInputChange;
(window as any).primarySearchFilterChange = primarySearchFilterChange;
(window as any).secondarySearchFilterChange = secondarySearchFilterChange;
//(window as any).resetFilter = resetFilter;
(window as any).viewSettingInputChange = viewSettingInputChange;
(window as any).containerTitleLiElementClick = containerTitleLiElementClick;
//(window as any).changeParameter = changeParameter;
//(window as any).changeParameters = changeParameters;
//(window as any).initializeParameter = initializeParameter;
(window as any).clickPrimarySearchFilterButton = clickPrimarySearchFilterButton;
(window as any).clickResetButtonOfPrimarySearchFilterBox = clickResetButtonOfPrimarySearchFilterBox;
(window as any).clickResetButtonOfSecondarySearchFilterBox = clickResetButtonOfSecondarySearchFilterBox;
(window as any).sortOrderInputChange = sortOrderInputChange;
(window as any).openDetailsDialog = openDetailsDialog;
(window as any).closeDetailsDialog = closeDetailsDialog;
(window as any).addDOIToDebugList = addDOIToDebugList;
(window as any).downloadDOIList = downloadDOIList;
(window as any).resetDOIList = resetDOIList;
async function domFinished() {
  initializeDebugModeListBox();
  showLoading("Loading...");

  try {
    await initialize();
    browserInfo.initialize(browserInfo.doiInfoCollection!);

    
    /*
    window.addEventListener("popstate", (_event) => {
      void EventFunctions.buildFromURLParameters(browserInfo);
    });
    */
  

    //setupButtons();

  } finally {
    hideLoading();
  }

  // コレクションのロード直後にも実行（検索中は Searching... オーバーレイを表示）
  //await EventFunctions.buildFromURLParameters(browserInfo);
}
document.addEventListener('DOMContentLoaded', domFinished);


window.addEventListener("popstate", () => {
  browserInfo.rebuildFromURLParameters(true, true, true, true);
});
