declare const dblpElements: HTMLElement[];
interface dblpElementInfo {
    node: HTMLElement;
    url: string;
}
declare class RequestCollection {
    collection: dblpElementInfo[];
}
declare function createCollections(info_list: dblpElementInfo[]): RequestCollection[];
declare function addToDBLPElements(nodes: HTMLCollectionOf<Element>, output: HTMLElement[]): void;
declare function node_request2(parent: HTMLElement, button: HTMLElement, url: string): void;
declare function node_request_X(info_collection: RequestCollection): void;
declare const dblpElementsWithURL: dblpElementInfo[];
declare const collections: RequestCollection[];
