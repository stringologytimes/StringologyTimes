
export class URLConverter{
    private static isArxivAbsURL(urlBody : string) : boolean{
        return urlBody.indexOf("arxiv.org/abs/") == 0;
    }
    private static isDOIURL(urlBody : string) : boolean{
        return urlBody.indexOf("doi.org/10.") == 0;
    }
    private static isHTTPURL(url : string) : boolean{
        return url.indexOf("http://") == 0;
    }
    private static isHTTPSURL(url : string) : boolean{
        return url.indexOf("https://") == 0;
    }
    public static isDOI(url_or_doi: string) : boolean{
        return url_or_doi.indexOf("10.") == 0;
    }

    public static convertToDOI(url_or_doi: string): string | null {
        
        if(URLConverter.isDOI(url_or_doi)){
            return url_or_doi;
        }else{
            let urlBody : string | null = null;
            if(URLConverter.isHTTPURL(url_or_doi)){
                urlBody = url_or_doi.substring("http://".length);
            }else if(URLConverter.isHTTPSURL(url_or_doi)){
                urlBody = url_or_doi.substring("https://".length);
            }else{
                urlBody = null;
            }

            if(urlBody != null){
                if(URLConverter.isArxivAbsURL(urlBody)){
                    const parse = urlBody.split("/");
                    let suf = "";
                    for(let i = 2; i < parse.length; i++){
                        suf += parse[i];
                        if(i < parse.length - 1){
                            suf += "/";
                        }
                    }
                    return "10.48550/arXiv." + suf;
                }else if (URLConverter.isDOIURL(urlBody)){
                    return urlBody.substring("doi.org/".length);
                }else{
                    return null;
                }    
            }else{
                return null;
            }
        }
    }
}