

export class ArticleInfo {
    public doi: string = "";
    public PaperType: string= "";
    public BookTitleOrJournal: string= "";
    public title: string = "";
    public year: number = 0;
    public authors: string[] = [];
    public volume: string= "";
    public tags: string[] = [];


    public static parse(obj: any[]): ArticleInfo {
        const articleInfo = new ArticleInfo();

        articleInfo.doi = obj[0];
        articleInfo.PaperType = obj[1];
        articleInfo.BookTitleOrJournal = obj[2];
        articleInfo.title = obj[3];
        articleInfo.year = obj[4];
        articleInfo.authors = obj[5];
        articleInfo.volume = obj[6];
        articleInfo.tags = obj[7];
        return articleInfo;
    }

}
export const articleInfos: ArticleInfo[] = [];


export function load_jsonl() {
    window.addEventListener('DOMContentLoaded', () => {
        fetch('./jsonl/stringology_dblp.jsonl')
            .then(response => {
                if (!response.ok) {
                    throw new Error('HTTP error: ' + response.status);
                }
                return response.text();  // Get as text, not JSON
            })
            .then(text => {
                const output = document.getElementById('output');
                const errorDiv = document.getElementById('error');

                const lines = text.split(/\r?\n/).filter(line => line.trim() !== '');

                if (lines.length === 0) {
                    if(output != null){
                        output.textContent = 'No valid lines in file.';
                    }
                    return;
                }


                lines.forEach((line, index) => {
                    try {
                        const obj = JSON.parse(line);
                        if(Array.isArray(obj)){
                            const articleInfo = ArticleInfo.parse(obj);
                            articleInfos.push(articleInfo);
                        }else{
                            throw new Error(`Cannot parse line ${index + 1} as JSON: ${line}`);
                        }
                    } catch (e) {
                        throw new Error(`Cannot parse line ${index + 1} as JSON: ${line}`);
                    }
                });

                // Update display after data loading is complete
                displayArticleInfos();

            })
            .catch(err => {
                const errorDiv = document.getElementById('error');
                if(errorDiv != null){
                    errorDiv.textContent =
                        'Load error: ' + err.message +
                        '\n(If opened via file:///, please open via local HTTP server)';
                }
            });
    });
}

