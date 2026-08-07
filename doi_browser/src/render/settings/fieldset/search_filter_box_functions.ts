import { RecordTypeFieldsetFunctions } from "./record_type_fieldset_functions";
import { YearFieldsetFunctions } from "./year_fieldset_functions";
import { ContainerDOIFieldsetFunctions } from "./container_doi_fieldset_functions";
import { RankFieldsetFunctions } from "./rank_fieldset_functions";

export class SearchFilterBoxFunctions {   

  public static convertInputToURLParameters(isPrimaryFilter: boolean): [string, string][] {

    const newTypes = RecordTypeFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);
    const newYearParameters = YearFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);
    const newContainerDOIParameters = ContainerDOIFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);
    const newRankParameters = RankFieldsetFunctions.convertInputToURLParameters(isPrimaryFilter);

    return [...newTypes, ...newYearParameters, ...newContainerDOIParameters, ...newRankParameters];




    //window.history.replaceState(null, "", url.toString());


  }

  public static setURLParameters(isPrimaryFilter: boolean, parameters: [string, string][]): void {
    const prefix = isPrimaryFilter ? "psf-" : "ssf-";
    const url = new URL(window.location.href);

    url.searchParams.delete(prefix + "type");
    url.searchParams.delete(prefix + "minimum-year");
    url.searchParams.delete(prefix + "maximum-year");
    url.searchParams.delete(prefix + "ancestor-doi");
    url.searchParams.delete(prefix + "top-container-type");
    url.searchParams.delete(prefix + "excluded-status");

    parameters.forEach(parameter => {
      url.searchParams.append(parameter[0], parameter[1]);
    });
    

    window.history.replaceState(null, "", url.toString());


  }
}