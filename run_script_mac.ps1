
$7zip = "C:\Program Files\7-Zip\7z.exe"
$7zipArg = "x -o./data/external/ ./data/external/dblp.xml.gz"

[string]$sourceFile = "./data/external/dblp.xml.gz"
[string]$destinationFile = "./data/external/dblp.xml"

# Open Gzip file
function Expand-Gzip {
    param (
        [string]$sourceFile,
        [string]$destinationFile
    )

    [System.IO.FileStream]$sourceStream = [System.IO.File]::OpenRead($sourceFile)
    [System.IO.FileStream]$destinationStream = [System.IO.File]::Create($destinationFile)
    [System.IO.Compression.GzipStream]$decompressionStream = New-Object System.IO.Compression.GzipStream($sourceStream, [System.IO.Compression.CompressionMode]::Decompress)
    
    $buffer = New-Object byte[] 4096
    while (($read = $decompressionStream.Read($buffer, 0, $buffer.Length)) -gt 0) {
        $destinationStream.Write($buffer, 0, $read)
    }

    $decompressionStream.Close()
    $sourceStream.Close()
    $destinationStream.Close()
}


$destinationDir = "./data/external/"
if (!(Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir -Force
}

# Download dblp.xml file if needed.
$remoteUrl = "https://dblp.uni-trier.de/xml/dblp.xml.gz"
$localFile = "./data/external/dblp.xml.gz"
try {
    Write-Output "Retrieving the last modified date of dblp.xml.gz on the web"
    
    $response = Invoke-WebRequest -Uri $remoteUrl -Method Head
    $remoteLastModified = $response.Headers["Last-Modified"]
    $remoteDate = [datetime]::ParseExact($remoteLastModified, "R", $null)
} catch {
    Write-Output "Failed to retrieve the last modified date of the remote file."
    exit
}

if (Test-Path $localFile) {
    $localDate = (Get-Item $localFile).LastWriteTime
} else {
    $localDate = [datetime]::MinValue
}
if ($remoteDate -gt $localDate) {
    echo "Downloading... $remoteUrl"

    Write-Output "The remote file is up-to-date. Starting the download."
    Invoke-WebRequest -Uri $remoteUrl -OutFile $localFile
} else {
    Write-Output "The local file is up-to-date. Download is not required."
}


Expand-Gzip -sourceFile $sourceFile -destinationFile $destinationFile


if (!(Test-Path ./data/external/dblp.dtd)){
    echo "Downloading... https://dblp.uni-trier.de/xml/dblp.dtd"
    Invoke-WebRequest -Uri https://dblp.uni-trier.de/xml/dblp.dtd -OutFile ./data/external/dblp.dtd    
}
else{
    echo "Skipped the download of the file from https://dblp.uni-trier.de/xml/dblp.dtd"
}

cd dblp_processor
dotnet build -c Release
cd ..


$dblpProcessor = "./dblp_processor/bin/Release/net9.0/dblp_processor"
$dblpProcessorArgs = @("dblp", "--x", "./data/external/dblp.xml", "--u", "./data/paper_list.txt", "--o", "./data/stringology_dblp.xml")
$dblpProc = Start-Process -FilePath $dblpProcessor -ArgumentList $dblpProcessorArgs -Wait

$arxivProcessor = "./dblp_processor/bin/Release/net9.0/dblp_processor"
$arxivProcessorArgs = @("arxiv", "--i", "./data/external/arxiv-metadata-oai-snapshot.json", "--o", "./data/cs.DS_arxiv_articles.tsv")
$arxivProc = Start-Process -FilePath $arxivProcessor -ArgumentList $arxivProcessorArgs -Wait



ts-node ./scripts/download_arxiv_xml_main.ts
ts-node ./scripts/process_stringology_dblp_main.ts
ts-node ./scripts/weekly_arxiv_main.ts


