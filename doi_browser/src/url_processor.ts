import { SearchFilter } from "./doi_filter/search_filter";
import { containerTypeList, paperTypeList, otherTypeList } from "./doi_record";
import { AnyContainerType, AnyPaperType, AnyOtherType } from "./doi_record";

export class URLProcessor {


    public static buildSearchFilterFromURL(isPrimaryFilter: boolean) : SearchFilter {
        const idPrefix = isPrimaryFilter ? "psf-" : "ssf-";
        const url = new URL(window.location.href);
        const primarySearchFilter = new SearchFilter();

        const types : string[] | null = url.searchParams.getAll(idPrefix + "type");
        if(types != null) {
            if(types.length == 0) {
                containerTypeList.concat(paperTypeList).concat(otherTypeList).forEach(type => {
                    primarySearchFilter.types.push(type);
                });
            }else{
                types.forEach(type => {
                    if(type == AnyContainerType) {
                        containerTypeList.forEach(containerType => {
                            primarySearchFilter.types.push(containerType);
                        });
                    }else if(type == AnyPaperType) {
                        paperTypeList.forEach(paperType => {
                            primarySearchFilter.types.push(paperType);
                        });
                    }else if(type == AnyOtherType) {
                        otherTypeList.forEach(otherType => {
                            primarySearchFilter.types.push(otherType);
                        });
                    }
                    else if(type == "Empty") {

                    }
                    else{
                        primarySearchFilter.types.push(type);
                    }
                });
    
            }


        }


        const topContainerType : string | null = url.searchParams.get(idPrefix + "top-container-type");
        if(topContainerType != null) {
            primarySearchFilter.topContainerType = topContainerType;
        }

        const topContainerDOI : string | null = url.searchParams.get(idPrefix + "top-container-doi");
        if(topContainerDOI != null) {
            primarySearchFilter.topContainerDOI = topContainerDOI;
        }

        const subContainerDOI : string | null = url.searchParams.get(idPrefix + "sub-container-doi");
        if(subContainerDOI != null) {
            primarySearchFilter.subContainerDOI = subContainerDOI;
        }

        const minimumYear : string | null = url.searchParams.get(idPrefix + "minimum-year");
        if(minimumYear != null) {
            primarySearchFilter.minimumYear = parseInt(minimumYear);
        }
        const maximumYear : string | null = url.searchParams.get(idPrefix + "maximum-year");
        if(maximumYear != null) {
            primarySearchFilter.maximumYear = parseInt(maximumYear);
        }

        const excludeStatus : string[] | null = url.searchParams.getAll(idPrefix + "excluded-status");
        if(excludeStatus != null) {
            excludeStatus.forEach(status => {
                if(status == "primary") {
                    primarySearchFilter.excludeStatus.push("primary");
                }
            });
        }

        return primarySearchFilter;

    }
}