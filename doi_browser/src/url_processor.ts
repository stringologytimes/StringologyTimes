import { PrimarySearchFilter } from "./doi_filter/primary_search_filter";
import { containerTypeList, paperTypeList, otherTypeList } from "./doi_record";

export class URLProcessor {


    public static buildPrimarySearchFilterFromURL() : PrimarySearchFilter {
        const url = new URL(window.location.href);
        const primarySearchFilter = new PrimarySearchFilter();

        const types : string[] | null = url.searchParams.getAll("psf-type");
        if(types != null) {
            types.forEach(type => {
                if(type == "Container-Any") {
                    containerTypeList.forEach(containerType => {
                        primarySearchFilter.types.push(containerType);
                    });
                }else if(type == "Paper-Any") {
                    paperTypeList.forEach(paperType => {
                        primarySearchFilter.types.push(paperType);
                    });
                }else if(type == "Other-Any") {
                    otherTypeList.forEach(otherType => {
                        primarySearchFilter.types.push(otherType);
                    });
                }else{
                    primarySearchFilter.types.push(type);
                }
            });
        }

        const ancestorDOI : string | null = url.searchParams.get("ancestor-doi");
        if(ancestorDOI != null) {
            primarySearchFilter.ancestor_doi = ancestorDOI;
        }

        const topContainerType : string | null = url.searchParams.get("top-container-type");
        if(topContainerType != null) {
            primarySearchFilter.top_container_type = topContainerType;
        }

        const minimumYear : string | null = url.searchParams.get("psf-year-from");
        if(minimumYear != null) {
            primarySearchFilter.minimum_year = parseInt(minimumYear);
        }
        const maximumYear : string | null = url.searchParams.get("psf-year-to");
        if(maximumYear != null) {
            primarySearchFilter.maximum_year = parseInt(maximumYear);
        }

        const excludeStatus : string[] | null = url.searchParams.getAll("exclude-status");
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