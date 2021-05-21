
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



Copy-Item -Path ".\*.exe" -Destination "$despath" -force
Copy-Item -Path ".\*.dll" -Destination "$despath" -force
copy-item -path ".\*.pdb"  -Destination "$despath" -force
Copy-Item -Path ".\the-script-to-run-on-preshutdown.ps1" -Destination "$despath" -force

$SCpath = $despath


Invoke-Expression -Command 'sc.exe create preshutdownnotify DisplayName="Pre-shutdown Notification Service" binpath="C:\Program Files\preshutdownnotify\preshutdownnotify.exe" type=own start=auto'