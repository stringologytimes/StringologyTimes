
$7zip = "C:\Program Files\7-Zip\7z.exe"
$7zipArg = "x -o./data/external/ ./data/external/dblp.xml.gz"

if (!(Test-Path ./data/external/dblp.xml.gz)){
    Invoke-WebRequest -Uri https://dblp.uni-trier.de/xml/dblp.xml.gz -OutFile ./data/external/dblp.xml.gz
}else{
    echo "Skipped the download of the file from https://dblp.uni-trier.de/xml/dblp.xml.gz"
}
Start-Process -FilePath $7zip -ArgumentList $7zipArg -NoNewWindow -Wait

if (!(Test-Path ./data/external/dblp.dtd)){
    Invoke-WebRequest -Uri https://dblp.uni-trier.de/xml/dblp.dtd -OutFile ./data/external/dblp.dtd    
}
else{
    echo "Skipped the download of the file from https://dblp.uni-trier.de/xml/dblp.dtd"
}

cd dblp_processor
dotnet build -c Release
cd ..

$dblpProcessor = "./dblp_processor/bin/Release/net6.0/dblp_processor.exe"
$dblpProcessorArg = "./data/external/dblp.xml ./data/paper_list.txt ./data/stringology_dblp.xml"

$dblpProc = Start-Process -FilePath $dblpProcessor -ArgumentList $dblpProcessorArg -Wait

