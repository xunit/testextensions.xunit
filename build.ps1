param(
    [string]$target = "Build",
    [string]$verbosity = "minimal"
)

if (test-path "env:\ProgramFiles(x86)") {
    $path = join-path ${env:ProgramFiles(x86)} "MSBuild\14.0\bin\MSBuild.exe"
    if (test-path $path) {
        $msbuild = $path
    }
}
if ($msbuild -eq $null) {
    $path = join-path $env:ProgramFiles "MSBuild\14.0\bin\MSBuild.exe"
    if (test-path $path) {
        $msbuild = $path
    }
}
if ($msbuild -eq $null) {
    throw "Could not find MSBuild v14.0."
}

$allArgs = @("VsTestExtensions.xUnit.proj", "/m", "/nologo", "/verbosity:$verbosity", "/t:$target", $args)
& $msbuild $allArgs
