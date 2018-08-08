# docker run --mount source="c:\dev\Agent\",target=/Sources,type=bind -it microsoft/dotnet:2.1.301-sdk-alpine /Sources/.cdpx/local.sh

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

Remove-Item -Path "$(Get-ScriptDirectory)\..\docker" -Recurse -ErrorAction Ignore
Remove-Item -Path "$(Get-ScriptDirectory)\..\.release" -Recurse -ErrorAction Ignore

docker run -it -v c:\dev\Agent:/Sources microsoft/dotnet:2.1.301-sdk-alpine /bin/sh -c '/Sources/.cdpx/local.sh'

