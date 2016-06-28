Properties {
    $rootDir = Split-Path $psake.build_script_file
    $solutionFile = "$rootDir\EnjoyCQRS.sln"
    $packagesDir = "$rootDir\packages"
    $nugetExe = "$packagesDir\NuGet.CommandLine.3.3.0\tools\NuGet.exe"

    $configuration = $env:CONFIGURATION

    $enjoyPath = "$rootDir\src\EnjoyCQRS"
    $nuspecEnjoy = "$enjoyPath\EnjoyCQRS.nuspec"
    $nupkgPathEnjoy = "$rootDir\src\EnjoyCQRS.{0}.nupkg"

    $enjoyTestFrameworkPath = "$rootDir\src\EnjoyCQRS.TestFramework"
    $nuspecTestFramework = "$enjoyTestFrameworkPath\EnjoyCQRS.TestFramework.nuspec"
    $nupkgPathTestFramework = "$rootDir\src\EnjoyCQRS.TestFramework.{0}.nupkg"

    $enjoyMongoDBPath = "$rootDir\src\EnjoyCQRS.EventStore.MongoDB"
    $nuspecMongoDB = "$enjoyMongoDBPath\EnjoyCQRS.EventStore.MongoDB.nuspec"
    $nupkgPathMongoDB = "$rootDir\src\EnjoyCQRS.EventStore.MongoDB.{0}.nupkg"

    $enjoySQLitePath = "$rootDir\src\EnjoyCQRS.EventStore.SQLite"
    $nuspecSQLite = "$enjoySQLitePath\EnjoyCQRS.EventStore.SQLite.nuspec"
    $nupkgPathSQLite = "$rootDir\src\EnjoyCQRS.EventStore.SQLite.{0}.nupkg"

    $target_unit_dll = "$rootDir\test\EnjoyCQRS.UnitTests\bin\$configuration\EnjoyCQRS.UnitTests.dll"
    $target_integration_dll = "$rootDir\test\EnjoyCQRS.IntegrationTests\bin\$configuration\EnjoyCQRS.IntegrationTests.dll"
    $target_owin_integration_dll = "$rootDir\test\EnjoyCQRS.Owin.IntegrationTests\bin\$configuration\EnjoyCQRS.Owin.IntegrationTests.dll"
    $target_sqlite_integration_dll = "$rootDir\test\EnjoyCQRS.SQLite.IntegrationTests\bin\$configuration\EnjoyCQRS.SQLite.IntegrationTests.dll"
    $target_mongodb_integration_dll = "$rootDir\test\EnjoyCQRS.MongoDB.IntegrationTests\bin\$configuration\EnjoyCQRS.MongoDB.IntegrationTests.dll"
    
    $logDir = ".\log"
    $outputXml = "$logDir\CodeCoverageResults.xml"
    $coverageReportDir = "$logDir\codecoverage\"

    $isAppVeyor = $env:APPVEYOR -eq $true
    $buildNumber = $env:APPVEYOR_BUILD_NUMBER
    $isRelease = $isAppVeyor -and (($env:APPVEYOR_REPO_BRANCH -eq "master") -or ($env:APPVEYOR_REPO_TAG -eq "true"))
}

Task Default -depends Build, Test-Coverage

Task Rebuild -depends Clean, Build

Task Build -depends Prepare-Build, Build-Only

Task Build-Only {
    if ($isAppVeyor) {
        Exec { msbuild $solutionFile /m /verbosity:minimal /p:Configuration=$configuration /logger:"C:\Program Files\AppVeyor\BuildAgent\Appveyor.MSBuildLogger.dll" }
    } else {
        Exec { msbuild $solutionFile /m /verbosity:minimal /p:Configuration=$configuration }
    }
}

Task Prepare-Build -depends Restore, Update-Nuspec

Task Clean {
    Exec { msbuild $solutionFile /t:Clean /v:quiet }
}

Task Restore {
    RestorePkgs $solutionFile
}

Task Update-Nuspec -precondition { $isAppVeyor -and ($isRelease -ne $true) } -depends Update-Enjoy-Nuspec, Update-TestFramework-Nuspec, Update-MongoDB-Nuspec, Update-SQLite-Nuspec

