import { loadArxivXML, ArxivXMLInfo } from "./basic_functions/arxiv_xml"
import { load_arxiv_ids, downloadAllArxivInfomation } from "./basic_functions/download_arxiv_xml"
import * as fs from 'fs' 

/**
 * url.csvに記載されたarXivの論文に対応するメタデータをhttp://export.arxiv.org/api/query?id_list=XXXを使ってダウンロード、そしてfiltered_arxiv.xmlに保存
 */


const arxivXMLPath = "data/auto_generated/filtered_arxiv.xml";
const arxivXMLInfo : ArxivXMLInfo = loadArxivXML(arxivXMLPath);
const urlPath = "data/auto_generated/url.csv";
const id_array = load_arxiv_ids(urlPath);
const new_id_array = id_array.filter((id) => !arxivXMLInfo.dic.has(id));


if (new_id_array.length > 0) {
    downloadAllArxivInfomation(new_id_array, arxivXMLInfo);
}

try {
    fs.writeFileSync(arxivXMLPath, arxivXMLInfo.document.toString());
    console.log(`Write ${arxivXMLPath}`);

} catch (e) {
    console.log(e);
}

