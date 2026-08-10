import { DOIRecordCollection } from "../../doi_record_collection";
import { RecordTypeFieldsetFunctions } from "./fieldset/record_type_fieldset_functions";
import { YearFieldsetFunctions } from "./fieldset/year_fieldset_functions";
import { RankFieldsetFunctions } from "./fieldset/rank_fieldset_functions";
import { ContainerDOIFieldsetFunctions } from "./fieldset/container_doi_fieldset_functions";
//import { SearchFilterBoxFunctions } from "./fieldset/search_filter_box_functions";


//let topContainerCategories: string[] = ["Journal", "Proceedings", "Preprint"];
//let containerSelect2Items: [string, string][] = [];

/*
export class PrimarySearchFilterRender {
  public static initialize(doiRecordCollection: DOIRecordCollection): void {
   
    RecordTypeFieldsetFunctions.initializeRecordTypes(true, doiRecordCollection.recordSummary.type_to_id_count_mapper);
    YearFieldsetFunctions.initializeYearBox(true, doiRecordCollection.recordSummary.year_to_id_count_mapper, doiRecordCollection.recordSummary.unknown_year_id_count);
    RankFieldsetFunctions.updateRankBox(true, doiRecordCollection.recordSummary.primary_record_count, doiRecordCollection.recordSummary.secondary_record_count);
    ContainerDOIFieldsetFunctions.initializeContainerBox(true, doiRecordCollection, false, doiRecordCollection.recordSummary.idToPrimaryRecordCountMapper, doiRecordCollection.recordSummary.idToSecondaryRecordCountMapper);

  }

}
*/
