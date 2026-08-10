import { RecordTypeFieldsetFunctions } from "./record_type_fieldset_functions";
import { YearFieldsetFunctions } from "./year_fieldset_functions";
import { ContainerDOIFieldsetFunctions } from "./container_doi_fieldset_functions";
import { RankFieldsetFunctions } from "./rank_fieldset_functions";
import { URLProcessor } from "../../../url_processor";
import { FoundRecordSummary } from "../../../doi_filter/found_record_summary";
import { DOIRecordCollection } from "../../../doi_record_collection";
export class SearchFilterBoxFunctions {   

  public static convertInputToURLParameters(isPrimaryFilter: boolean): [string, string][] {

    const newTypes = RecordTypeFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);
    const newYearParameters = YearFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);
    const newContainerDOIParameters = ContainerDOIFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);
    const newRankParameters = RankFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);

    return [...newTypes, ...newYearParameters, ...newContainerDOIParameters, ...newRankParameters];
  }

  public static initializeFilterBox(isPrimaryFilter: boolean, foundRecordSummary: FoundRecordSummary, doiRecordCollection: DOIRecordCollection): void {
    const filterSearch = URLProcessor.buildSearchFilterFromURL(isPrimaryFilter);

    RecordTypeFieldsetFunctions.initializeRecordTypes(isPrimaryFilter, foundRecordSummary.type_to_id_count_mapper, filterSearch.types);
    YearFieldsetFunctions.initializeYearBox(isPrimaryFilter, foundRecordSummary.year_to_id_count_mapper, foundRecordSummary.unknown_year_id_count, filterSearch.minimumYear, filterSearch.maximumYear);
    RankFieldsetFunctions.updateRankBox(isPrimaryFilter, foundRecordSummary.primary_record_count, foundRecordSummary.secondary_record_count, filterSearch.excludeStatus);
    const removeEmptyContainers = isPrimaryFilter ? false : true;
    ContainerDOIFieldsetFunctions.initialize(isPrimaryFilter, removeEmptyContainers,
      foundRecordSummary.idToPrimaryRecordCountMapper, foundRecordSummary.idToSecondaryRecordCountMapper,
      doiRecordCollection, filterSearch.topContainerType, filterSearch.topContainerDOI, filterSearch.subContainerDOI);
 
  }


  public static setURLParameters(isPrimaryFilter: boolean, parameters: [string, string][]): void {
    const prefix = isPrimaryFilter ? "psf-" : "ssf-";
    const url = new URL(window.location.href);

    url.searchParams.delete(prefix + "type");
    url.searchParams.delete(prefix + "minimum-year");
    url.searchParams.delete(prefix + "maximum-year");
    url.searchParams.delete(prefix + "top-container-doi");
    url.searchParams.delete(prefix + "sub-container-doi");
    url.searchParams.delete(prefix + "top-container-type");
    url.searchParams.delete(prefix + "excluded-status");

    parameters.forEach(parameter => {
      url.searchParams.append(parameter[0], parameter[1]);
    });
    

    window.history.replaceState(null, "", url.toString());
  }

  public static resetURLParameters(isPrimaryFilter: boolean): void {
    this.setURLParameters(isPrimaryFilter, []);
  }

}