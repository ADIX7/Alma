if((Get-Content env:\GITHUB_REF_TYPE) -ne "tag")
{
    Return
}

$version = (Get-Content env:\GITHUB_REF).Replace("refs/tags/v", "")
$git_sha = (Get-Content env:\GITHUB_SHA).Substring(0, 8)

Write-Host $version
Write-Host $git_sha

(Get-Content src\Alma\Alma.csproj).Replace("0.0.0", $version).Replace("development", $git_sha) | Set-Content src\Alma\Alma.csproj