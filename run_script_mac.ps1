param(
    [switch]$ForceCompile = $false
)

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

#$skipDownload = $true;
#$localFile = "./data/external/dblp.xml.gz"

#if(!$skipDownload){
## Download dblp.xml file if needed.
#$remoteUrl = "https://dblp.org/xml/dblp.xml.gz"
#try {
#    Write-Host "Retrieving the last modified date of dblp.xml.gz on the web" -ForegroundColor Yellow
#    
#    $response = Invoke-WebRequest -Uri $remoteUrl -Method Head
#    $remoteLastModified = $response.Headers["Last-Modified"]
#    $remoteDate = [datetime]::ParseExact($remoteLastModified, "R", $null)
#} catch {
#    Write-Host "Failed to retrieve the last modified date of the remote file." -ForegroundColor Red
#    exit
#}
#
#}

#if (Test-Path $localFile) {
#    $localDate = (Get-Item $localFile).LastWriteTime
#} else {
#    $localDate = [datetime]::MinValue
#}
#if ($remoteDate -gt $localDate) {
#    Write-Host "Downloading... $remoteUrl" -ForegroundColor Yellow
#    Write-Host "The remote file is up-to-date. Starting the download." -ForegroundColor Yellow
#    Invoke-WebRequest -Uri $remoteUrl -OutFile $localFile
#} else {
#    Write-Host "The local file is up-to-date. Download is not required." -ForegroundColor Green
#}

#Expand-Gzip -sourceFile $sourceFile -destinationFile $destinationFile

#if (!(Test-Path ./data/external/dblp.dtd)){
#    echo "Downloading... https://dblp.org/xml/dblp.dtd"
#    Invoke-WebRequest -Uri https://dblp.org/xml/dblp.dtd -OutFile ./data/external/dblp.dtd    
#}
#else{
#    echo "Skipped the download of the file from https://dblp.org/xml/dblp.dtd"
#}

Write-Host "Execute: ts-node ./scripts/process_user_url_and_doi_lists_main.ts" -ForegroundColor Yellow
ts-node ./scripts/process_user_url_and_doi_lists_main.ts

Write-Host "Execute: ts-node ./scripts/process_user_tag_lists_main.ts" -ForegroundColor Yellow
ts-node ./scripts/process_user_tag_lists_main.ts

## Compute the hash of the url.csv file
Write-Host "Computing the hash of the url.csv file" -ForegroundColor Yellow
$urlHashInfo = Get-FileHash -LiteralPath "./data/auto_generated/url.csv" -Algorithm SHA256
$urlHashPath = "./data/auto_generated/url.csv.sha256"

$dir = Split-Path -Path $urlHashPath -Parent
if ($dir -and -not (Test-Path -LiteralPath $dir)) {
    New-Item -ItemType Directory -Path $dir | Out-Null
}

$previousUrlHash = $null;
if (-not (Test-Path -LiteralPath $urlHashPath -PathType Leaf)) {
    $previousUrlHash = $null;
} else {
    $previousUrlHash = Get-Content -LiteralPath $urlHashPath -Encoding UTF8
}
$urlUpdated = $false;
if ($previousUrlHash -ne $urlHashInfo.Hash) {
    $urlUpdated = $true;
    $urlHashInfo.Hash | Set-Content -LiteralPath $urlHashPath -Encoding UTF8
}

### Compute the hash of the dblp.xml file
#Write-Host "Computing the hash of the dblp.xml file" -ForegroundColor Yellow
#$dblpHashInfo = Get-FileHash -LiteralPath "./data/external/dblp.xml" -Algorithm SHA256
#$dblpHashPath = "./data/auto_generated/dblp.xml.sha256"
#$previousDblpHash = $null;
#if (-not (Test-Path -LiteralPath $dblpHashPath -PathType Leaf)) {
#    $previousDblpHash = $null;
#} else {
#    $previousDblpHash = Get-Content -LiteralPath $dblpHashPath -Encoding UTF8
#}
#$DBLPUpdated = $false;
#if ($previousDblpHash -ne $dblpHashInfo.Hash) {
#    $DBLPUpdated = $true;
#    $dblpHashInfo.Hash | Set-Content -LiteralPath $dblpHashPath -Encoding UTF8
#}


