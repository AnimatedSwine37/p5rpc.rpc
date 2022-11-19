# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/p5r.rpc/*" -Force -Recurse
dotnet publish "./p5r.rpc.csproj" -c Release -o "$env:RELOADEDIIMODS/p5r.rpc" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location