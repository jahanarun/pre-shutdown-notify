
$Stringarray = @()
$newarray = @()
$value = get-ItemProperty -Path "hklm:SYSTEM\CurrentControlSet\Control" -Name PreshutdownOrder 
$Stringarray = $value.Preshutdownorder

$newval = "preshutdownnotify"

[boolean]$NewServiceexists = $true
foreach ($item in $Stringarray){
        if ($item -like "preshutdownnotify")
        {
            $NewServiceexists = $false
        }
}

If ($NewServiceexists) {
    $newarray += $newval
    $newarray += $Stringarray
    Set-ItemProperty -Path "hklm:SYSTEM\CurrentControlSet\Control" -Name "PreshutdownOrder" -type MultiString -Value $newarray
    $newarray
}


$findPreshutdownnotify = get-service -name preshutdownnotify -ErrorAction SilentlyContinue

If ($findPreshutdownnotify -like ""){
}
Else {
    stop-service -name preshutdownnotify -Force
    Invoke-Expression -Command "sc.exe delete preshutdownnotify"
}

$despath = "C:\Program Files\preshutdownnotify"

$foldercheck = Test-Path "$despath" -PathType Container

If (-not $foldercheck){
    New-Item -Path $despath -ItemType Directory
}

$sourcePath = "."
Get-ChildItem -Path $sourcePath | % { 
  Copy-Item $_.fullname "$despath" -Recurse -Force -Exclude @("install.ps1") 
}

$SCpath = $despath


Invoke-Expression -Command 'sc.exe create preshutdownnotify DisplayName="Pre-shutdown Notification Service" binpath="C:\Program Files\preshutdownnotify\preshutdownnotify.exe" type=own start=auto'