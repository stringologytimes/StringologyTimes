import { DOIInfoCollection } from "./browser";
import { BrowserInfo, DOIInfoSearchInput } from "./browser_info";
import { Render } from "./render";

let browserInfo = new BrowserInfo();

console.log("index.ts loaded");


function setFoundDOIList(list: any[]) {
  browserInfo.foundDOIList = list;
  browserInfo.pageNumber = 0; // 検索結果が変わったら最初のページに戻る
  Render.render(browserInfo);
  updatePaginationControls();
}

function goToPage(pageNumber: number) {
  const totalPages = browserInfo.foundDOIList
    ? Math.ceil(browserInfo.foundDOIList.length / browserInfo.pageSize)
    : 0;
  if (pageNumber >= 0 && pageNumber < totalPages) {
    browserInfo.pageNumber = pageNumber;
    Render.render(browserInfo);
    updatePaginationControls();
  }
}

function goToPreviousPage() {
  if (browserInfo.pageNumber > 0) {
    goToPage(browserInfo.pageNumber - 1);
  }
}

function goToNextPage() {
  const totalPages = browserInfo.foundDOIList
    ? Math.ceil(browserInfo.foundDOIList.length / browserInfo.pageSize)
    : 0;
  if (browserInfo.pageNumber < totalPages - 1) {
    goToPage(browserInfo.pageNumber + 1);
  }
}

function updatePaginationControls() {
  const prevButton = document.getElementById('prevPageButton');
  const nextButton = document.getElementById('nextPageButton');
  const pageInfo = document.getElementById('pageInfo');

  if (!browserInfo.foundDOIList || browserInfo.foundDOIList.length === 0) {
    if (prevButton) (prevButton as HTMLButtonElement).disabled = true;
    if (nextButton) (nextButton as HTMLButtonElement).disabled = true;
    if (pageInfo) pageInfo.textContent = '';
    return;
  }

  const totalPages = Math.ceil(browserInfo.foundDOIList.length / browserInfo.pageSize);
  const currentPage = browserInfo.pageNumber + 1;

  if (prevButton) {
    (prevButton as HTMLButtonElement).disabled = browserInfo.pageNumber === 0;
  }
  if (nextButton) {
    (nextButton as HTMLButtonElement).disabled = browserInfo.pageNumber >= totalPages - 1;
  }
  if (pageInfo) {
    const startIndex = browserInfo.pageNumber * browserInfo.pageSize + 1;
    const endIndex = Math.min(startIndex + browserInfo.pageSize - 1, browserInfo.foundDOIList.length);
    pageInfo.textContent = `ページ ${currentPage}/${totalPages} (${startIndex}-${endIndex} / ${browserInfo.foundDOIList.length}件)`;
  }
}

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
  const pageParam = urlParams.get('page');
  if (pageParam) {
    const pageNumber = parseInt(pageParam);
    if (!isNaN(pageNumber) && pageNumber >= 0) {
      browserInfo.pageNumber = pageNumber;
    }
  }

  // URLパラメーターがある場合は検索を実行
  if (urlParams.toString().length > 0 && !pageParam) {
    const searchInput = DOIInfoSearchInput.buildFromURLParameters();
    const tmp = searchInput.search(browserInfo.doiInfoCollection);
    setFoundDOIList(tmp);
  } else {
    // ページ番号だけが指定されている場合も更新
    updatePaginationControls();
  }
}

// ボタンのイベントリスナーを設定
function setupButtons() {
  const renderAllButton = document.getElementById('renderAllButton');
  if (renderAllButton) {
    renderAllButton.addEventListener('click', () => {
      Render.renderAll(browserInfo);
      updatePaginationControls();
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

async function domFinished() {
  showLoading();
  
  try {
    await initialize();
    preprocessURLParameters();
    // コレクションのロード直後にも実行
    if (browserInfo.doiInfoCollection) {
      browserInfo.doiInfoCollection.intitalizeFilterOptions();
    }
    setupButtons();
  } finally {
    hideLoading();
  }
}
document.addEventListener('DOMContentLoaded', domFinished);



