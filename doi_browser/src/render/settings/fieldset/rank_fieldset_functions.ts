export class RankFieldsetFunctions {
    public static updateRankBox(is_primary_filter: boolean, primary_record_count: number, secondary_record_count: number, excludedStatus: string[]) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";

        const primaryRecordCheckbox: HTMLInputElement = document.getElementById(`${id_prefix}-primary-record-checkbox`) as HTMLInputElement;
        if (primaryRecordCheckbox == null) {
          throw new Error(`${id_prefix}-primary-record-checkbox is not found`);
        }
        primaryRecordCheckbox.checked = !excludedStatus.includes("primary");

        const secondaryRecordCheckbox: HTMLInputElement = document.getElementById(`${id_prefix}-secondary-record-checkbox`) as HTMLInputElement;
        if (secondaryRecordCheckbox == null) {
          throw new Error(`${id_prefix}-secondary-record-checkbox is not found`);
        }
        secondaryRecordCheckbox.checked = !excludedStatus.includes("secondary");


        const primaryRecordLabel = document.getElementById(`${id_prefix}-primary-record-label`) as HTMLElement;
        if (primaryRecordLabel == null) {
            throw new Error(`${id_prefix}-primary-record-label is not found`);
        }else{
            primaryRecordLabel.textContent = `Primary record (${primary_record_count})`;
        }
        const secondaryRecordLabel = document.getElementById(`${id_prefix}-secondary-record-label`) as HTMLElement;
        if (secondaryRecordLabel == null) {
            throw new Error(`${id_prefix}-secondary-record-label is not found`);
        }else{
            secondaryRecordLabel.textContent = `Secondary record (${secondary_record_count})`;
        }
    }

    public static getExcludedStatus(is_primary_filter: boolean): string[] {
        const id_prefix = is_primary_filter ? "psf" : "ssf";

        const excludedStatus = [];
        const primaryRecordCheckbox: HTMLInputElement = document.getElementById(`${id_prefix}-primary-record-checkbox`) as HTMLInputElement;
        if (primaryRecordCheckbox == null) {
          throw new Error(`${id_prefix}-primary-record-checkbox is not found`);
        }
        const b1 = primaryRecordCheckbox.checked;
        if (!b1) {
          excludedStatus.push("primary");
        }
    
        const secondaryRecordCheckbox: HTMLInputElement = document.getElementById(`${id_prefix}-secondary-record-checkbox`) as HTMLInputElement;
        if (secondaryRecordCheckbox == null) {
          throw new Error(`${id_prefix}-secondary-record-checkbox is not found`);
        }
        const b2 = secondaryRecordCheckbox.checked;
        if (!b2) {
          excludedStatus.push("secondary");
        }
        return excludedStatus;
      }

      public static convertInputToURLParameters(isPrimaryFilter: boolean): [string, string][] {
        const prefix = isPrimaryFilter ? "psf-" : "ssf-";
        const r: [string, string][] = [];
        const excludedStatus = this.getExcludedStatus(isPrimaryFilter);
        excludedStatus.forEach(status => {
          r.push([prefix + "excluded-status", status]);
        });
        return r;
      }
    
}
