param (
	[string]$configuration
)

$reportGeneratorExe = (Resolve-Path "packages\ReportGenerator.*\tools\ReportGenerator.exe").ToString()
$openCoverExe = (Resolve-Path "packages\OpenCover.*\tools\OpenCover.Console.exe").ToString()
$coverallsNetExe = (Resolve-Path "packages\coveralls.io.*\tools\coveralls.net.exe").ToString()
$xunitConsoleExe = (Resolve-Path "packages\xunit.runner.console.*\tools\xunit.console.x86.exe").ToString()

$isAppVeyor = $env:APPVEYOR -eq $true

$packages_folder = '.\packages'
$test_folder = ".\test"

$target_unit_dll = "$test_folder\EnjoyCQRS.UnitTests\bin\$configuration\EnjoyCQRS.UnitTests.dll"
$target_integration_dll = "$test_folder\EnjoyCQRS.IntegrationTests\bin\$configuration\EnjoyCQRS.IntegrationTests.dll"

$logDir = ".\log"
$outputXml = "$logDir\CodeCoverageResults.xml"
$coverageReportDir = "$logDir\codecoverage\"

If (Test-Path $logDir) {
    Remove-Item -Recurse -Force $logDir
}

mkdir $logDir | Out-Null

function TestPath($paths) {
    $notFound = @()
    foreach($path in $paths) {
        if ((Test-Path $path) -eq $false)
        {
            $notFound += $path
        }
    }
    $notFound
}

function RunTestWithCoverage($fullTestDllPaths) {
    $notFoundPaths = TestPath $openCoverExe, $xunitConsoleExe, $reportGeneratorExe
    if ($notFoundPaths.length -ne 0) {
        Write-Host -ForegroundColor Red "Paths not found: "
        foreach($path in $notFoundPaths) {
            Write-Host -ForegroundColor Red "  $path"
        }
        throw "Paths for test executables not found"
    }
    $targetArgs = ""
    Foreach($fullTestDllPath in $fullTestDllPaths) {
        $targetArgs += $fullTestDllPath + " "
    }
    $targetArgs = $targetArgs.Substring(0, $targetArgs.Length - 1)
    $appVeyor = ""
    if ($isAppVeyor) {
        $appVeyor = " -appveyor"
    }
    $arguments = '-register:user', "`"-target:$xunitConsoleExe`"", "`"-targetargs:$targetArgs $appVeyor -noshadow -parallel none -nologo`"", "`"-filter:+[Enjoy*]*`"", "`"-output:$outputXml`"", '-coverbytest:EnjoyCQRS.*Tests.dll', '-log:All', '-returntargetcode'
    
    &$openCoverExe $arguments

    Write-Host -ForegroundColor Green "Exporting code coverage report"
    
    &$reportGeneratorExe -verbosity:Info -reports:$outputXml -targetdir:$coverageReportDir

    if ($env:COVERALLS_REPO_TOKEN -ne $null) {
        
        Write-Host -ForegroundColor Green "Uploading coverage report to Coveralls.io"

        &$coverallsNetExe --opencover $outputXml --full-sources
    }
}

RunTestWithCoverage $target_unit_dll, $target_integration_dll