import { DOIFilterInput } from "./doi_filter_input";

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

export class DOIFilterWithViewSetting {
    public doiFilterInput: DOIFilterInput = new DOIFilterInput();
    public doiFilterViewSetting: DOIFilterViewSetting = new DOIFilterViewSetting();

    public static buildFromURLParameters(): DOIFilterWithViewSetting {        
        let r = new DOIFilterWithViewSetting();
        r.doiFilterInput = DOIFilterInput.buildFromURLParameters();
        r.doiFilterViewSetting = DOIFilterViewSetting.buildFromURLParameters();
       return r;
    }

    public copy(): DOIFilterWithViewSetting {
        let r = new DOIFilterWithViewSetting();
        r.doiFilterInput = this.doiFilterInput.copy();
        r.doiFilterViewSetting = this.doiFilterViewSetting.copy();
        return r;
    }


    public getHash(): string {
        return this.doiFilterInput.getHash() + this.doiFilterViewSetting.getHash();
    }
}