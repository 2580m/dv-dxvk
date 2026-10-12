param([string]$GameDirectory = 'W:\SteamLibrary\steamapps\common\Derail Valley')
$ErrorActionPreference = 'Stop'
$project = $PSScriptRoot
$compiler = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$managed = Join-Path $GameDirectory 'DerailValley_Data\Managed'
$output = Join-Path $project 'dist\DVVulkan'
New-Item -ItemType Directory -Path "$output\dxvk" -Force | Out-Null
Copy-Item "$project\package\*" $output -Force
Copy-Item "$project\vendor\dxvk-3.1.1\x64\d3d11.dll", "$project\vendor\dxvk-3.1.1\x64\dxgi.dll" "$output\dxvk" -Force
Copy-Item "$project\vendor\DXVK-LICENSE", "$project\vendor\DXVK-README.md" $output -Force
& $compiler /nologo /target:library /optimize+ "/out:$output\DVVulkan.dll" "/reference:$managed\UnityModManager\UnityModManager.dll" "/reference:$managed\UnityEngine.CoreModule.dll" "/reference:$managed\UnityEngine.IMGUIModule.dll" "/reference:$managed\I2.Localization.dll" "$project\src\Main.cs" "$project\src\Deployment.cs" "$project\src\Text.cs"
if ($LASTEXITCODE -ne 0) { throw 'Mod compilation failed' }
& $compiler /nologo /target:exe /optimize+ "/out:$output\DVVulkan.Worker.exe" "$project\src\Worker.cs" "$project\src\Deployment.cs" "$project\src\Text.cs"
if ($LASTEXITCODE -ne 0) { throw 'Worker compilation failed' }
$version = (Get-Content "$project\package\Info.json" -Raw | ConvertFrom-Json).Version
$zip = Join-Path $project "dist\DVVulkan-$version.zip"
Compress-Archive -Path $output -DestinationPath $zip -Force
Get-Item $zip | Select-Object FullName, Length
