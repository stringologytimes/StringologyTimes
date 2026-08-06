export class RankFieldsetFunctions {
    public static updateRankBox(is_primary_filter: boolean, primary_record_count: number, secondary_record_count: number) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
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
}
