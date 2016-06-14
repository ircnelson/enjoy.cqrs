Import-Module $PSScriptRoot\packages\psake.4.5.0\tools\psake.psm1 -force
Invoke-Expression("Invoke-psake -framework '4.6.1' build.targets.ps1 " + $MyInvocation.UnboundArguments -join " ")
exit !($psake.build_success)