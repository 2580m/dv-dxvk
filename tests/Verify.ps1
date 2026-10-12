$ErrorActionPreference = 'Stop'
$project = Split-Path $PSScriptRoot
$package = Join-Path $project 'dist\DVVulkan'
$scratch = Join-Path $PSScriptRoot ('runs\' + [Guid]::NewGuid().ToString('N'))
$root = Join-Path $scratch 'game'
$mod = Join-Path $root 'Mods\DVVulkan'
New-Item -ItemType Directory -Path $mod -Force | Out-Null
Copy-Item "$package\*" $mod -Recurse
[IO.File]::WriteAllBytes((Join-Path $root 'DerailValley.exe'), [byte[]](0))
$worker = Join-Path $mod 'DVVulkan.Worker.exe'
$pending = Join-Path $mod 'pending.txt'
function Assert($condition, $message) { if (!$condition) { throw $message } }
function Apply($action, $expectedCode = 0) {
    [IO.File]::WriteAllText($pending, $action)
    & $worker $root 0 0
    Assert ($LASTEXITCODE -eq $expectedCode) "Unexpected worker exit for $action : $LASTEXITCODE"
}
function Installed {
    foreach ($name in @('d3d11.dll', 'dxgi.dll')) {
        $destination = Join-Path $root $name
        if (!(Test-Path $destination)) { return $false }
        if ((Get-FileHash $destination).Hash -ne (Get-FileHash (Join-Path $mod "dxvk\$name")).Hash) { return $false }
    }
    return $true
}
Apply enable
Assert (Installed) 'Enable did not deploy both official DLLs'
Assert (!(Test-Path $pending)) 'Successful deployment did not consume request'
Apply enable
Assert (Installed) 'Repeated enable changed payload'
Apply disable
Assert (!(Test-Path "$root\d3d11.dll") -and !(Test-Path "$root\dxgi.dll")) 'Disable left DLLs'
Apply disable

[IO.File]::WriteAllText("$root\dxgi.dll", 'foreign DLL sentinel')
Apply enable 1
Assert ([IO.File]::ReadAllText("$root\dxgi.dll") -eq 'foreign DLL sentinel') 'Foreign DLL was overwritten'
Assert (!(Test-Path "$root\d3d11.dll")) 'Conflict caused partial enable'
Assert (Test-Path $pending) 'Failure should retain request for retry'
Apply disable 1
Assert ([IO.File]::ReadAllText("$root\dxgi.dll") -eq 'foreign DLL sentinel') 'Foreign DLL was deleted'
Remove-Item -LiteralPath "$root\dxgi.dll"

[IO.File]::WriteAllText("$mod\dxvk\dxgi.dll", 'corrupt payload')
Apply enable 1
Assert (!(Test-Path "$root\d3d11.dll")) 'Corrupt payload caused partial enable'
Copy-Item "$package\dxvk\dxgi.dll" "$mod\dxvk\dxgi.dll" -Force
Apply invalid 1
Assert (!(Test-Path "$root\d3d11.dll")) 'Invalid action was applied'

# A blocked second file must roll back the first file of an enable operation.
New-Item -ItemType Directory -Path "$root\dxgi.dll" | Out-Null
Apply enable 1
Assert (!(Test-Path "$root\d3d11.dll")) 'Failed enable did not roll back its first DLL'
Remove-Item -LiteralPath "$root\dxgi.dll"

Apply enable
$locked = [IO.File]::Open("$root\dxgi.dll", [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
try {
    Apply disable 1
    Assert (Installed) 'Failed disable did not restore first DLL'
} finally { $locked.Dispose() }
Apply disable

# The latest toggle wins while the helper waits for the original process to exit.
$sleeper = Start-Process -FilePath 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -ArgumentList '-NoProfile -Command Start-Sleep -Seconds 4' -WindowStyle Hidden -PassThru
try {
    [IO.File]::WriteAllText($pending, 'enable')
    $arguments = '"' + $root + '" ' + $sleeper.Id + ' ' + $sleeper.StartTime.ToUniversalTime().Ticks
    $waiting = Start-Process -FilePath $worker -ArgumentList $arguments -WindowStyle Hidden -PassThru
    Start-Sleep -Milliseconds 300
    Assert (!(Test-Path "$root\d3d11.dll")) 'Worker applied before target process exited'
    [IO.File]::WriteAllText($pending, 'disable')
    $waiting.WaitForExit()
    Assert ($waiting.ExitCode -eq 0) 'Waiting worker failed'
    Assert (!(Test-Path "$root\d3d11.dll")) 'Worker ignored latest disable request'
} finally { $sleeper.WaitForExit(); $sleeper.Dispose() }

Add-Type -AssemblyName System.IO.Compression.FileSystem
$info = Get-Content "$package\Info.json" -Raw | ConvertFrom-Json
$zip = [IO.Compression.ZipFile]::OpenRead((Join-Path $project "dist\DVVulkan-$($info.Version).zip"))
try {
    $entries = @($zip.Entries | ForEach-Object { $_.FullName.Replace('\', '/') })
    foreach ($file in @('Info.json', 'DVVulkan.dll', 'DVVulkan.Worker.exe', 'dxvk/d3d11.dll', 'dxvk/dxgi.dll', 'DXVK-LICENSE', 'Disable-DXVK.cmd')) {
        Assert ($entries -contains "DVVulkan/$file") "ZIP missing $file"
    }
    $info = Get-Content "$package\Info.json" -Raw | ConvertFrom-Json
    Assert ($info.Id -eq 'DVVulkan' -and $info.EntryMethod -eq 'DVVulkan.Main.Load') 'Bad UMM metadata'
} finally { $zip.Dispose() }

Apply enable
Apply uninstall
Assert (!(Test-Path "$root\d3d11.dll") -and !(Test-Path "$root\dxgi.dll")) 'Uninstall left root DXVK DLLs'
Assert (Test-Path "$mod\uninstalled.txt") 'Uninstall did not persist its marker'
Assert ((Get-Content "$mod\deployment.log" -Raw) -match 'Applied: uninstall') 'Missing uninstall success log'
Assert (Test-Path "$mod\Info.json") 'Uninstall should leave mod files for UMM removal'
Apply enable
Assert (!(Test-Path "$root\d3d11.dll") -and !(Test-Path "$root\dxgi.dll")) 'Stale enable redeployed DXVK after uninstall'
Remove-Item -LiteralPath "$mod\uninstalled.txt"
Apply enable
Assert (Installed) 'Explicit reinstall failed'
[IO.File]::WriteAllText("$root\dxgi.dll", 'foreign DLL sentinel')
Apply uninstall 1
Assert ([IO.File]::ReadAllText("$root\dxgi.dll") -eq 'foreign DLL sentinel') 'Uninstall deleted a foreign DLL'
Assert (Test-Path "$root\d3d11.dll") 'Conflict caused partial uninstall'
Copy-Item "$package\dxvk\dxgi.dll" "$root\dxgi.dll" -Force
$locked = [IO.File]::Open("$root\dxgi.dll", [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::Read)
try {
    [IO.File]::WriteAllText("$mod\uninstalled.txt", 'uninstall requested')
    Apply uninstall 1
    Assert (Installed) 'Failed uninstall did not roll back'
    Assert (Test-Path $pending) 'Failed uninstall lost its retry request'
} finally { $locked.Dispose() }
Apply uninstall
Assert (!(Test-Path "$root\d3d11.dll") -and !(Test-Path "$root\dxgi.dll")) 'Uninstall retry left DLLs'

# The worker must read the last game language after waiting, not only at startup.
Remove-Item -LiteralPath "$mod\uninstalled.txt"
[IO.File]::WriteAllText("$mod\language.txt", 'en')
Apply enable
$sleeper = Start-Process -FilePath 'C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe' -ArgumentList '-NoProfile -Command Start-Sleep -Seconds 4' -WindowStyle Hidden -PassThru
try {
    [IO.File]::WriteAllText($pending, 'disable')
    $arguments = '"' + $root + '" ' + $sleeper.Id + ' ' + $sleeper.StartTime.ToUniversalTime().Ticks
    $waiting = Start-Process -FilePath $worker -ArgumentList $arguments -WindowStyle Hidden -PassThru
    Start-Sleep -Milliseconds 300
    [IO.File]::WriteAllText("$mod\language.txt", 'zh')
    $waiting.WaitForExit()
    Assert ($waiting.ExitCode -eq 0) 'Localized waiting worker failed'
    Assert ((Get-Content "$mod\deployment.log" -Raw) -match '已应用：停用') 'Worker did not use final game language'
} finally { $sleeper.WaitForExit(); $sleeper.Dispose() }
Apply enable
Apply uninstall
Assert ((Get-Content "$mod\deployment.log" -Raw) -match '已应用：卸载') 'Chinese uninstall log missing'
Remove-Item -LiteralPath "$mod\uninstalled.txt"
[IO.File]::WriteAllText("$root\dxgi.dll", 'foreign DLL sentinel')
Apply enable 1
Assert ((Get-Content "$mod\deployment.log" -Raw) -match '错误：存在其他来源') 'Chinese conflict error missing'

& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:exe "/out:$mod\LanguageTests.exe" "/reference:$worker" "$PSScriptRoot\LanguageTests.cs"
Assert ($LASTEXITCODE -eq 0) 'Language test compilation failed'
& "$mod\LanguageTests.exe" $mod
Assert ($LASTEXITCODE -eq 0) 'Language tests failed'
Write-Output 'PASS: deployment, rollback, process wait, UMM ZIP, uninstall cleanup, persistent uninstall, reinstall, foreign DLL and locked DLL protection.'
