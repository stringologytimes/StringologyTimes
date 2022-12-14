//document.body.style.border = "5px solid red";
import {preprocessDBLP} from "./dblp_preprocessor"
import {preprocessStringologyTimes} from "./stringology_times_processor"

if(window.location.host == "dblp.org"){
    preprocessDBLP();
}else if(window.location.pathname.indexOf("weekly_arxiv") != -1){
    preprocessStringologyTimes();
}







//addToDBLPElements(document.body.getElementsByClassName("entry inproceedings toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry informal toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry article toc"), dblpElements);
