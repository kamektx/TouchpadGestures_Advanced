$resourcePath = "TouchpadGestures_Advanced.csproj"
$resourceText = Get-Content -Encoding UTF8 -Raw $resourcePath
$resourceText = [regex]::replace($resourceText, "(<Version>\d+\.\d+\.)(\d+)", { $args.groups[1].value + [string](1 + [int]$args.groups[2].value) })
Set-Content -Encoding UTF8 -Value $resourceText -Path $resourcePath