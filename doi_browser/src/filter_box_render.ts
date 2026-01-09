import { BrowserInfo } from "./browser_info";
import { DOIInfoCollectionFilter } from "./browser";

/*
function getUniqueStringSet(items: string[]): string[] {
  const uniqueSet = new Set<string>();
  items.forEach(item => {
    uniqueSet.add(item);
  });
  return Array.from(uniqueSet);
}
*/

export function renderFilterBox(browserInfo: BrowserInfo) {
  const doiInfoCollection = browserInfo.doiInfoCollection;
  if (doiInfoCollection == null) {
    throw new Error("DOIInfoCollection is not loaded");
  }
  else {
    const doiIDs = Array.from({length: doiInfoCollection.length()}, (_, index) => index);
    const foundDOIIDs : number[] = browserInfo.doiInfoSearchInput.search(new DOIInfoCollectionFilter(doiIDs, doiInfoCollection), doiInfoCollection);
    const currentDOIInfoCollectionFilter = new DOIInfoCollectionFilter(foundDOIIDs, doiInfoCollection);
    const doiNumberFilterSet = new Set<number>(foundDOIIDs);

    



    const type_list = currentDOIInfoCollectionFilter.getTypes();
    type_list.sort();

    const typeSelect = document.getElementById("type-select");
    if (typeSelect) {
      typeSelect.innerHTML = "";
      const defaultOption = document.createElement("option");
      defaultOption.value = "";
      defaultOption.textContent = "Any";
      typeSelect.appendChild(defaultOption);
      type_list.forEach(type => {
        const option = document.createElement("option");
        const doiCount = currentDOIInfoCollectionFilter.searchByType(type, doiNumberFilterSet, doiInfoCollection).length;
        option.value = type;
        option.textContent = `${type} (${doiCount})`;
        typeSelect.appendChild(option);
      });
    }

    const containerTitleSelect = document.getElementById("container-title-select");
    const containerTitle_list = currentDOIInfoCollectionFilter.getContainerTitles();

    if (containerTitleSelect) {
      containerTitleSelect.innerHTML = "";
      const defaultOption = document.createElement("option");
      defaultOption.value = "";
      defaultOption.textContent = "Any";
      containerTitleSelect.appendChild(defaultOption);
      containerTitle_list.forEach(containerTitle => {
        const option = document.createElement("option");
        const doiCount = currentDOIInfoCollectionFilter.searchByContainerTitle(containerTitle, doiNumberFilterSet, doiInfoCollection).length;
        option.value = containerTitle;
        option.textContent = `${containerTitle} (${doiCount})`;
        containerTitleSelect.appendChild(option);
      });
    }

    const yearFromSelect = document.getElementById("year-from-select");
    const minYear = currentDOIInfoCollectionFilter.getMinimumYear();
    const maxYear = currentDOIInfoCollectionFilter.getMaxmumYear();
    if (yearFromSelect) {
      yearFromSelect.innerHTML = "";
      const defaultOption = document.createElement("option");
      defaultOption.value = "";
      defaultOption.textContent = "Any";
      yearFromSelect.appendChild(defaultOption);
      for (let year = minYear; year <= maxYear; year++) {
        const option = document.createElement("option");
        const doiCount = currentDOIInfoCollectionFilter.searchByYear(year, year, doiNumberFilterSet, doiInfoCollection).length;
        option.value = year.toString();
        option.textContent = `${year} (${doiCount})`;
        option.textContent = year.toString();
        yearFromSelect.appendChild(option);
      }
    }
    const yearToSelect = document.getElementById("year-to-select");
    if (yearToSelect) {
      yearToSelect.innerHTML = "";
      const defaultOption = document.createElement("option");
      defaultOption.value = "";
      defaultOption.textContent = "Any";
      yearToSelect.appendChild(defaultOption);
      for (let year = minYear; year <= maxYear; year++) {
        const option = document.createElement("option");
        const doiCount = currentDOIInfoCollectionFilter.searchByYear(year, year, doiNumberFilterSet, doiInfoCollection).length;
        option.value = year.toString();
        option.textContent = `${year} (${doiCount})`;
        option.textContent = year.toString();
        yearToSelect.appendChild(option);
      }
    }
  }

}