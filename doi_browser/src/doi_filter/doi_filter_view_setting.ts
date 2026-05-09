import { DOIFilterQuery } from "./doi_filter_query";

export class DOIFilterViewSetting {
    public viewMode: "article_list" | "unkonwn" = "article_list";
    public pageNumber: number | null = 0;
    public pageSize: number | null = 100;

    public static buildFromURLParameters(): DOIFilterViewSetting {
        let r = new DOIFilterViewSetting();
        const sp = new URL(location.href).searchParams;
        return r;
    }
    public copy(): DOIFilterViewSetting {
        let r = new DOIFilterViewSetting();
        r.viewMode = this.viewMode;
        r.pageNumber = this.pageNumber;
        r.pageSize = this.pageSize;
        return r;
    }
    public getHash(): string {
        return JSON.stringify(this);
    }
}

