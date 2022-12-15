//document.body.style.border = "5px solid red";
import {preprocessDBLP} from "./dblp_preprocessor"
import {preprocessWeeklyArxiv} from "./weekly_arxiv"
import {preprocessWeeklyArxivTop} from "./weekly_arxiv_top"


if(window.location.host == "dblp.org"){
    preprocessDBLP();
}else if(window.location.pathname.indexOf("weekly_arxiv/") != -1){
    preprocessWeeklyArxiv();
}else if(window.location.pathname.indexOf("weekly_arxiv_top") != -1){
    console.log(window.location.pathname);

    preprocessWeeklyArxivTop();
}







//addToDBLPElements(document.body.getElementsByClassName("entry inproceedings toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry informal toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry article toc"), dblpElements);
