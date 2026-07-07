# PixoGUI-Cwebp

PixoGUI-Cwebp is a WPF application for image conversion on Windows. 
It provides a graphical interface for converting images between various formats using cwebp and ImageMagick as backend tools.

---

## Features

### Image Conversion

- Conversion to WebP format using cwebp
- Conversion to AVIF, JPEG, JPG, ICO, and other formats using ImageMagick
- Support for multiple input formats: JPG, JPEG, PNG, WEBP, AVIF, ICO, BMP, GIF, TIFF, TIF

### Conversion Options

- Quality adjustment for lossy formats (0-100 for WebP, 1-100 for AVIF/JPEG)
- Resize by width or height
- Option to add timestamp to output filenames
- Batch conversion of multiple images

### User Interface

- Drag and drop support for adding files
- Progress bar for batch operations
- Real-time feedback during conversion
- Ability to stop ongoing conversions
- Display of conversion results and errors

---

## Project Structure

```
PixoGUI-Cwebp/
├── App.xaml, App.xaml.cs              # Application entry point
├── MainWindow.xaml, MainWindow.xaml.cs  # Main application window with UI and conversion logic
├── ImageConverterService.cs           # Image conversion service using cwebp and ImageMagick
├── ConfigManager.cs                   # Configuration management for tool paths
├── AssemblyInfo.cs                    # Assembly information
├── PixoGUI.csproj                     # Visual Studio project file
├── config.ini                         # Configuration file with paths to cwebp and ImageMagick
└── img/                               # Application images and icons
    └── logo_white.ico
```

---

## Requirements

### External Tools

The application requires the following external tools to be present in the specified folders:

- **cwebp.exe**: WebP encoder from Google's libwebp library
  - Default path: `libwebp-1.6.0-windows-x64\bin\`
- **dwebp.exe**: WebP decoder
  - Default path: `-1.6.0-windows-x64\bin\`
- **magick.exe**: ImageMagick command-line tool
  - Default path: `ImageMagick-7.1.2-13-portable-Q16-HDRI-x64\`


## Supported Formats

### Input Formats

- JPG
- JPEG
- PNG
- WEBP
- AVIF
- ICO
- BMP
- GIF
- TIFF
- TIF

### Output Formats

- WebP (using cwebp)
- AVIF (using ImageMagick)
- JPEG/JPG (using ImageMagick)
- ICO (using ImageMagick)
- Other formats supported by ImageMagick

---
## Technical Details

### Technology Stack

- **Language**: C#
- **Framework**: .NET (WPF)
- **IDE**: Visual Studio
- **External Tools**: cwebp, ImageMagick