## Compute the hash of arxiv-metadata-oai-snapshot.json file
Write-Host "Computing the hash of the arxiv-metadata-oai-snapshot.json file" -ForegroundColor Yellow
$arxivHashInfo = Get-FileHash -LiteralPath "./data/external/arxiv-metadata-oai-snapshot.json" -Algorithm SHA256
$arxivHashPath = "./data/auto_generated/arxiv-metadata-oai-snapshot.json.sha256"
$previousArxivHash = $null;
if (-not (Test-Path -LiteralPath $arxivHashPath -PathType Leaf)) {
    $previousArxivHash = $null;
} else {
    $previousArxivHash = Get-Content -LiteralPath $arxivHashPath -Encoding UTF8
}
$arxivUpdated = $false;
if ($previousArxivHash -ne $arxivHashInfo.Hash) {
    $arxivUpdated = $true;
    $arxivHashInfo.Hash | Set-Content -LiteralPath $arxivHashPath -Encoding UTF8
}

Write-Host "Is url.csv updated? $urlUpdated" -ForegroundColor Yellow
#Write-Host "Is dblp.xml updated? $DBLPUpdated" -ForegroundColor Yellow
Write-Host "Is arxiv-metadata-oai-snapshot.json updated? $arxivUpdated" -ForegroundColor Yellow

$doiProcessor = "./doi_processor/bin/Release/net9.0/doi_processor"
$doiProcessorArgs = @("-d", "./data")

if ($urlUpdated -or $arxivUpdated) {
    Write-Host "Compile: $dblpProcessor" -ForegroundColor Yellow
    cd doi_processor
    dotnet build -c Release
    cd ..    
}

Write-Host "`$ForceCompile = $ForceCompile" -ForegroundColor Yellow


if ($urlUpdated -or $arxivUpdated -or $ForceCompile) {
    Write-Host "Execute: $doiProcessor $doiProcessorArgs" -ForegroundColor Yellow
    $dblpProc = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgs -Wait    
}else{
    Write-Host "Skip: $doiProcessor $doiProcessorArgs" -ForegroundColor Green
}

#Write-Host "Copy: ./data/auto_generated/stringology_dblp.jsonl to ./docs/output/jsonl/stringology_dblp.jsonl" -ForegroundColor Yellow
#Copy-Item "./data/auto_generated/stringology_dblp.jsonl" "./docs/output/jsonl/stringology_dblp.jsonl"

## tsc -p ./scripts/browser/tsconfig.json



##
## $arxivProcessor = "./dblp_processor/bin/Release/net9.0/dblp_processor"
## $arxivProcessorArgs = @("arxiv", "--i", "./data/external/arxiv-metadata-oai-snapshot.json", "--o", "./data/auto_generated/cs.DS_arxiv_articles.tsv")
## if ($urlUpdated -or $DBLPUpdated -or $arxivUpdated) {
##     Write-Host "Execute: $arxivProcessor $arxivProcessorArgs" -ForegroundColor Yellow
##     $arxivProc = Start-Process -FilePath $arxivProcessor -ArgumentList $arxivProcessorArgs -Wait
## }else{
##     Write-Host "Skip: $arxivProcessor $arxivProcessorArgs" -ForegroundColor Green
## }
## 
## Write-Host "Execute: ts-node ./scripts/download_arxiv_xml_main.ts" -ForegroundColor Yellow
## ts-node ./scripts/download_arxiv_xml_main.ts
## 
## Write-Host "Execute: ts-node ./scripts/process_stringology_dblp_main.ts" -ForegroundColor Yellow
## ts-node ./scripts/process_stringology_dblp_main.ts
## 
## Write-Host "Execute: ts-node ./scripts/weekly_arxiv_main.ts" -ForegroundColor Yellow
## ts-node ./scripts/weekly_arxiv_main.ts


