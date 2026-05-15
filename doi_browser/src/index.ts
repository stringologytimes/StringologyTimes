import { DOIInfoCollection } from "./doi_info";
import { BrowserInfo } from "./browser_info";
import { DOIFilterStandardRender } from "./render/doi_filter_standard_render";
import * as EventFunctions from "./event_functions";
import { DOIFilter } from "./doi_filter/doi_filter";

let browserInfo = new BrowserInfo();
(window as any).browserInfo = browserInfo;

console.log("index.ts loaded");





/*
function setFoundDOIList(list: any[]) {
  browserInfo.foundDOIList = list;
  browserInfo.pageNumber = 0; // 検索結果が変わったら最初のページに戻る
  Render.render(browserInfo);
  EventFunctions.updatePaginationControls(browserInfo);
}
*/

function goToPage(pageNumber: number) {
  browserInfo.currentDOIFilter.viewSetting.pageNumber = pageNumber;
  browserInfo.processCurrentDOIFilterInput();
  DOIFilterStandardRender.render(browserInfo.getCurrentDOIFilterResult(), browserInfo.getCurrentDOIFilterWithViewSetting().viewSetting.getItemIndex(), browserInfo.getCurrentDOIFilterWithViewSetting().viewSetting.pageSize!, browserInfo.doiInfoCollection!);
  EventFunctions.updatePaginationControls(browserInfo);
}

/*
function goToPreviousPage() {
  if (browserInfo.doiFilterInputNumber > 0) {
    goToPage(browserInfo.doiFilterInputNumber - 1);
  }
}

function goToNextPage() {
  goToNextPage(browserInfo.doiFilterInputNumber + 1);

  const totalPages = browserInfo.foundDOIList
    ? Math.ceil(browserInfo.foundDOIList.length / browserInfo.pageSize)
    : 0;
  if (browserInfo.pageNumber < totalPages - 1) {
    goToPage(browserInfo.pageNumber + 1);
  }
}
*/

async function initialize() {
  await new Promise(resolve => setTimeout(resolve, 1000));
  if (browserInfo.doiInfoCollection == null) {
    browserInfo.doiInfoCollection = await DOIInfoCollection.load("./lightweight_doi_info");
  }



}
/*
function preprocessURLParameters() {
  if (browserInfo.doiInfoCollection == null) {
    throw new Error("DOIInfoCollection is not loaded");
  }
  // URLパラメーターからページ番号を読み込む
  const urlParams = new URL(location.href).searchParams;

  console.log("preprocessURLParameters");
  const currentDOIFilterWithViewSetting = DOIFilter.buildFromURLParameters();
  console.log("currentDOIFilterWithViewSetting", currentDOIFilterWithViewSetting);
  browserInfo.initialize(browserInfo.doiInfoCollection!);
  browserInfo.setCurrentDOIFilterWithViewSetting(currentDOIFilterWithViewSetting);
  browserInfo.processCurrentDOIFilterInput();

  console.log("browserInfo.initialize");
  DOIFilterStandardRender.render(browserInfo.getCurrentDOIFilterResult(), browserInfo.getCurrentDOIFilterWithViewSetting().viewSetting.getItemIndex(), browserInfo.getCurrentDOIFilterWithViewSetting().viewSetting.pageSize!, browserInfo.doiInfoCollection!);
}
*/

// ボタンのイベントリスナーを設定
function setupButtons() {
  /*
  const renderAllButton = document.getElementById('renderAllButton');
  if (renderAllButton) {
    renderAllButton.addEventListener('click', () => {
      Render.renderAll(browserInfo);
      EventFunctions.updatePaginationControls(browserInfo);
    });
  }

  const prevButton = document.getElementById('prevPageButton');
  if (prevButton) {
    prevButton.addEventListener('click', goToPreviousPage);
  }

  const nextButton = document.getElementById('nextPageButton');
  if (nextButton) {
    nextButton.addEventListener('click', goToNextPage);
  }
  */
}

function showLoading() {
  const loadingOverlay = document.getElementById('loading-overlay');
  if (loadingOverlay) {
    loadingOverlay.classList.add('show');
  }
}

function hideLoading() {
  const loadingOverlay = document.getElementById('loading-overlay');
  if (loadingOverlay) {
    loadingOverlay.classList.remove('show');
  }
}

function filterInputChange(inputElementName: string) {
  EventFunctions.filterInputChange(inputElementName, browserInfo);
}

function viewSettingInputChange(inputElementName: string) {
  EventFunctions.ViewSettingInputChange(inputElementName, browserInfo);
}

function containerTitleLiElementClick(containerTitle: string) {
  browserInfo.currentDOIFilter.query.container_title = containerTitle;
  browserInfo.currentDOIFilter.viewSetting.pageNumber = 0;
  browserInfo.currentDOIFilter.viewSetting.viewMode = "article_list";
  browserInfo.processCurrentDOIFilterInput();
  browserInfo.render();
}

function resetFilter() {
  browserInfo.currentDOIFilter = new DOIFilter();
  browserInfo.processCurrentDOIFilterInput();
  browserInfo.render();
}

// グローバルスコープに公開（onchange属性からアクセスできるようにする）
(window as any).filterInputChange = filterInputChange;
(window as any).resetFilter = resetFilter;
(window as any).viewSettingInputChange = viewSettingInputChange;
(window as any).containerTitleLiElementClick = containerTitleLiElementClick;
async function domFinished() {
  showLoading();

  try {
    await initialize();
    browserInfo.initialize(browserInfo.doiInfoCollection!);

    // コレクションのロード直後にも実行
    EventFunctions.process(browserInfo);
    setupButtons();
  } finally {
    hideLoading();
  }
}
document.addEventListener('DOMContentLoaded', domFinished);