Task Update-Enjoy-Nuspec -precondition { $isAppVeyor -and ($isRelease -ne $true) } {
    UpdateNuspec $nuspecEnjoy
}

Task Update-TestFramework-Nuspec -precondition { $isAppVeyor -and ($isRelease -ne $true) } {
    UpdateNuspec $nuspecTestFramework
}

Task Update-MongoDB-Nuspec -precondition { $isAppVeyor -and ($isRelease -ne $true) } {
    UpdateNuspec $nuspecMongoDB
}

Task Update-SQLite-Nuspec -precondition { $isAppVeyor -and ($isRelease -ne $true) } {
    UpdateNuspec $nuspecSQLite
}

Task Pack-Nuget -precondition { return $isAppVeyor } -depends Pack-Enjoy-Nuget, Pack-TestFramework-Nuget, Pack-MongoDB-Nuget, Pack-SQLite-Nuget

Task Pack-Enjoy-Nuget {
    PackNuget "$rootDir\src" $nuspecEnjoy $nupkgPathEnjoy
}

Task Pack-TestFramework-Nuget {
    PackNuget "$rootDir\src" $nuspecTestFramework $nupkgPathTestFramework
}

Task Pack-MongoDB-Nuget {
    PackNuget "$rootDir\src" $nuspecMongoDB $nupkgPathMongoDB
}

Task Pack-SQLite-Nuget {
    PackNuget "$rootDir\src" $nuspecSQLite $nupkgPathSQLite
}

Task Test-Coverage -depends Set-Log {
    RunTestWithCoverage $target_unit_dll, $target_integration_dll, $target_owin_integration_dll, $target_sqlite_integration_dll, $target_mongodb_integration_dll
}

Task Set-Log {
    
    if (Test-Path $logDir) {
        Remove-Item -Recurse -Force $logDir
    }

    if ((Test-Path $logDir) -eq $false)
    {
        Write-Host -ForegroundColor DarkBlue "Creating log directory $logDir"
        mkdir $logDir | Out-Null
    }
}

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

function RestorePkgs($sln) {
    Write-Host -ForegroundColor Green "Restoring $sln..."
    . $nugetExe restore $sln
}

function PackNuget($dir, $nuspecFile, $nupkgFile) {
    [xml]$xml = cat $nuspecFile
    
    $projectId = $xml.package.metadata.id
    Write-Host "Packing nuget for $projectId..."
    
    $nupkgFile = $nupkgFile -f $xml.package.metadata.version
    Write-Host "Nupkg path is $nupkgFile"
    . $nugetExe pack $nuspecFile -OutputDirectory $dir
    
    ls $nupkgFile
    
    Write-Host "Nuget packed for $projectId!"
    Write-Host "Pushing nuget artifact for $projectId..."
    
    appveyor PushArtifact $nupkgFile
  
    Write-Host "Nupkg pushed for $projectId!"
}

function UpdateNuspec($nuspecPath) {
    [xml]$xml = cat $nuspecPath
    
    $packageId = $xml.package.metadata.id
    Write-Host "Updating version in nuspec file for $packageId to $buildNumber"

    $xml.package.metadata.version+=".$buildNumber"
    Write-Host "Nuspec version will be $($xml.package.metadata.version)"

    $xml.Save($nuspecPath)

    Write-Host "Nuspec saved for $packageId!"
}

function RunTestWithCoverage($fullTestDllPaths) {

    $reportGeneratorExe = (Resolve-Path "$rootDir\packages\ReportGenerator.*\tools\ReportGenerator.exe").ToString()
    $openCoverExe = (Resolve-Path "$rootDir\packages\OpenCover.*\tools\OpenCover.Console.exe").ToString()
    $coverallsNetExe = (Resolve-Path "$rootDir\packages\coveralls.io.*\tools\coveralls.net.exe").ToString()
    $xunitConsoleExe = (Resolve-Path "$rootDir\packages\xunit.runner.console.*\tools\xunit.console.x86.exe").ToString()

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

        Write-Host -ForegroundColor Green "Adding to OpenCover target arguments: " $fullTestDllPath

        $targetArgs += $fullTestDllPath + " "
    }

    $targetArgs = $targetArgs.Substring(0, $targetArgs.Length - 1)

    Write-Host -ForegroundColor Green "OpenCover target arguments: " $targetArgs

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