export function displayArticleInfos() {
    const output = document.getElementById('output');
    if (!output) return;

    if (articleInfos.length === 0) {
        output.innerHTML = '<p>No article information available.</p>';
        return;
    }

    // Search and filtering UI
    output.innerHTML = `
        <div id="controls">
            <div class="search-box">
                <input type="text" id="searchInput" placeholder="Search by title, author, tag..." />
                <select id="yearFilter">
                    <option value="">All years</option>
                </select>
                <select id="typeFilter">
                    <option value="">All types</option>
                </select>
                <select id="bookTitleOrJournalFilter">
                    <option value="">All publications</option>
                </select>
                <select id="tagFilter1">
                    <option value="">All tags (1)</option>
                </select>
                <select id="tagFilter2">
                    <option value="">All tags (2)</option>
                </select>
                <button id="clearFilters">Clear filters</button>
            </div>
            <div id="stats">
                <p>Displaying: <span id="displayCount">${articleInfos.length}</span> / Total: ${articleInfos.length} articles</p>
                <div class="sort-controls">
                    <label for="sortOrder">Sort by Year:</label>
                    <select id="sortOrder">
                        <option value="desc">Descending (newest first)</option>
                        <option value="asc">Ascending (oldest first)</option>
                    </select>
                </div>
            </div>
            <div id="pagination" class="pagination-controls">
                <button id="prevBtn" disabled>Prev</button>
                <span id="pageInfo">Page 1</span>
                <button id="nextBtn">Next</button>
            </div>
        </div>
        <div id="articleList"></div>
    `;

    // Generate filter options for years, types, BookTitleOrJournal, and tags
    const years = [...new Set(articleInfos.map(a => a.year))].sort((a, b) => b - a);
    const types = [...new Set(articleInfos.map(a => a.PaperType))].sort();
    const bookTitleOrJournals = [...new Set(articleInfos.map(a => a.BookTitleOrJournal))].filter(b => b && b.trim() !== '').sort();
    const allTags = [...new Set(articleInfos.reduce((acc: string[], a) => acc.concat(a.tags), []))].filter((t: string) => t && t.trim() !== '').sort();
    
    const yearFilter = document.getElementById('yearFilter') as HTMLSelectElement;
    const typeFilter = document.getElementById('typeFilter') as HTMLSelectElement;
    const bookTitleOrJournalFilter = document.getElementById('bookTitleOrJournalFilter') as HTMLSelectElement;
    const tagFilter1 = document.getElementById('tagFilter1') as HTMLSelectElement;
    const tagFilter2 = document.getElementById('tagFilter2') as HTMLSelectElement;
    
    years.forEach(year => {
        const option = document.createElement('option');
        const yearStr = year.toString();
        const count = articleInfos.filter(a => a.year.toString() === yearStr).length;
        option.value = yearStr;
        option.textContent = `${yearStr} (${count})`;
        yearFilter.appendChild(option);
    });
    
    types.forEach(type => {
        const option = document.createElement('option');
        const count = articleInfos.filter(a => a.PaperType === type).length;
        option.value = type;
        option.textContent = `${type} (${count})`;
        typeFilter.appendChild(option);
    });
    
    bookTitleOrJournals.forEach(bookTitleOrJournal => {
        const option = document.createElement('option');
        const count = articleInfos.filter(a => a.BookTitleOrJournal === bookTitleOrJournal).length;
        option.value = bookTitleOrJournal;
        option.textContent = `${bookTitleOrJournal} (${count})`;
        bookTitleOrJournalFilter.appendChild(option);
    });
    
    allTags.forEach((tag: string) => {
        const count = articleInfos.filter(a => a.tags.includes(tag)).length;
        
        const option1 = document.createElement('option');
        option1.value = tag;
        option1.textContent = `${tag} (${count})`;
        tagFilter1.appendChild(option1);
        
        const option2 = document.createElement('option');
        option2.value = tag;
        option2.textContent = `${tag} (${count})`;
        tagFilter2.appendChild(option2);
    });

    // Set up event listeners
    const searchInput = document.getElementById('searchInput') as HTMLInputElement;
    const clearFiltersBtn = document.getElementById('clearFilters') as HTMLButtonElement;
    const sortOrder = document.getElementById('sortOrder') as HTMLSelectElement;
    const prevBtn = document.getElementById('prevBtn') as HTMLButtonElement;
    const nextBtn = document.getElementById('nextBtn') as HTMLButtonElement;
    const pageInfo = document.getElementById('pageInfo') as HTMLSpanElement;
    
    // Page state
    let currentPage = 1;
    const ITEMS_PER_PAGE = 100;
    
    // Get filtered articles based on current filter conditions (excluding the specified filter)
    const getFilteredArticles = (excludeFilter?: string): ArticleInfo[] => {
        const searchTerm = searchInput.value.toLowerCase();
        const selectedYear = excludeFilter !== 'year' ? yearFilter.value : '';
        const selectedType = excludeFilter !== 'type' ? typeFilter.value : '';
        const selectedBookTitleOrJournal = excludeFilter !== 'bookTitleOrJournal' ? bookTitleOrJournalFilter.value : '';
        const selectedTag1 = excludeFilter !== 'tag1' ? tagFilter1.value : '';
        const selectedTag2 = excludeFilter !== 'tag2' ? tagFilter2.value : '';

        return articleInfos.filter(article => {
            const matchesSearch = !searchTerm || 
                article.title.toLowerCase().includes(searchTerm) ||
                article.authors.some(author => author.toLowerCase().includes(searchTerm)) ||
                article.tags.some(tag => tag.toLowerCase().includes(searchTerm));
            
            const matchesYear = !selectedYear || article.year.toString() === selectedYear;
            const matchesType = !selectedType || article.PaperType === selectedType;
            const matchesBookTitleOrJournal = !selectedBookTitleOrJournal || article.BookTitleOrJournal === selectedBookTitleOrJournal;
            const matchesTag1 = !selectedTag1 || article.tags.includes(selectedTag1);
            const matchesTag2 = !selectedTag2 || article.tags.includes(selectedTag2);

            return matchesSearch && matchesYear && matchesType && matchesBookTitleOrJournal && matchesTag1 && matchesTag2;
        });
    };
    
    const updateDisplay = () => {
        const filtered = getFilteredArticles();
        const sortValue = sortOrder.value;
        
        // Reset to page 1 when filters change
        currentPage = 1;
        
        // Sort articles by year
        const sortedArticles = [...filtered].sort((a, b) => {
            if (sortValue === 'asc') {
                return a.year - b.year;
            } else {
                return b.year - a.year;
            }
        });
        
        const totalPages = Math.ceil(sortedArticles.length / ITEMS_PER_PAGE);
        const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
        const endIndex = startIndex + ITEMS_PER_PAGE;
        const paginatedArticles = sortedArticles.slice(startIndex, endIndex);
        
        renderArticles(paginatedArticles, startIndex);
        
        // Update pagination controls
        updatePaginationControls(sortedArticles.length, totalPages);
        
        const displayCount = document.getElementById('displayCount');
        if (displayCount) {
            const totalCount = sortedArticles.length;
            const displayedCount = paginatedArticles.length;
            if (totalCount > ITEMS_PER_PAGE) {
                displayCount.textContent = `${displayedCount} (${startIndex + 1}-${startIndex + displayedCount} of ${totalCount})`;
            } else {
                displayCount.textContent = totalCount.toString();
            }
        }
    };
    
    const updatePaginationControls = (totalCount: number, totalPages: number) => {
        // Update page info
        if (pageInfo) {
            pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
        }
        
        // Update button states
        if (prevBtn) {
            prevBtn.disabled = currentPage === 1;
        }
        if (nextBtn) {
            nextBtn.disabled = currentPage >= totalPages || totalCount === 0;
        }
    };
    
    const goToPage = (page: number) => {
        const filtered = getFilteredArticles();
        const sortValue = sortOrder.value;
        
        // Sort articles by year
        const sortedArticles = [...filtered].sort((a, b) => {
            if (sortValue === 'asc') {
                return a.year - b.year;
            } else {
                return b.year - a.year;
            }
        });
        
        const totalPages = Math.ceil(sortedArticles.length / ITEMS_PER_PAGE);
        
        if (page < 1 || page > totalPages) {
            return;
        }
        
        currentPage = page;
        const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
        const endIndex = startIndex + ITEMS_PER_PAGE;
        const paginatedArticles = sortedArticles.slice(startIndex, endIndex);
        
        renderArticles(paginatedArticles, startIndex);
        updatePaginationControls(sortedArticles.length, totalPages);
        
        const displayCount = document.getElementById('displayCount');
        if (displayCount) {
            const totalCount = sortedArticles.length;
            const displayedCount = paginatedArticles.length;
            if (totalCount > ITEMS_PER_PAGE) {
                displayCount.textContent = `${displayedCount} (${startIndex + 1}-${startIndex + displayedCount} of ${totalCount})`;
            } else {
                displayCount.textContent = totalCount.toString();
            }
        }
        
        // Scroll to top of article list
        const articleList = document.getElementById('articleList');
        if (articleList) {
            articleList.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    };

    // Count articles matching a specific filter value
    const countArticlesForValue = (filterType: string, value: string): number => {
        const filteredArticles = getFilteredArticles(filterType);
        
        if (filterType === 'year') {
            return filteredArticles.filter(a => a.year.toString() === value).length;
        } else if (filterType === 'type') {
            return filteredArticles.filter(a => a.PaperType === value).length;
        } else if (filterType === 'bookTitleOrJournal') {
            return filteredArticles.filter(a => a.BookTitleOrJournal === value).length;
        } else if (filterType === 'tag1' || filterType === 'tag2') {
            return filteredArticles.filter(a => a.tags.includes(value)).length;
        }
        return 0;
    };

    // Update filter options based on current filtered articles
    const updateFilterOptions = (filterType: string) => {
        const filteredArticles = getFilteredArticles(filterType);
        
        if (filterType === 'year') {
            const currentValue = yearFilter.value;
            const availableYears = [...new Set(filteredArticles.map(a => a.year))].sort((a, b) => b - a);
            yearFilter.innerHTML = '<option value="">All years</option>';
            availableYears.forEach(year => {
                const option = document.createElement('option');
                const yearStr = year.toString();
                const count = countArticlesForValue('year', yearStr);
                option.value = yearStr;
                option.textContent = `${yearStr} (${count})`;
                yearFilter.appendChild(option);
            });
            if (currentValue) {
                yearFilter.value = currentValue;
            }
        } else if (filterType === 'type') {
            const currentValue = typeFilter.value;
            const availableTypes = [...new Set(filteredArticles.map(a => a.PaperType))].sort();
            typeFilter.innerHTML = '<option value="">All types</option>';
            availableTypes.forEach(type => {
                const option = document.createElement('option');
                const count = countArticlesForValue('type', type);
                option.value = type;
                option.textContent = `${type} (${count})`;
                typeFilter.appendChild(option);
            });
            if (currentValue) {
                typeFilter.value = currentValue;
            }
        } else if (filterType === 'bookTitleOrJournal') {
            const currentValue = bookTitleOrJournalFilter.value;
            const availableBookTitleOrJournals = [...new Set(filteredArticles.map(a => a.BookTitleOrJournal))].filter(b => b && b.trim() !== '').sort();
            bookTitleOrJournalFilter.innerHTML = '<option value="">All publications</option>';
            availableBookTitleOrJournals.forEach(bookTitleOrJournal => {
                const option = document.createElement('option');
                const count = countArticlesForValue('bookTitleOrJournal', bookTitleOrJournal);
                option.value = bookTitleOrJournal;
                option.textContent = `${bookTitleOrJournal} (${count})`;
                bookTitleOrJournalFilter.appendChild(option);
            });
            if (currentValue) {
                bookTitleOrJournalFilter.value = currentValue;
            }
        } else if (filterType === 'tag1') {
            const currentValue = tagFilter1.value;
            const availableTags = [...new Set(filteredArticles.reduce((acc: string[], a) => acc.concat(a.tags), []))].filter((t: string) => t && t.trim() !== '').sort();
            tagFilter1.innerHTML = '<option value="">All tags (1)</option>';
            availableTags.forEach((tag: string) => {
                const option = document.createElement('option');
                const count = countArticlesForValue('tag1', tag);
                option.value = tag;
                option.textContent = `${tag} (${count})`;
                tagFilter1.appendChild(option);
            });
            if (currentValue) {
                tagFilter1.value = currentValue;
            }
        } else if (filterType === 'tag2') {
            const currentValue = tagFilter2.value;
            const availableTags = [...new Set(filteredArticles.reduce((acc: string[], a) => acc.concat(a.tags), []))].filter((t: string) => t && t.trim() !== '').sort();
            tagFilter2.innerHTML = '<option value="">All tags (2)</option>';
            availableTags.forEach((tag: string) => {
                const option = document.createElement('option');
                const count = countArticlesForValue('tag2', tag);
                option.value = tag;
                option.textContent = `${tag} (${count})`;
                tagFilter2.appendChild(option);
            });
            if (currentValue) {
                tagFilter2.value = currentValue;
            }
        }
    };

    sortOrder.addEventListener('change', () => {
        // Keep current page when sorting changes
        const filtered = getFilteredArticles();
        const sortValue = sortOrder.value;
        
        // Sort articles by year
        const sortedArticles = [...filtered].sort((a, b) => {
            if (sortValue === 'asc') {
                return a.year - b.year;
            } else {
                return b.year - a.year;
            }
        });
        
        const totalPages = Math.ceil(sortedArticles.length / ITEMS_PER_PAGE);
        if (currentPage > totalPages) {
            currentPage = Math.max(1, totalPages);
        }
        const startIndex = (currentPage - 1) * ITEMS_PER_PAGE;
        const endIndex = startIndex + ITEMS_PER_PAGE;
        const paginatedArticles = sortedArticles.slice(startIndex, endIndex);
        
        renderArticles(paginatedArticles, startIndex);
        updatePaginationControls(sortedArticles.length, totalPages);
        
        const displayCount = document.getElementById('displayCount');
        if (displayCount) {
            const totalCount = sortedArticles.length;
            const displayedCount = paginatedArticles.length;
            if (totalCount > ITEMS_PER_PAGE) {
                displayCount.textContent = `${displayedCount} (${startIndex + 1}-${startIndex + displayedCount} of ${totalCount})`;
            } else {
                displayCount.textContent = totalCount.toString();
            }
        }
    });
    
    prevBtn.addEventListener('click', () => {
        if (currentPage > 1) {
            goToPage(currentPage - 1);
        }
    });
    
    nextBtn.addEventListener('click', () => {
        const filtered = getFilteredArticles();
        const totalPages = Math.ceil(filtered.length / ITEMS_PER_PAGE);
        if (currentPage < totalPages) {
            goToPage(currentPage + 1);
        }
    });
    
    searchInput.addEventListener('input', () => {
        updateDisplay();
        // Update all filter options when search changes
        updateFilterOptions('year');
        updateFilterOptions('type');
        updateFilterOptions('bookTitleOrJournal');
        updateFilterOptions('tag1');
        updateFilterOptions('tag2');
    });
    
    yearFilter.addEventListener('focus', () => updateFilterOptions('year'));
    yearFilter.addEventListener('change', () => {
        updateDisplay();
        // Update other filter options when year changes
        updateFilterOptions('type');
        updateFilterOptions('bookTitleOrJournal');
        updateFilterOptions('tag1');
        updateFilterOptions('tag2');
    });
    
    typeFilter.addEventListener('focus', () => updateFilterOptions('type'));
    typeFilter.addEventListener('change', () => {
        updateDisplay();
        // Update other filter options when type changes
        updateFilterOptions('year');
        updateFilterOptions('bookTitleOrJournal');
        updateFilterOptions('tag1');
        updateFilterOptions('tag2');
    });
    
    bookTitleOrJournalFilter.addEventListener('focus', () => updateFilterOptions('bookTitleOrJournal'));
    bookTitleOrJournalFilter.addEventListener('change', () => {
        updateDisplay();
        // Update other filter options when bookTitleOrJournal changes
        updateFilterOptions('year');
        updateFilterOptions('type');
        updateFilterOptions('tag1');
        updateFilterOptions('tag2');
    });
    
    tagFilter1.addEventListener('focus', () => updateFilterOptions('tag1'));
    tagFilter1.addEventListener('change', () => {
        updateDisplay();
        // Update other filter options when tag1 changes
        updateFilterOptions('year');
        updateFilterOptions('type');
        updateFilterOptions('bookTitleOrJournal');
        updateFilterOptions('tag2');
    });
    
    tagFilter2.addEventListener('focus', () => updateFilterOptions('tag2'));
    tagFilter2.addEventListener('change', () => {
        updateDisplay();
        // Update other filter options when tag2 changes
        updateFilterOptions('year');
        updateFilterOptions('type');
        updateFilterOptions('bookTitleOrJournal');
        updateFilterOptions('tag1');
    });
    
    clearFiltersBtn.addEventListener('click', () => {
        searchInput.value = '';
        yearFilter.value = '';
        typeFilter.value = '';
        bookTitleOrJournalFilter.value = '';
        tagFilter1.value = '';
        tagFilter2.value = '';
        // Reset all filter options to show all values with counts
        const allYears = [...new Set(articleInfos.map(a => a.year))].sort((a, b) => b - a);
        const allTypes = [...new Set(articleInfos.map(a => a.PaperType))].sort();
        const allBookTitleOrJournals = [...new Set(articleInfos.map(a => a.BookTitleOrJournal))].filter(b => b && b.trim() !== '').sort();
        const allTags = [...new Set(articleInfos.reduce((acc: string[], a) => acc.concat(a.tags), []))].filter((t: string) => t && t.trim() !== '').sort();
        
        yearFilter.innerHTML = '<option value="">All years</option>';
        allYears.forEach(year => {
            const option = document.createElement('option');
            const yearStr = year.toString();
            const count = articleInfos.filter(a => a.year.toString() === yearStr).length;
            option.value = yearStr;
            option.textContent = `${yearStr} (${count})`;
            yearFilter.appendChild(option);
        });
        
        typeFilter.innerHTML = '<option value="">All types</option>';
        allTypes.forEach(type => {
            const option = document.createElement('option');
            const count = articleInfos.filter(a => a.PaperType === type).length;
            option.value = type;
            option.textContent = `${type} (${count})`;
            typeFilter.appendChild(option);
        });
        
        bookTitleOrJournalFilter.innerHTML = '<option value="">All publications</option>';
        allBookTitleOrJournals.forEach(bookTitleOrJournal => {
            const option = document.createElement('option');
            const count = articleInfos.filter(a => a.BookTitleOrJournal === bookTitleOrJournal).length;
            option.value = bookTitleOrJournal;
            option.textContent = `${bookTitleOrJournal} (${count})`;
            bookTitleOrJournalFilter.appendChild(option);
        });
        
        tagFilter1.innerHTML = '<option value="">All tags (1)</option>';
        tagFilter2.innerHTML = '<option value="">All tags (2)</option>';
        allTags.forEach((tag: string) => {
            const count = articleInfos.filter(a => a.tags.includes(tag)).length;
            
            const option1 = document.createElement('option');
            option1.value = tag;
            option1.textContent = `${tag} (${count})`;
            tagFilter1.appendChild(option1);
            
            const option2 = document.createElement('option');
            option2.value = tag;
            option2.textContent = `${tag} (${count})`;
            tagFilter2.appendChild(option2);
        });
        
        updateDisplay();
    });

    // Initial display
    updateDisplay();
}

function renderArticles(articles: ArticleInfo[], startIndex: number = 0) {
    const articleList = document.getElementById('articleList');
    if (!articleList) return;

    if (articles.length === 0) {
        articleList.innerHTML = '<p>No matching articles found.</p>';
        return;
    }

    articleList.innerHTML = articles.map((article, index) => {
        const articleNumber = startIndex + index + 1;
        return `
        <div class="article-card">
            <div class="article-number">#${articleNumber}</div>
            <h3 class="article-title">${escapeHtml(article.title)}</h3>
            <div class="article-meta">
                <div class="meta-item">
                    <strong>Authors:</strong> ${article.authors.map(a => escapeHtml(a)).join(', ')}
                </div>
                <div class="meta-item">
                    <strong>Year:</strong> ${article.year}
                </div>
                <div class="meta-item">
                    <strong>Type:</strong> ${escapeHtml(article.PaperType)}
                </div>
                <div class="meta-item">
                    <strong>Published in:</strong> ${escapeHtml(article.BookTitleOrJournal)}
                </div>
                ${article.volume ? `<div class="meta-item"><strong>Volume:</strong> ${escapeHtml(article.volume)}</div>` : ''}
                ${article.doi ? `<div class="meta-item"><strong>DOI:</strong> <a href="https://doi.org/${escapeHtml(article.doi)}" target="_blank">${escapeHtml(article.doi)}</a></div>` : ''}
                ${article.tags.length > 0 ? `<div class="meta-item"><strong>Tags:</strong> ${article.tags.map(t => `<span class="tag">${escapeHtml(t)}</span>`).join('')}</div>` : ''}
            </div>
        </div>
        `;
    }).join('');
}

function escapeHtml(text: string): string {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}