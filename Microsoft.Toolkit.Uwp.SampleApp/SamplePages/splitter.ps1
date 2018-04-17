# Make sure we're in the right path - https://blogs.msdn.microsoft.com/powershell/2007/06/19/get-scriptdirectory-to-the-rescue/ 
function Get-ScriptDirectory 
{ 
    $Invocation = (Get-Variable MyInvocation -Scope 1).Value 
    Split-Path $Invocation.MyCommand.Path 
} 

$curpath = Get-Location # save user's path 
# Set our path to be in the SamplePages folder 
$scriptroot = Get-ScriptDirectory #(Join-Path (Get-ScriptDirectory) "SamplePages") 

Set-Location -Path $scriptroot

$samples = [System.IO.File]::ReadAllText((Join-Path $scriptroot "samples.json")) | ConvertFrom-Json 

$samples | ForEach-Object {
    $category = $_.Name
    $_.Samples | ForEach-Object {
        IF (Test-Path $_.Name) {
            if (Get-Member -InputObject $_ -Name "CodeFile" -MemberType Properties) {
                $before = "" + $_.CodeFile
                $_.CodeFile = $_.CodeFile -replace ".bind",".code"

                <#Push-Location $_.Name
                # rename .bind 'code' files to .code files
                $cmd = "git mv " + $before + " " + $_.CodeFile
                Invoke-Expression $cmd

                Pop-Location#>
            }

            $_ | Add-Member -Name "Category" -Value $category -MemberType NoteProperty

            # save definition to sample directory
            $text = $_ | ConvertTo-Json -Depth 4
            [System.IO.File]::WriteAllText((Join-Path (Join-Path $scriptroot $_.Name) "definition.json"), $text)
        }
    }
}
