import { info } from "console";
import { DOIRecordCollection } from "../../../doi_record_collection";

export type SortOrderType = "default" | "newest-first" | "oldest-first" | "title-a-z" | "title-z-a";


export class SearchResultSortOrder {
    public primarySortOrder: SortOrderType = "default";

    public static convertInputToURLParameters(): [string, string][] {
        const r: [string, string][] = [];
        const value =
        document.querySelector<HTMLInputElement>(
            'input[name="primary-sort-order-radio"]:checked'
        )?.value;

        if (value != null && value != "default") {
            r.push(["primary-sort-order", value]);
        }
        return r;
    }

    public static getURLParameterKeys(): string[] {
        return ["primary-sort-order"];
    }
    public render() {
        const radioButtons = document.querySelectorAll<HTMLInputElement>('input[name="primary-sort-order-radio"]');
        radioButtons.forEach(radioButton => {
            if(radioButton.value == this.primarySortOrder) {
                radioButton.checked = true;
            }else{
                radioButton.checked = false;
            }
        });
    }
    

    public static buildFromURLParameters(): SearchResultSortOrder {
        let r = new SearchResultSortOrder();
        const sp = new URL(location.href).searchParams;

        const primarySortOrder = sp.get("primary-sort-order");
        if(primarySortOrder != null) {
            r.primarySortOrder = primarySortOrder as SortOrderType;
        }
        return r;
    }

    public getHash(): string {
        return `primary-sort-order-${this.primarySortOrder}`;
    }

    public sort(foundRecordIDs: number[], collection: DOIRecordCollection): void {
        if(this.primarySortOrder == "default") {

        }else if(this.primarySortOrder == "newest-first") {
            foundRecordIDs.sort((a, b) => {
                const infoA = collection.lightweightDOIRecords[a];
                const infoB = collection.lightweightDOIRecords[b];
                if(infoA.isUnknownYear() && infoB.isUnknownYear()) {
                    return a - b
                }
                else if(infoA.isUnknownYear()) {
                    return 1;
                }else if(infoB.isUnknownYear()) {
                    return -1;
                }else{
                    if(infoA.year == infoB.year) {
                        if(infoA.isUnknownMonth() && infoB.isUnknownMonth()) {
                            return a - b;
                        }else if(infoA.isUnknownMonth()) {
                            return 1;
                        }else if(infoB.isUnknownMonth()) {
                            return -1;
                        }else{
                            return infoB.month - infoA.month;
                        }
                    }else{
                        return infoB.year - infoA.year;
                    }
                }
            });
        }else if(this.primarySortOrder == "oldest-first") {
            foundRecordIDs.sort((a, b) => {
                const infoA = collection.lightweightDOIRecords[a];
                const infoB = collection.lightweightDOIRecords[b];
                if(infoA.isUnknownYear() && infoB.isUnknownYear()) {
                    return a - b
                }
                else if(infoA.isUnknownYear()) {
                    return 1;
                }else if(infoB.isUnknownYear()) {
                    return -1;
                }else{
                    if(infoA.year == infoB.year) {
                        if(infoA.isUnknownMonth() && infoB.isUnknownMonth()) {
                            return a - b;
                        }else if(infoA.isUnknownMonth()) {
                            return 1;
                        }else if(infoB.isUnknownMonth()) {
                            return -1;
                        }else{
                            return infoA.month - infoB.month;
                        }
                    }else{
                        return infoA.year - infoB.year;
                    }
                }
            });

        }else if(this.primarySortOrder == "title-a-z") {
            foundRecordIDs.sort((a, b) => {
                const infoA = collection.lightweightDOIRecords[a];
                const infoB = collection.lightweightDOIRecords[b];
                if(infoA.title == infoB.title) {
                    return a - b;
                }else{
                    return infoA.title.localeCompare(infoB.title);
                }
            });
        }else if(this.primarySortOrder == "title-z-a") {
            foundRecordIDs.sort((a, b) => {
                const infoA = collection.lightweightDOIRecords[a];
                const infoB = collection.lightweightDOIRecords[b];
                if(infoA.title == infoB.title) {
                    return a - b;
                }else{
                    return infoB.title.localeCompare(infoA.title);
                }
            });
        }

    }



}