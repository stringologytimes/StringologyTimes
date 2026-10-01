import { DOIRecordCollection } from "../doi_record_collection";

export class FoundRecordSummary {
    public primary_record_count: number = 0;
    public secondary_record_count: number = 0;
    public year_to_id_count_mapper: Map<number, number> = new Map<number, number>();
    public unknown_year_id_count: number = 0;
    public type_to_id_count_mapper: Map<string, number> = new Map<string, number>();
    public idToPrimaryRecordCountMapper: Map<number, number> = new Map<number, number>();
    public idToSecondaryRecordCountMapper: Map<number, number> = new Map<number, number>();

    public getMinimumYear(): number {
        if(this.year_to_id_count_mapper.size > 0){
            return Math.min(...this.year_to_id_count_mapper.keys());
        } else {
            return 0;
        }
    }

    public getMaximumYear(): number {
        if(this.year_to_id_count_mapper.size > 0){
            return Math.max(...this.year_to_id_count_mapper.keys());
        } else {
            return 0;
        }
    }

    
}
