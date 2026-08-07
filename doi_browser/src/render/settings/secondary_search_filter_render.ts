import { DOIRecordCollection } from "../../doi_record_collection";
import { RecordTypeFieldsetFunctions } from "./fieldset/record_type_fieldset_functions";
import { YearFieldsetFunctions } from "./fieldset/year_fieldset_functions";
import { RankFieldsetFunctions } from "./fieldset/rank_fieldset_functions";
import { ContainerDOIFieldsetFunctions } from "./fieldset/container_doi_fieldset_functions";
import { FoundRecordSummary } from "../../doi_filter/found_record_summary";



export class SecondarySearchFilterRender {
  public static initialize(initial_record_ids: number[], found_record_summary: FoundRecordSummary, doiRecordCollection: DOIRecordCollection): void {
    RecordTypeFieldsetFunctions.initializeRecordTypes(false, found_record_summary.type_to_id_count_mapper);
    YearFieldsetFunctions.initializeYearBox(false, found_record_summary.year_to_id_count_mapper, found_record_summary.unknown_year_id_count);
    RankFieldsetFunctions.updateRankBox(false, found_record_summary.primary_record_count, found_record_summary.secondary_record_count);
    ContainerDOIFieldsetFunctions.initializeContainerBox(false, doiRecordCollection, true, found_record_summary.idToPrimaryRecordCountMapper, found_record_summary.idToSecondaryRecordCountMapper);
    //this.initializeYearBox(doiRecordCollection);
  }
}