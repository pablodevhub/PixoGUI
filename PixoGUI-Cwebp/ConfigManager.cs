using System;
using System.IO;
using System.Linq;

namespace PixoGUI
{
    public class ConfigManager
    {
        private const string ConfigFileName = "config.ini";
        private string _cwebpPath;
        private string _dwebpPath;
        private string _imageMagickPath;

        public ConfigManager()
        {
            LoadConfiguration();
        }

        private void LoadConfiguration()
        {
            var exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = Path.Combine(exeDirectory, ConfigFileName);

            // Default paths
            _cwebpPath = Path.Combine(exeDirectory, "webp", "cwebp.exe");
            _dwebpPath = Path.Combine(exeDirectory, "webp", "dwebp.exe");
            _imageMagickPath = Path.Combine(exeDirectory, "imagemagick", "magick.exe");

            if (File.Exists(configPath))
            {
                try
                {
                    var lines = File.ReadAllLines(configPath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                            continue;

                        var parts = line.Split('=').Select(p => p.Trim()).ToArray();
                        if (parts.Length != 2) continue;

                        var key = parts[0].ToLower();
                        var value = parts[1];

                        switch (key)
                        {
                            case "cwebp_folder":
                                _cwebpPath = Path.Combine(exeDirectory, value, "cwebp.exe");
                                break;
                            case "dwebp_folder":
                                _dwebpPath = Path.Combine(exeDirectory, value, "dwebp.exe");
                                break;
                            case "imagemagick_folder":
                                _imageMagickPath = Path.Combine(exeDirectory, value, "magick.exe");
                                break;
                        }
                    }
                }
                catch
                {
                    // Use default paths if config reading fails
                }
            }
        }

        public string GetCwebpPath() => _cwebpPath;
        public string GetDwebpPath() => _dwebpPath;
        public string GetImageMagickPath() => _imageMagickPath;
    }
}