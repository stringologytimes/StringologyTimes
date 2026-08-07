import { DOIRecordCollection } from "../../doi_record_collection";
import { RecordTypeFieldsetFunctions } from "./fieldset/record_type_fieldset_functions";
import { YearFieldsetFunctions } from "./fieldset/year_fieldset_functions";
import { RankFieldsetFunctions } from "./fieldset/rank_fieldset_functions";
import { ContainerDOIFieldsetFunctions } from "./fieldset/container_doi_fieldset_functions";
//import { SearchFilterBoxFunctions } from "./fieldset/search_filter_box_functions";


//let topContainerCategories: string[] = ["Journal", "Proceedings", "Preprint"];
//let containerSelect2Items: [string, string][] = [];

export class PrimarySearchFilterRender {
  public static initialize(doiRecordCollection: DOIRecordCollection): void {
   

    RecordTypeFieldsetFunctions.initializeRecordTypes(true, doiRecordCollection.recordSummary.type_to_id_count_mapper);
    YearFieldsetFunctions.initializeYearBox(true, doiRecordCollection.recordSummary.year_to_id_count_mapper, doiRecordCollection.recordSummary.unknown_year_id_count);
    RankFieldsetFunctions.updateRankBox(true, doiRecordCollection.recordSummary.primary_record_count, doiRecordCollection.recordSummary.secondary_record_count);
    ContainerDOIFieldsetFunctions.initializeContainerBox(true, doiRecordCollection, false, doiRecordCollection.recordSummary.idToPrimaryRecordCountMapper, doiRecordCollection.recordSummary.idToSecondaryRecordCountMapper);




    /*
    this.initializeRecordTypes(recordTypeCounters, doiRecordCollection);
    this.initializeYearBox(doiRecordCollection);
    */
  }




  

  /*


  public static setURLParameters(doiRecordCollection: DOIRecordCollection): void {
    const newParameters = SearchFilterBoxFunctions.convertInputToURLParameters(true);

    const url = new URL(window.location.href);
    url.searchParams.delete("psf-type");
    const newTypes = RecordTypeFieldsetFunctions.getCheckedTypes(true);
    newTypes.forEach(type => {
      url.searchParams.append("psf-type", type);
    });

    const newSubContainerDOI = ContainerDOIFieldsetFunctions.getSubContainerDOI(true);

    url.searchParams.delete("ancestor-doi");
    url.searchParams.delete("top-container-type");


    if (newSubContainerDOI != null) {
      url.searchParams.set("ancestor-doi", newSubContainerDOI);
    } else {
      const newTopContainerDOI = ContainerDOIFieldsetFunctions.getTopContainerDOI(true);
      if (newTopContainerDOI != null) {
        url.searchParams.set("ancestor-doi", newTopContainerDOI);
      } else {
        const newTopContainerType = ContainerDOIFieldsetFunctions.getTopContainerType(true);
        if (newTopContainerType != null) {
          url.searchParams.set("top-container-type", newTopContainerType);
        }
      }
    }




    const newYearFrom = YearFieldsetFunctions.getYearFrom(true);
    url.searchParams.delete("psf-minimum-year");
    if (newYearFrom != null) {
      url.searchParams.append("psf-minimum-year", newYearFrom);
    }
    const newYearTo = YearFieldsetFunctions.getYearTo(true);
    url.searchParams.delete("psf-year-to");
    if (newYearTo != null) {
      url.searchParams.append("psf-year-to", newYearTo);
    }

    const newExcludedStatus = RankFieldsetFunctions.getExcludedStatus(true);
    console.log("newExcludedStatus: " + newExcludedStatus);
    url.searchParams.delete("psf-excluded-status");
    if (newExcludedStatus.length > 0) {
      newExcludedStatus.forEach(status => {
        url.searchParams.append("psf-excluded-status", status);
      });
    }

    window.history.replaceState(null, "", url.toString());


  }
  */

}

