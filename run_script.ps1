
$7zip = "C:\Program Files\7-Zip\7z.exe"
$7zipArg = "x -o./data/external/ ./data/external/dblp.xml.gz"

$destinationDir = "./data/external/"
if (!(Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir -Force
}

echo "Downloading... https://dblp.org/xml/dblp.xml.gz"
Start-BitsTransfer -Source https://dblp.org/xml/dblp.xml.gz -Destination ./data/external/dblp.xml.gz
#Invoke-WebRequest -Uri https://dblp.uni-trier.de/xml/dblp.xml.gz -OutFile ./data/external/dblp.xml.gz

#if (!(Test-Path ./data/external/dblp.xml.gz)){
#    echo "Downloading... https://dblp.uni-trier.de/xml/dblp.xml.gz"
#
#    Invoke-WebRequest -Uri https://dblp.uni-trier.de/xml/dblp.xml.gz -OutFile ./data/external/dblp.xml.gz
#}else{
#    echo "Skipped the download of the file from https://dblp.uni-trier.de/xml/dblp.xml.gz"
#}
Start-Process -FilePath $7zip -ArgumentList $7zipArg -NoNewWindow -Wait

if (!(Test-Path ./data/external/dblp.dtd)){
    echo "Downloading... https://dblp.org/xml/dblp.dtd"

    Invoke-WebRequest -Uri https://dblp.org/xml/dblp.dtd -OutFile ./data/external/dblp.dtd    
}
else{
    echo "Skipped the download of the file from https://dblp.org/xml/dblp.dtd"
}

cd dblp_processor
dotnet build -c Release
cd ..


$dblpProcessor = "./dblp_processor/bin/Release/net6.0/dblp_processor.exe"
$dblpProcessorArg = "dblp --x ./data/external/dblp.xml --u ./data/paper_list.txt --o ./data/stringology_dblp.xml"

$dblpProc = Start-Process -FilePath $dblpProcessor -ArgumentList $dblpProcessorArg -Wait

$arxivProcessor = "./dblp_processor/bin/Release/net6.0/dblp_processor.exe"
$arxivProcessorArg = "arxiv --i ./data/external/arxiv-metadata-oai-snapshot.json --o ./data/cs.DS_arxiv_articles.tsv"
$arxivProc = Start-Process -FilePath $arxivProcessor -ArgumentList $arxivProcessorArg -Wait



ts-node ./scripts/download_arxiv_xml_main.ts
ts-node ./scripts/process_stringology_dblp_main.ts
ts-node ./scripts/weekly_arxiv_main.ts


