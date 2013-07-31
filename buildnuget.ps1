msbuild RJ.RuntimePocoGenerator.sln /p:Configuration="Release" /p:Platform="Any Cpu"

if(!(Test-Path NuGetBuild))
{
	New-Item NuGetBuild -type Directory
}

Remove-Item .\NuGetBuild\* -recurse

New-Item NuGetBuild\lib -type Directory 
Copy-Item .\src\bin\Release\RJ.RuntimePocoGenerator.dll .\NuGetBuild\lib
Copy-Item .\src\bin\Release\RJ.RuntimePocoGenerator.pdb .\NuGetBuild\lib

$assemblyInfo = Get-Content src\Properties\AssemblyInfo.cs
$version = [regex]::Match($assemblyInfo, 'AssemblyVersion\w*\("([\d\.]+)"\w*\)').Groups[1].Value
$nugetspec = Get-Content Package.nuspec
$output = [regex]::Replace($nugetspec, '\$version\$', $version)
$output > NuGetBuild\Package.nuspec

cd NuGetBuild
nuget pack Package.nuspec
cd ..