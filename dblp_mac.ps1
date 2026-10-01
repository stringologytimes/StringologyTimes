param(
    [switch]$ForceCompile = $false
)

$7zip = "C:\Program Files\7-Zip\7z.exe"
$7zipArg = "x -o./data/external/ ./data/external/dblp.xml.gz"

[string]$sourceFile = "./data/external/dblp.xml.gz"
[string]$destinationFile = "./data/external/dblp.xml"



$destinationDir = "./data/external/"
if (!(Test-Path -Path $destinationDir)) {
    New-Item -ItemType Directory -Path $destinationDir -Force
}

$doiProcessor = "./data_formatter/bin/Release/net9.0/data_formatter"
$doiProcessorArgs = @("--data", "./data", "--skip_build", "--mode", "dblp_proceedings_preprocessor")
#$doiProcessorArgs = @("--data", "./data", "--skip_build", "--mode", "dblp_proceedings_processor")

#$doiProcessorArgs = @("--data", "./data")

Write-Host "Compile: $dblpProcessor" -ForegroundColor Yellow
cd data_formatter
dotnet build -c Release
cd ..    

Write-Host "Execute: $doiProcessor $doiProcessorArgs" -ForegroundColor Yellow
$dblpProc = Start-Process -FilePath $doiProcessor -ArgumentList $doiProcessorArgs -Wait    



