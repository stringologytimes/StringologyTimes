export class YearFieldsetFunctions {

    public static initializeYearBox(is_primary_filter: boolean, year_to_id_count_mapper: Map<number, number>, unknown_year_id_count: number) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const yearFromSelect = document.getElementById(`${id_prefix}-year-from-select`) as HTMLElement;
        if (yearFromSelect == null) {
            throw new Error(`${id_prefix}-year-from-select is not found`);
        }
        const yearToSelect = document.getElementById(`${id_prefix}-year-to-select`) as HTMLElement;
        if (yearToSelect == null) {
            throw new Error(`${id_prefix}-year-to-select is not found`);
        }
        yearFromSelect.replaceChildren();
        yearToSelect.replaceChildren();

        {
            const anyOption1 = document.createElement("option");
            const anyOption2 = document.createElement("option");

            anyOption1.value = "Any";
            anyOption1.textContent = "Any";
            anyOption1.setAttribute("data-year", "Any");
            yearFromSelect.appendChild(anyOption1);

            anyOption2.value = "Any";
            anyOption2.textContent = "Any";
            anyOption2.setAttribute("data-year", "Any");
            yearToSelect.appendChild(anyOption2);
        }

        if (year_to_id_count_mapper.size > 0) {
            let minYear = Math.min(...year_to_id_count_mapper.keys());
            let maxYear = Math.max(...year_to_id_count_mapper.keys());
            for (let year = minYear; year <= maxYear; year++) {

                const option1 = document.createElement("option");
                const option2 = document.createElement("option");
                const recordCount = year_to_id_count_mapper.get(year) ?? 0;

                if (recordCount > 0) {
                    option1.value = year.toString();
                    option1.textContent = `${year} (${recordCount})`;
                    option1.setAttribute("data-year", year.toString());
                    yearFromSelect.appendChild(option1);

                    option2.value = year.toString();
                    option2.textContent = `${year} (${recordCount})`;
                    option2.setAttribute("data-year", year.toString());
                    yearToSelect.appendChild(option2);
                }
            }
        }
    }

    public static updateYearBox(is_primary_filter: boolean, year_to_id_count_mapper: Map<number, number>, unknown_year_id_count: number) {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const yearFieldset = document.getElementById(`${id_prefix}-year-fieldset`) as HTMLElement;
        if (yearFieldset == null) {
            throw new Error(`${id_prefix}-year-fieldset is not found`);
        } else {
            const options = yearFieldset.querySelectorAll<HTMLSelectElement>("option");
            options.forEach(option => {
                if (option.getAttribute("data-year") != null) {
                    const year = parseInt(option.getAttribute("data-year") as string);
                    const count = year_to_id_count_mapper.get(year) ?? 0;
                    if (year != null) {
                        option.textContent = `${year} (${count})`;
                    }
                }
            });
        }
    }

    public static getYearFrom(is_primary_filter: boolean): string | null {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const yearFromSelect: HTMLSelectElement = document.getElementById(`${id_prefix}-year-from-select`) as HTMLSelectElement;
        if (yearFromSelect == null) {
            throw new Error(`${id_prefix}-year-from-select is not found`);
        }
        const selectedValue = yearFromSelect.value;
        if (selectedValue == "Any") {
            return null;
        }
        return selectedValue;
    }

    public static getYearTo(is_primary_filter: boolean): string | null {
        const id_prefix = is_primary_filter ? "psf" : "ssf";
        const yearToSelect: HTMLSelectElement = document.getElementById(`${id_prefix}-year-to-select`) as HTMLSelectElement;
        if (yearToSelect == null) {
            throw new Error(`${id_prefix}-year-to-select is not found`);
        }
        const selectedValue = yearToSelect.value;
        if (selectedValue == "Any") {
            return null;
        }
        return selectedValue;
    }

}
