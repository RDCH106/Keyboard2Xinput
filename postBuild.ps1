$ErrorActionPreference = "Stop";
$TargetDir='Keyboard2XinputGui\bin\Release\'
$buildDir="build"
$distDir="$buildDir\dist"
$docDir="$buildDir\doc"

# Create directories
if ((test-path $buildDir) ) {
    rm -r -Force $buildDir
}
mkdir $buildDir
mkdir $distDir
mkdir $docDir

# generate HTML from markdown
Write-Host "Generating HTML documentation"
Write-Host "Using Python: $env:PYTHON"
& $env:PYTHON\python.exe md2Html\md2Html.py
Write-Host "Generating HTML documentation: Done."

Copy-Item .\samples\ -Destination .\build\doc\ -Recurse

# create zip
$zipPath="$distDir\Keyboard2Xinput.zip"
Write-Host "Generating $zipPath"
$compress = @{
Path = "$TargetDir*.dll", "$TargetDir*.exe", "dist\*.config", "$TargetDir*.ini", "samples\mappings\I-PAC2\mapping.ini", "build\doc", "samples"
CompressionLevel = "Fastest"
DestinationPath = "$zipPath"
}
Compress-Archive @compress
