

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

export function load_compressed_file(url: string): Promise<string> {
    return fetch(url)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Failed to fetch file: ${response.statusText}`);
            }
            return response.arrayBuffer();
        })
        .then(buffer => {
            // @ts-ignore
            const pako = (globalThis as any).pako || (window as any).pako; // Assume pako is available in global scope
            if (!pako) {
                throw new Error('pako library not loaded. Please include pako in your HTML.');
            }
            const compressed = new Uint8Array(buffer);
            const decompressed = pako.ungzip(compressed);
            // Decode to UTF-8 text
            const decoder = new TextDecoder('utf-8');
            return decoder.decode(decompressed);
        });
}