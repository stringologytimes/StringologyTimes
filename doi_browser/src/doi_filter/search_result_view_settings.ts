export type ViewModeType = "article_list" | "container_title_list" | "series_title_list" | "group_render" | "unkonwn";

const defaultPageNumber = 0;
const defaultPageSize = 100;
const defaultMode = "article_list";

export class SearchResultViewSettings {
    public mode: ViewModeType = defaultMode;
    public pageNumber: number | null = defaultPageNumber;
    public pageSize: number | null = defaultPageSize;

    public static buildFromURLParameters(): SearchResultViewSettings {
        let r = new SearchResultViewSettings();
        const sp = new URL(location.href).searchParams;

        const viewMode = sp.get("srvs-mode");
        if(viewMode != null) {
            r.mode = viewMode as ViewModeType;
        }

        const pageNumber = sp.get("srvs-page-number");
        if(pageNumber != null) {
            r.pageNumber = parseInt(pageNumber);
        }

        const pageSize = sp.get("srvs-page-size");
        if(pageSize != null) {
            r.pageSize = parseInt(pageSize);
        }

        return r;
    }

    public static getURLParameterKeys(): string[] {
        return ["srvs-mode", "srvs-page-number", "srvs-page-size"];
    }

    public convertToURLParameters(): [string, string][] {
        let r: [string, string][] = [];
        if(this.mode != defaultMode) {
            r.push(["srvs-mode", this.mode]);
        }
        if(this.pageNumber != defaultPageNumber) {
            r.push(["srvs-page-number", this.pageNumber!.toString()]);
        }
        if(this.pageSize != defaultPageSize) {
            r.push(["srvs-page-size", this.pageSize!.toString()]);
        }
        return r;
    }

    public copy(): SearchResultViewSettings {
        let r = new SearchResultViewSettings();
        r.mode = this.mode;
        r.pageNumber = this.pageNumber;
        r.pageSize = this.pageSize;
        
        return r;
    }
    public getHash(): string {
        return this.convertToURLParameters().map(p => p[0] + "=" + p[1]).join("&");
    }
    public getItemIndex(): number {
        return this.pageNumber! * this.pageSize!;
    }

    public static convertHTMLElementToInstance() : SearchResultViewSettings {
        let r = new SearchResultViewSettings();
        /*
        const modeElement = document.getElementById("view-mode-list-div") as HTMLSpanElement;
        if(modeElement != null) {
            r.mode = modeElement.textContent as ViewModeType;
        }
        */
        const pageNumberElement = document.getElementById("view-setting:page-number-select") as HTMLSelectElement;
        if(pageNumberElement != null) {
            r.pageNumber = parseInt(pageNumberElement.value);
        }
        const pageSizeElement = document.getElementById("view-setting:page-size-select") as HTMLSelectElement;
        if(pageSizeElement != null) {
            r.pageSize = parseInt(pageSizeElement.value);
        }
        return r;
    }

    /*
    public static buildFromJSON(json: string): SearchResultViewSettings {
        return JSON.parse(json);
    }
    */
}

