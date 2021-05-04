$resourcePath = "TGA_NativeMessaging_Cliant.rc"
$resourceText = Get-Content -Encoding Default -Raw $resourcePath
$resourceText = [regex]::replace($resourceText, "(FILEVERSION\s+\d+,\s*\d+,\s*)(\d+),\s*(\d+)", { $args.groups[1].value + [string](1 + [int]$args.groups[2].value) + ",0" })
$resourceText = [regex]::replace($resourceText, "(PRODUCTVERSION\s+\d+,\s*\d+,\s*)(\d+),\s*(\d+)", { $args.groups[1].value + [string](1 + [int]$args.groups[2].value) + ",0" })
Set-Content -Encoding Default -Value $resourceText -Path $resourcePath