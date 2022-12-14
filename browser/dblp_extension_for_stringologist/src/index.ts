//document.body.style.border = "5px solid red";

console.log("Hello");


import {preprocessDBLP} from "./dblp_preprocessor"

const dblpElements: HTMLElement[] = []

preprocessDBLP(dblpElements);






//addToDBLPElements(document.body.getElementsByClassName("entry inproceedings toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry informal toc"), dblpElements);
//addToDBLPElements(document.body.getElementsByClassName("entry article toc"), dblpElements);
