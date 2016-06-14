function Import-Psake {
    $psakeModule = "$PSScriptRoot\packages\psake.4.5.0\tools\psake.psm1"
    if ((Test-Path $psakeModule) -ne $true) {
        . $nugetExe restore $PSScriptRoot\.nuget\packages.config -SolutionDirectory $PSScriptRoot
    }
    Import-Module $psakeModule -force
}

$nugetExe = "$PSScriptRoot\.nuget\nuget.exe"

if ((Test-Path $nugetExe) -ne $true) {
    wget "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $PSScriptRoot\.nuget\nuget.exe

    $nugetExe = "$PSScriptRoot\.nuget\nuget.exe"
}

Import-Psake

if ($MyInvocation.UnboundArguments.Count -ne 0) {
    . $PSScriptRoot\psake.ps1 -taskList ($MyInvocation.UnboundArguments -join " ")
}
else {
    . $PSScriptRoot\build.ps1 Build
}

exit !($psake.build_success)