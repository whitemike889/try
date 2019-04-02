function Get-ScriptDirectory
{
  $Invocation = (Get-Variable MyInvocation -Scope 1).Value
  Split-Path $Invocation.MyCommand.Path
}

Foreach ($file in $(Get-ChildItem -Path $(Get-ScriptDirectory))) 
{
    $file = (Convert-Path $file)
    $text = [IO.File]::ReadAllText($file) -replace "`r`n", "`n"
    [IO.File]::WriteAllText($file, $text)
}

$scriptDir = (Get-ScriptDirectory)
$agentDir = (get-item $scriptDir ).parent.FullName.ToLower()

Remove-Item -Path "$(Get-ScriptDirectory)\..\docker" -Recurse -ErrorAction Ignore
Remove-Item -Path "$(Get-ScriptDirectory)\..\.release" -Recurse -ErrorAction Ignore

Copy-Item "$env:APPDATA\NuGet\NuGet.Config" "$(Get-ScriptDirectory)\..\User-NuGet.Config.tmp"

Write-Output "Mounting directory ${agentDir} in docker" 
docker run -it -v ${agentDir}:/Sources mcr.microsoft.com/dotnet/core/sdk:3.0-alpine /bin/sh -c '/Sources/.cdpx/local.sh'

t-rex.exe --path "$(Get-ScriptDirectory)/.." 
