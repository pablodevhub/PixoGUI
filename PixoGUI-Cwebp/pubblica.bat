@echo off
del /s /q .\publish
dotnet publish --self-contained -r win-x64 -o .\publish
xcopy .\bin\Debug\net10.0-windows\ImageMagick-7.1.2-13-portable-Q16-HDRI-x64 .\publish\ImageMagick-7.1.2-13-portable-Q16-HDRI-x64 /E /I
xcopy .\bin\Debug\net10.0-windows\libwebp-1.6.0-windows-x64 .\publish\libwebp-1.6.0-windows-x64 /E /I
