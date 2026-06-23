using PixoGUI;
using System;
using System.Diagnostics;
using System.IO;

namespace PixoGUI
{
    public class ImageConverterService
    {
        private readonly ConfigManager _configManager;

        public ImageConverterService(ConfigManager configManager)
        {
            _configManager = configManager;
        }

        public void ConvertImage(string inputPath, string outputFormat, int? quality,
            bool addDateTime, int? resizeValue, string resizeDimension)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Input file not found", inputPath);

            var outputPath = GenerateOutputPath(inputPath, outputFormat, addDateTime);

            // Use cwebp for WebP output, dwebp for reading WebP, ImageMagick for everything else
            if (outputFormat.ToLower() == "webp")
            {
                ConvertToWebP(inputPath, outputPath, quality, resizeValue, resizeDimension);
            }
            else
            {
                // All other formats use ImageMagick
                ConvertWithImageMagick(inputPath, outputPath, outputFormat, quality, resizeValue, resizeDimension);
            }
        }

        private string GenerateOutputPath(string inputPath, string outputFormat, bool addDateTime)
        {
            var directory = Path.GetDirectoryName(inputPath);
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(inputPath);

            if (addDateTime)
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                fileNameWithoutExt = $"{fileNameWithoutExt}_{timestamp}";
            }

            return Path.Combine(directory, $"{fileNameWithoutExt}.{outputFormat}");
        }

        private void ConvertToWebP(string inputPath, string outputPath, int? quality,
            int? resizeValue, string resizeDimension)
        {
            var cwebpPath = _configManager.GetCwebpPath();

            if (!File.Exists(cwebpPath))
                throw new FileNotFoundException("cwebp.exe not found. Please ensure it's in the webp folder.", cwebpPath);

            var args = $"\"{inputPath}\" -o \"{outputPath}\"";

            if (quality.HasValue)
            {
                args += $" -q {quality.Value}";
            }

            if (resizeValue.HasValue)
            {
                if (resizeDimension == "width")
                    args += $" -resize {resizeValue.Value} 0";
                else
                    args += $" -resize 0 {resizeValue.Value}";
            }

            RunProcess(cwebpPath, args);
        }

        private void ConvertWithImageMagick(string inputPath, string outputPath, string outputFormat,
            int? quality, int? resizeValue, string resizeDimension)
        {
            var magickPath = _configManager.GetImageMagickPath();

            if (!File.Exists(magickPath))
                throw new FileNotFoundException("ImageMagick (magick.exe) not found. Please ensure it's in the imagemagick folder.", magickPath);

            var args = $"\"{inputPath}\"";

            // Handle resize
            if (resizeValue.HasValue)
            {
                if (resizeDimension == "width")
                    args += $" -resize {resizeValue.Value}x";
                else
                    args += $" -resize x{resizeValue.Value}";
            }

            // Handle quality for formats that support it
            if (quality.HasValue)
            {
                var fmt = outputFormat.ToLower();
                if (fmt == "avif" || fmt == "jpeg" || fmt == "jpg")
                {
                    args += $" -quality {quality.Value}";
                }
            }

            // Special handling for ICO format
            if (outputFormat.ToLower() == "ico")
            {
                // ICO files benefit from specific size
                var icoSize = resizeValue ?? 256;
                if (!resizeValue.HasValue)
                {
                    args += $" -resize {icoSize}x{icoSize}";
                }
                args += " -define icon:auto-resize=256,128,64,48,32,16";
            }

            args += $" \"{outputPath}\"";

            RunProcess(magickPath, args);
        }

        private void RunProcess(string fileName, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(processStartInfo))
            {
                if (process == null)
                    throw new Exception($"Failed to start process: {fileName}");

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    var error = process.StandardError.ReadToEnd();
                    var output = process.StandardOutput.ReadToEnd();
                    var errorMessage = !string.IsNullOrEmpty(error) ? error : output;
                    throw new Exception($"Conversion failed (Exit code: {process.ExitCode}): {errorMessage}");
                }
            }
        }
    }
}