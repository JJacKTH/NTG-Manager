$path = Join-Path $PSScriptRoot "Properties\AssemblyInfo.cs"
if (Test-Path $path) {
    $content = Get-Content $path -Raw
    if ($content -match 'AssemblyFileVersion\("(\d+\.\d+\.\d+)\.(\d+)"\)') {
        $baseVer = $matches[1]
        $rev = [int]$matches[2] + 1
        $newVer = "$baseVer.$rev"
        $content = $content -replace 'AssemblyFileVersion\("[^"]+"\)', "AssemblyFileVersion(`"$newVer`")"
        $content = $content -replace 'AssemblyVersion\("[^"]+"\)', "AssemblyVersion(`"$newVer`")"
        [System.IO.File]::WriteAllText($path, $content)
        Write-Host "Auto-updated version to $newVer"
    }
}
