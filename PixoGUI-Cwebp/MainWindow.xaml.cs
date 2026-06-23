using Microsoft.Win32;
using PixoGUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixoGUI
{
    public partial class MainWindow : Window
    {
        private readonly ConfigManager _configManager;
        private readonly List<string> _filePaths = new List<string>();
        private bool _isConverting = false;
        private bool _shouldStopConversion = false;

        public MainWindow()
        {
            InitializeComponent();
            _configManager = new ConfigManager();
            UpdateQualityControls();

            this.StateChanged += MainWindow_StateChanged;
            LoadAppIcon();
        }

        private void LoadAppIcon()
        {
            try
            {
                var exePath = AppDomain.CurrentDomain.BaseDirectory;
                var iconPath = System.IO.Path.Combine(exePath, "img", "logo_white.ico");

                if (File.Exists(iconPath))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(iconPath, UriKind.Absolute);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    AppIcon.Source = bitmap;
                }
            }
            catch
            {
                // Se l'icona non esiste o c'è un errore, semplicemente non viene mostrata
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (MaximizeButton == null) return;

            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Content = "◱";
            }
            else
            {
                MaximizeButton.Content = "⬜";
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateQualityControls();
        }

        private void UpdateQualityControls()
        {
            if (QualityTextBox == null || QualityRangeLabel == null) return;

            var selectedFormat = ((ComboBoxItem)FormatComboBox.SelectedItem).Content.ToString().ToLower();

            // Quality is supported for: webp, avif, jpeg, jpg
            bool supportsQuality = selectedFormat == "webp" || selectedFormat == "avif" ||
                                   selectedFormat == "jpeg" || selectedFormat == "jpg";

            QualityTextBox.IsEnabled = supportsQuality;
            QualityRangeLabel.Visibility = supportsQuality ? Visibility.Visible : Visibility.Collapsed;

            if (!supportsQuality)
            {
                QualityTextBox.Text = "";
            }
            else if (string.IsNullOrEmpty(QualityTextBox.Text))
            {
                QualityTextBox.Text = "75";
            }

            // Update range label based on format
            if (supportsQuality)
            {
                if (selectedFormat == "webp")
                {
                    QualityRangeLabel.Text = "Range: 0-100 (0=lossless, 1-100=lossy quality)";
                }
                else if (selectedFormat == "avif")
                {
                    QualityRangeLabel.Text = "Range: 1-100 (higher is better)";
                }
                else // jpeg, jpg
                {
                    QualityRangeLabel.Text = "Range: 1-100 (higher is better)";
                }
            }
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextNumeric(e.Text);
        }

        private bool IsTextNumeric(string text)
        {
            return Regex.IsMatch(text, "^[0-9]+$");
        }

        private void ResizeValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ResizeDimensionComboBox.IsEnabled = !string.IsNullOrWhiteSpace(ResizeValueTextBox.Text);
        }

        private void AddFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.avif;*.ico;*.bmp;*.gif;*.tiff;*.tif|All Files|*.*",
                Title = "Select Images to Convert"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AddFiles(openFileDialog.FileNames);
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _filePaths.Clear();
            FilesTextBox.Text = string.Empty;
            UpdateConvertButtonState();
        }

        private void FilesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateConvertButtonState();
        }

        private void UpdateConvertButtonState()
        {
            if (ConvertButton == null) return;
            ConvertButton.IsEnabled = !string.IsNullOrWhiteSpace(FilesTextBox.Text) && !_isConverting;
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFiles(files);
            }
        }

        private void FilesArea_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void FilesArea_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void FilesArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFiles(files);
                e.Handled = true;
            }
        }

        private void TextBox_PreviewDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void TextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                AddFiles(files);
                e.Handled = true;
            }
        }

        private void AddFiles(string[] files)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".avif", ".ico", ".bmp", ".gif", ".tiff", ".tif" };

            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                if (imageExtensions.Contains(ext) && !_filePaths.Contains(file))
                {
                    _filePaths.Add(file);
                }
            }

            UpdateFilesTextBox();
        }

        private void UpdateFilesTextBox()
        {
            FilesTextBox.Text = string.Join(Environment.NewLine, _filePaths);
            UpdateConvertButtonState();
        }

        private List<string> GetFilePathsFromTextBox()
        {
            var paths = new List<string>();
            var lines = FilesTextBox.Text.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed) && File.Exists(trimmed))
                {
                    paths.Add(trimmed);
                }
            }

            return paths;
        }

        private List<string> GetNonExistentFiles()
        {
            var nonExistent = new List<string>();
            var lines = FilesTextBox.Text.Split(new[] { Environment.NewLine, "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed) && !File.Exists(trimmed))
                {
                    nonExistent.Add(trimmed);
                }
            }

            return nonExistent;
        }

        private void SetControlsEnabled(bool enabled)
        {
            FormatComboBox.IsEnabled = enabled;
            QualityTextBox.IsEnabled = enabled && QualityTextBox.IsEnabled;
            AddDateTimeCheckBox.IsEnabled = enabled;
            ResizeValueTextBox.IsEnabled = enabled;
            ResizeDimensionComboBox.IsEnabled = enabled && ResizeDimensionComboBox.IsEnabled;
            FilesTextBox.IsReadOnly = !enabled;
            AddFilesButton.IsEnabled = enabled;
            ClearButton.IsEnabled = enabled;
        }

        private async void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isConverting)
            {
                // Stop conversion
                _shouldStopConversion = true;
                ConvertButton.Content = "Stopping...";
                ConvertButton.IsEnabled = false;
                return;
            }

            var filesToConvert = GetFilePathsFromTextBox();

            if (filesToConvert.Count == 0)
            {
                MessageBox.Show("No valid image files found. Please check the file paths.", "No Valid Files",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var outputFormat = ((ComboBoxItem)FormatComboBox.SelectedItem).Content.ToString().ToLower();
            int? quality = null;

            if (QualityTextBox.IsEnabled && !string.IsNullOrWhiteSpace(QualityTextBox.Text))
            {
                if (int.TryParse(QualityTextBox.Text, out int q))
                {
                    quality = Math.Clamp(q, 0, 100);
                }
            }

            int? resizeValue = null;
            string resizeDimension = null;

            if (!string.IsNullOrWhiteSpace(ResizeValueTextBox.Text))
            {
                if (int.TryParse(ResizeValueTextBox.Text, out int r))
                {
                    resizeValue = r;
                    resizeDimension = ((ComboBoxItem)ResizeDimensionComboBox.SelectedItem).Content.ToString().ToLower();
                }
            }

            bool addDateTime = AddDateTimeCheckBox.IsChecked == true;

            _isConverting = true;
            _shouldStopConversion = false;
            ConvertButton.Content = "Stop Conversion";
            SetControlsEnabled(false);

            ProgressSection.Visibility = Visibility.Visible;
            ConversionProgressBar.Maximum = filesToConvert.Count;
            ConversionProgressBar.Value = 0;

            var converter = new ImageConverterService(_configManager);
            int successCount = 0;
            int errorCount = 0;
            var errors = new List<string>();
            var remainingFiles = new List<string>();

            for (int i = 0; i < filesToConvert.Count; i++)
            {
                if (_shouldStopConversion)
                {
                    // Add remaining files to the list
                    for (int j = i; j < filesToConvert.Count; j++)
                    {
                        remainingFiles.Add(filesToConvert[j]);
                    }
                    break;
                }

                var filePath = filesToConvert[i];
                ProgressText.Text = $"Converting {i + 1} of {filesToConvert.Count}: {Path.GetFileName(filePath)}";

                try
                {
                    await Task.Run(() => converter.ConvertImage(
                        filePath,
                        outputFormat,
                        quality,
                        addDateTime,
                        resizeValue,
                        resizeDimension
                    ));
                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    errors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                    remainingFiles.Add(filePath);
                }

                ConversionProgressBar.Value = i + 1;
            }

            _isConverting = false;
            ConvertButton.Content = "Start Conversion";
            ConvertButton.IsEnabled = false;
            SetControlsEnabled(true);
            ProgressSection.Visibility = Visibility.Collapsed;

            // Update textarea with remaining files
            if (remainingFiles.Count > 0)
            {
                FilesTextBox.Text = string.Join(Environment.NewLine, remainingFiles);
            }
            else
            {
                FilesTextBox.Text = string.Empty;
            }

            UpdateConvertButtonState();

            var nonExistentFiles = GetNonExistentFiles();
            var message = _shouldStopConversion
                ? $"Conversion stopped by user!\n\nSuccess: {successCount}\nRemaining: {remainingFiles.Count}"
                : $"Conversion completed!\n\nSuccess: {successCount}\nErrors: {errorCount}";

            if (nonExistentFiles.Count > 0)
            {
                message += $"\n\nFiles not found: {nonExistentFiles.Count}";
                if (nonExistentFiles.Count <= 3)
                {
                    message += "\n" + string.Join("\n", nonExistentFiles.Select(f => Path.GetFileName(f)));
                }
            }

            if (errors.Count > 0 && errors.Count <= 5)
            {
                message += "\n\nErrors:\n" + string.Join("\n", errors);
            }
            else if (errors.Count > 5)
            {
                message += "\n\nFirst 5 errors:\n" + string.Join("\n", errors.Take(5));
                message += $"\n... and {errors.Count - 5} more errors.";
            }

            MessageBox.Show(message, _shouldStopConversion ? "Conversion Stopped" : "Conversion Complete",
                MessageBoxButton.OK,
                errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
    }
}