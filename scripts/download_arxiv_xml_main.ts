const fs = require('fs');
import { loadArxivXML, ArxivXMLInfo } from "./basic_functions/arxiv_xml"
import { load_arxiv_ids, downloadAllArxivInfomation } from "./basic_functions/download_arxiv_xml"


const arxivXMLPath = "data/arxiv.xml";
const arxivXMLInfo : ArxivXMLInfo = loadArxivXML(arxivXMLPath);
const urlPath = "data/paper_list.txt";
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

