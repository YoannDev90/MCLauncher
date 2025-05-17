using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;

namespace MCLauncher;

public partial class SettingsWindow : Window
{
    // Chemin du fichier de configuration
    private readonly string _settingsFilePath;

    // Configuration actuelle
    private readonly AppSettings _currentSettings;

    public SettingsWindow()
    {
        InitializeComponent();

        // Définition du chemin des paramètres
        _settingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MCLauncher",
            "settings.json");

        // Chargement des paramètres
        _currentSettings = LoadSettings();

        // Configuration après le chargement de la fenêtre
        Loaded += SettingsWindow_Loaded;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SettingsWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Configuration des contrôles
            SetupJavaControls();
            SetupMemoryControls();
            SetupThemeControls();
            SetupDeveloperModeControls();
            SetupCheckboxControls();

            // Mise à jour des informations système
            UpdateSystemInfo();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de l'initialisation des paramètres: {ex}");
        }
    }

    private void SetupJavaControls()
    {
        // Configuration du bouton de recherche de Java
        var browseButton = this.FindControl<Button>("BrowseButton");
        if (browseButton != null) browseButton.Click += BrowseButton_Click;

        // Configuration du bouton de détection automatique
        var autoDetectButton = this.FindControl<Button>("AutoDetectButton");
        if (autoDetectButton != null) autoDetectButton.Click += AutoDetectJava;

        // Affichage du chemin Java actuel
        var javaPathTextBox = this.FindControl<TextBox>("JavaPathTextBox");
        if (javaPathTextBox != null && !string.IsNullOrEmpty(_currentSettings.JavaPath))
            javaPathTextBox.Text = _currentSettings.JavaPath;
    }

    private void SetupMemoryControls()
    {
        // Configuration du slider mémoire
        var memorySlider = this.FindControl<Slider>("MemorySlider");
        var memoryValueText = this.FindControl<TextBlock>("MemoryValueText");
        var memoryProgressBar = this.FindControl<ProgressBar>("MemoryProgressBar");

        if (memorySlider != null && memoryValueText != null && memoryProgressBar != null)
        {
            // Définir la valeur initiale
            memorySlider.Value = _currentSettings.MemoryAllocationGB;
            memoryProgressBar.Value = _currentSettings.MemoryAllocationGB;
            UpdateMemoryText(_currentSettings.MemoryAllocationGB);

            // Configurer l'événement de changement
            memorySlider.ValueChanged += (s, args) =>
            {
                var value = (int)args.NewValue;
                UpdateMemoryText(value);
                memoryProgressBar.Value = value;
                _currentSettings.MemoryAllocationGB = value;
                SaveSettings();
            };
        }
    }

    private void UpdateMemoryText(int memoryValue)
    {
        var memoryValueText = this.FindControl<TextBlock>("MemoryValueText");
        if (memoryValueText != null) memoryValueText.Text = $"{memoryValue} Go alloués";
    }

    private void SetupThemeControls()
    {
        // Configuration de la ComboBox du thème
        var themeComboBox = this.FindControl<ComboBox>("ThemeComboBox");
        if (themeComboBox != null)
        {
            themeComboBox.SelectedIndex = _currentSettings.ThemeIndex;
            themeComboBox.SelectionChanged += (s, args) =>
            {
                _currentSettings.ThemeIndex = themeComboBox.SelectedIndex;
                SaveSettings();
            };
        }
    }

    private void SetupDeveloperModeControls()
    {
        // Configuration de la CheckBox du mode développeur
        var devModeCheckBox = this.FindControl<CheckBox>("DevModeCheckBox");
        var devModeOptionsPanel = this.FindControl<StackPanel>("DevModeOptionsPanel");

        if (devModeCheckBox != null && devModeOptionsPanel != null)
        {
            devModeCheckBox.IsChecked = _currentSettings.DeveloperMode;
            devModeOptionsPanel.IsVisible = _currentSettings.DeveloperMode;

            devModeCheckBox.IsCheckedChanged += (s, args) =>
            {
                var isChecked = devModeCheckBox.IsChecked ?? false;
                devModeOptionsPanel.IsVisible = isChecked;
                _currentSettings.DeveloperMode = isChecked;
                SaveSettings();
            };
        }
    }

    private void SetupCheckboxControls()
    {
        // CheckBox pour fermer après le lancement
        var closeAfterLaunchCheckBox = this.FindControl<CheckBox>("CloseAfterLaunchCheckBox");
        if (closeAfterLaunchCheckBox != null)
        {
            closeAfterLaunchCheckBox.IsChecked = _currentSettings.CloseAfterLaunch;
            closeAfterLaunchCheckBox.IsCheckedChanged += (s, args) =>
            {
                _currentSettings.CloseAfterLaunch = closeAfterLaunchCheckBox.IsChecked ?? false;
                SaveSettings();
            };
        }

        // CheckBox pour afficher la sortie de débogage
        var showDebugOutputCheckBox = this.FindControl<CheckBox>("ShowDebugOutputCheckBox");
        if (showDebugOutputCheckBox != null)
        {
            showDebugOutputCheckBox.IsChecked = _currentSettings.ShowDebugOutput;
            showDebugOutputCheckBox.IsCheckedChanged += (s, args) =>
            {
                _currentSettings.ShowDebugOutput = showDebugOutputCheckBox.IsChecked ?? false;
                SaveSettings();
            };
        }
    }

    private void UpdateSystemInfo()
    {
        // Mise à jour des informations sur la mémoire disponible
        var availableMemoryText = this.FindControl<TextBlock>("AvailableMemoryText");
        var memoryProgressBar = this.FindControl<ProgressBar>("MemoryProgressBar");

        if (availableMemoryText != null && memoryProgressBar != null)
            try
            {
                var totalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
                var totalMemoryGB = totalMemoryMB / 1024;
                availableMemoryText.Text = $"{totalMemoryGB} Go disponibles";

                // Mettre à jour la valeur maximale de la barre de progression
                memoryProgressBar.Maximum = totalMemoryGB;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la récupération de la mémoire: {ex.Message}");
                availableMemoryText.Text = "-- Go disponibles";
            }
    }

    private void OnLucasLinkClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            var url = "https://lthb.fr/";
            var psi = new ProcessStartInfo(url) { UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Impossible d'ouvrir l'URL: {ex.Message}");
        }
    }

    private void OnGitHubLinkClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            var url = "https://github.com/YoannDev90/MCLauncher";

    private void OnYoannLinkClicked(object? sender, RoutedEventArgs e)
    {
        try
        {
            var url = "https://github.com/YoannDev90";

            var psi = new ProcessStartInfo(url) { UseShellExecute = true };
            Process.Start(psi);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Impossible d'ouvrir l'URL: {ex.Message}");
        }
    }

    private void SelectJavaPath(object? sender, RoutedEventArgs e)
    {
        SelectJavaPathAsync();
    }

    private async void SelectJavaPathAsync()
    {
        try
        {
            var javaFileType = new FilePickerFileType("Exécutable Java") { Patterns = new[] { "*.exe" } };
            var allFileType = new FilePickerFileType("Tous les fichiers") { Patterns = new[] { "*" } };

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Sélectionner l'exécutable Java",
                AllowMultiple = false,
                FileTypeFilter = new[] { javaFileType, allFileType }
            });

            if (files != null && files.Count > 0)
            {
                var path = files[0].Path.LocalPath;
                var javaPathTextBox = this.FindControl<TextBox>("JavaPathTextBox");
                if (javaPathTextBox != null)
                {
                    javaPathTextBox.Text = path;
                    _currentSettings.JavaPath = path;
                    SaveSettings();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error selecting Java path: {ex.Message}");
        }
    }

    private void AutoDetectJava(object? sender, RoutedEventArgs e)
    {
        var javaPath = FindJavaPath();

        var javaPathTextBox = this.FindControl<TextBox>("JavaPathTextBox");
        if (javaPath != null && javaPathTextBox != null)
        {
            javaPathTextBox.Text = javaPath;
            _currentSettings.JavaPath = javaPath;
            SaveSettings();
        }
        else
        {
            Debug.WriteLine("Java non détecté automatiquement");
        }
    }

    private string? FindJavaPath()
    {
        try
        {
            // Tenter de localiser Java à partir des variables d'environnement
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");

            if (!string.IsNullOrEmpty(javaHome))
            {
                var javaBin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine(javaHome, "bin", "java.exe")
                    : Path.Combine(javaHome, "bin", "java");

                if (File.Exists(javaBin))
                    return javaBin;
            }

            // Vérifier si java est dans le PATH
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "where",
                            Arguments = "java",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    var output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        string[] paths = output.Split(new[] { Environment.NewLine },
                            StringSplitOptions.RemoveEmptyEntries);
                        if (paths.Length > 0 && File.Exists(paths[0]))
                            return paths[0];
                    }
                }
                else
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "which",
                            Arguments = "java",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };

                    process.Start();
                    var output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output) && File.Exists(output))
                        return output;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la recherche de Java dans le PATH: {ex.Message}");
            }

            // Vérifier dans les chemins communs
            string[] commonPaths = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new[]
                {
                    @"C:\Program Files\Java",
                    @"C:\Program Files (x86)\Java",
                    @"C:\Program Files\Eclipse Adoptium"
                }
                : new[]
                {
                    "/usr/bin/java",
                    "/usr/local/bin/java",
                    "/opt/jdk/bin/java"
                };

            foreach (var path in commonPaths)
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (Directory.Exists(path))
                        // Parcourir les sous-répertoires pour trouver Java
                        foreach (var dir in Directory.GetDirectories(path))
                        {
                            var javaBin = Path.Combine(dir, "bin", "java.exe");
                            if (File.Exists(javaBin))
                                return javaBin;
                        }
                }
                else if (File.Exists(path))
                {
                    return path;
                }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de la recherche de Java: {ex}");
        }

        return null;
    }

    // Chargement des paramètres depuis le fichier
    private AppSettings LoadSettings()
    {
        try
        {
            // Créer le répertoire s'il n'existe pas
            var directory = Path.GetDirectoryName(_settingsFilePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Vérifier si le fichier existe
            if (File.Exists(_settingsFilePath))
            {
                var json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null) return settings;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors du chargement des paramètres: {ex.Message}");
        }

        // Retourner les paramètres par défaut si le fichier n'existe pas ou en cas d'erreur
        return new AppSettings();
    }

    // Sauvegarde des paramètres dans le fichier
    private void SaveSettings()
    {
        try
        {
            // Créer le répertoire s'il n'existe pas
            var directory = Path.GetDirectoryName(_settingsFilePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            // Sérialiser et sauvegarder les paramètres
            var json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de la sauvegarde des paramètres: {ex.Message}");
        }
    }

    private async void BrowseButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Use StorageProvider API instead of OpenFileDialog
            var javaExeExtensions = new List<string> { "java.exe", "javaw.exe" };
            var allFilesExtensions = new List<string> { "*" };

            var javaFileType = new FilePickerFileType("Exécutable Java") { Patterns = javaExeExtensions };
            var allFileType = new FilePickerFileType("Tous les fichiers") { Patterns = allFilesExtensions };

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Sélectionner Java",
                AllowMultiple = false,
                FileTypeFilter = new[] { javaFileType, allFileType }
            });

            if (files != null && files.Count > 0)
            {
                var javaPath = files[0].Path.LocalPath;
                _currentSettings.JavaPath = javaPath;
                SaveSettings();

                // Update the Java path TextBox
                var javaPathTextBox = this.FindControl<TextBox>("JavaPathTextBox");
                if (javaPathTextBox != null) javaPathTextBox.Text = javaPath;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error selecting Java path: {ex.Message}");
        }
    }

    private void UpdateAvailableJavaVersions()
    {
        var javaVersionsComboBox = this.FindControl<ComboBox>("JavaVersionsComboBox");
        if (javaVersionsComboBox != null)
        {
            // Create a list of Java versions
            var versions = new List<string> { "Java 8", "Java 11", "Java 17", "Java 21" };

            // Use ItemsSource instead of Items
            javaVersionsComboBox.ItemsSource = versions;

            var selectedVersion = _currentSettings.SelectedJavaVersion;
            if (selectedVersion != null && versions.Contains(selectedVersion))
                javaVersionsComboBox.SelectedItem = selectedVersion;
        }
    }

    private void SaveProfileSettings()
    {
        var profileNameTextBox = this.FindControl<TextBox>("ProfileNameTextBox");
        var usernameTextBox = this.FindControl<TextBox>("UsernameTextBox");

        if (profileNameTextBox != null && usernameTextBox != null)
        {
            // Fix nullable warnings by using null coalescing operators
            var profileName = profileNameTextBox.Text ?? string.Empty;
            var username = usernameTextBox.Text ?? string.Empty;

            _currentSettings.ProfileName = profileName;
            _currentSettings.Username = username;
            SaveSettings();
        }
    }
}

// Classe pour stocker les paramètres - renommée pour éviter les conflits
public class AppSettings
{
    // Paramètres généraux
    public string JavaPath { get; set; } = "";
    public int MemoryAllocationGB { get; set; } = 4;
    public int ThemeIndex { get; set; }

    // Paramètres Minecraft
    public bool CloseAfterLaunch { get; set; }
    public bool ShowDebugOutput { get; set; }
    public bool DeveloperMode { get; set; }

    // Nouvelles propriétés
    public string? SelectedJavaVersion { get; set; }
    public string ProfileName { get; set; } = "";
    public string Username { get; set; } = "";
}