import { SearchFilter } from "./doi_filter/search_filter";
import { containerTypeList, paperTypeList, otherTypeList } from "./doi_record";
import { AnyContainerType, AnyPaperType, AnyOtherType } from "./doi_record";

export class URLProcessor {


    public static buildPrimarySearchFilterFromURL() : SearchFilter {
        const url = new URL(window.location.href);
        const primarySearchFilter = new SearchFilter();

        const types : string[] | null = url.searchParams.getAll("psf-type");
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

        const ancestorDOI : string | null = url.searchParams.get("psf-ancestor-doi");
        if(ancestorDOI != null) {
            primarySearchFilter.ancestorDoi = ancestorDOI;
        }

        const topContainerType : string | null = url.searchParams.get("psf-top-container-type");
        if(topContainerType != null) {
            primarySearchFilter.topContainerType = topContainerType;
        }

        const minimumYear : string | null = url.searchParams.get("psf-minimum-year");
        if(minimumYear != null) {
            primarySearchFilter.minimumYear = parseInt(minimumYear);
        }
        const maximumYear : string | null = url.searchParams.get("psf-maximum-year");
        if(maximumYear != null) {
            primarySearchFilter.maximumYear = parseInt(maximumYear);
        }

        const excludeStatus : string[] | null = url.searchParams.getAll("psf-excluded-status");
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