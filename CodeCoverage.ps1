param (
	[string]$configuration
)

$packages_folder = '.\packages'
$test_folder = ".\test"

$opencover_console = (Resolve-Path "packages\OpenCover.*\tools\OpenCover.Console.exe").ToString()
$xunit_runner_console = (Resolve-Path "packages\xunit.runner.console.*\tools\xunit.console.x86.exe").ToString()

$report_folder = ".\Reports"

$target_unit_dll = "$test_folder\EnjoyCQRS.UnitTests\bin\$configuration\EnjoyCQRS.UnitTests.dll"
$target_integration_dll = "$test_folder\EnjoyCQRS.IntegrationTests\bin\$configuration\EnjoyCQRS.IntegrationTests.dll"

If (Test-Path $report_folder) {
    Remove-Item -Recurse -Force $report_folder
}

mkdir $report_folder | Out-Null

&$opencover_console `
    -register:user `
    -threshold:1 `
    -returntargetcode `
    -hideskipped:All `
    -filter:"+[Enjoy*]*" `
    -excludebyattribute:*.ExcludeFromCodeCoverage* `
    -output:"$report_folder\OpenCover.Unit.xml" `
    -target:"$xunit_runner_console" `
    -targetargs:"$target_unit_dll -noshadow"

&$opencover_console `
    -register:user `
    -threshold:1 `
    -returntargetcode `
    -hideskipped:All `
    -filter:"+[Enjoy*]*" `
    -excludebyattribute:*.ExcludeFromCodeCoverage* `
    -output:"$report_folder\OpenCover.Integration.xml" `
    -target:"$xunit_runner_console" `
    -targetargs:"$target_integration_dll -noshadow"

$env:Path = "C:\Python27;C:\Python27\Scripts;$env:Path"
python -m pip install --upgrade pip
pip install codecov
&{codecov -f ".\Reports\OpenCover.Unit.xml" ".\Reports\OpenCover.Integration.xml" -X gcov}