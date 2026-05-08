import { DOIInfoCollection } from "./doi_info";
import { BrowserInfo } from "./browser_info";
import { Render } from "./render/doi_filter_result_render";
import { DOIFilterStandardRender } from "./render/doi_filter_standard_render";
import { DOIFilterInput } from "./doi_filter/doi_filter_input";
import * as EventFunctions from "./event_functions";

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
  browserInfo.currentDOIFilterInput.pageNumber = pageNumber;
  browserInfo.processCurrentDOIFilterInput();
  DOIFilterStandardRender.render(browserInfo.getCurrentDOIFilterPartialResult(), browserInfo.doiInfoCollection!);
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
    browserInfo.doiInfoCollection = await DOIInfoCollection.load("./doi_info_parts");
  }



}
function preprocessURLParameters() {
  if (browserInfo.doiInfoCollection == null) {
    throw new Error("DOIInfoCollection is not loaded");
  }
  // URLパラメーターからページ番号を読み込む
  const urlParams = new URL(location.href).searchParams;
  /*
  const pageParam = urlParams.get('page');
  if (pageParam) {
    const pageNumber = parseInt(pageParam);
    if (!isNaN(pageNumber) && pageNumber >= 0) {
      browserInfo.pageNumber = pageNumber;
    }
  }
  */

  console.log("preprocessURLParameters");
  const currentDOIFilterInput = DOIFilterInput.buildFromURLParameters();
  console.log("currentDOIFilterInput", currentDOIFilterInput);
  browserInfo.initialize(currentDOIFilterInput, browserInfo.doiInfoCollection!);
  console.log("browserInfo.initialize");
  DOIFilterStandardRender.render(browserInfo.getCurrentDOIFilterPartialResult(), browserInfo.doiInfoCollection!);
  // URLパラメーターがある場合は検索を実行
  /*
  if (urlParams.toString().length > 0 && !pageParam) {
  } else {
    // ページ番号だけが指定されている場合も更新
    updatePaginationControls();
  }
  */
}

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

function resetFilter() {
  browserInfo.currentDOIFilterInput = new DOIFilterInput();
  browserInfo.processCurrentDOIFilterInput();
  DOIFilterStandardRender.render(browserInfo.getCurrentDOIFilterPartialResult(), browserInfo.doiInfoCollection!);
}

// グローバルスコープに公開（onchange属性からアクセスできるようにする）
(window as any).filterInputChange = filterInputChange;
(window as any).resetFilter = resetFilter;

async function domFinished() {
  showLoading();

  try {
    await initialize();
    preprocessURLParameters();
    // コレクションのロード直後にも実行
    EventFunctions.process(browserInfo);
    setupButtons();
  } finally {
    hideLoading();
  }
}
document.addEventListener('DOMContentLoaded', domFinished);



