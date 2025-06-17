$ErrorActionPreference = "Stop"

dotnet tool restore
dotnet build

AddToPath .\Pinicola.HotKeys\bin\Debug\
