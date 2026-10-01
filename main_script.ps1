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


###### CS STEP

$doiProcessor = "./data_formatter/bin/Release/net9.0/data_formatter"

Write-Host "Compile: $dblpProcessor" -ForegroundColor Yellow
cd data_formatter
dotnet build -c Release
cd ..    

$doiProcessorArgsY = @("--data", "./data", "--skip_build", "--mode", "dblp_proceedings_preprocessor")
Write-Host "Execute: $doiProcessor $doiProcessorArgsY" -ForegroundColor Yellow
$dblpProcY = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgsY -Wait    


$doiProcessorArgsX = @("--data", "./data", "--skip_build", "--mode", "build_big_cache")
Write-Host "Execute: $doiProcessor $doiProcessorArgsX" -ForegroundColor Yellow
$dblpProcX = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgsX -Wait    

$doiProcessorArgsA = @("--data", "./data", "--skip_build", "--mode", "build_small_cache")
Write-Host "Execute: $doiProcessor $doiProcessorArgsA" -ForegroundColor Yellow
$dblpProcA = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgsA -Wait    


$doiProcessorArgsB = @("--data", "./data", "--skip_build", "--mode", "build_doi_element_dictionary")
Write-Host "Execute: $doiProcessor $doiProcessorArgsB" -ForegroundColor Yellow
$dblpProcB = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgsB -Wait    

$doiProcessorArgsC = @("--data", "./data", "--skip_build", "--mode", "modify_doi_element_dictionary")
Write-Host "Execute: $doiProcessor $doiProcessorArgsC" -ForegroundColor Yellow
$dblpProcC = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgsC -Wait    

$doiProcessorArgsE = @("--data", "./data", "--skip_build", "--mode", "create_lightweight_doi_info_folder")
Write-Host "Execute: $doiProcessor $doiProcessorArgsE" -ForegroundColor Yellow
$dblpProcE = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgsE -Wait    

###### FINAL STEP

Write-Host "Copying folder: ./data/auto_generated/result/lightweight_doi_info to ./stringology_explorer/lightweight_doi_info" -ForegroundColor Yellow
$sourceFolder = "./data/auto_generated/result/lightweight_doi_info"
$destinationFolder = "./stringology_explorer"
if (Test-Path $sourceFolder) {
    Copy-Item $sourceFolder $destinationFolder -Recurse -Force
    Write-Host "Copied folder: $sourceFolder to $destinationFolder" -ForegroundColor Green
} else {
    Write-Host "Source folder $sourceFolder does not exist. Skipping copy." -ForegroundColor Red
}

Write-Host "Execute: npm run build in ./stringology_explorer" -ForegroundColor Yellow


cd ./stringology_explorer
npm run build
cd ..





