using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace MCLauncher;

public partial class SimpleSettingsWindow : Window
{
    // Chemin du fichier de configuration
    private readonly string _settingsFilePath;
    
    // Configuration actuelle
    private AppSettings _currentSettings;
    
    public SimpleSettingsWindow()
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
        this.Loaded += SimpleSettingsWindow_Loaded;
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void SimpleSettingsWindow_Loaded(object? sender, RoutedEventArgs e)
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
        if (browseButton != null)
        {
            browseButton.Click += SelectJavaPath;
        }
        
        // Configuration du bouton de détection automatique
        var autoDetectButton = this.FindControl<Button>("AutoDetectButton");
        if (autoDetectButton != null)
        {
            autoDetectButton.Click += AutoDetectJava;
        }
        
        // Affichage du chemin Java actuel
        var javaPathTextBox = this.FindControl<TextBox>("JavaPathTextBox");
        if (javaPathTextBox != null && !string.IsNullOrEmpty(_currentSettings.JavaPath))
        {
            javaPathTextBox.Text = _currentSettings.JavaPath;
        }
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
                int value = (int)args.NewValue;
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
        if (memoryValueText != null)
        {
            memoryValueText.Text = $"{memoryValue} Go alloués";
        }
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
                bool isChecked = devModeCheckBox.IsChecked ?? false;
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
        {
            try
            {
                long totalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
                long totalMemoryGB = totalMemoryMB / 1024;
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
    }
    
    private void OnLuckyLinkClicked(object? sender, RoutedEventArgs e)
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
    
    private void SelectJavaPath(object? sender, RoutedEventArgs e)
    {
        SelectJavaPathAsync();
    }
    
    private async void SelectJavaPathAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Sélectionner l'exécutable Java",
            AllowMultiple = false
        };
        
        // Définir les filtres en fonction du système d'exploitation
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            dialog.Filters.Add(new FileDialogFilter { Name = "Exécutable Java", Extensions = { "exe" } });
        }
        
        var result = await dialog.ShowAsync(this);
        
        if (result != null && result.Length > 0)
        {
            string path = result[0];
            var javaPathTextBox = this.FindControl<TextBox>("JavaPathTextBox");
            if (javaPathTextBox != null)
            {
                javaPathTextBox.Text = path;
                _currentSettings.JavaPath = path;
                SaveSettings();
            }
        }
    }
    
    private void AutoDetectJava(object? sender, RoutedEventArgs e)
    {
        string? javaPath = FindJavaPath();
        
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
            string? javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            
            if (!string.IsNullOrEmpty(javaHome))
            {
                string javaBin = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
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
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    if (!string.IsNullOrEmpty(output))
                    {
                        string[] paths = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
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
                    string output = process.StandardOutput.ReadToEnd().Trim();
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
                ? new[] { 
                    @"C:\Program Files\Java", 
                    @"C:\Program Files (x86)\Java",
                    @"C:\Program Files\Eclipse Adoptium"
                  }
                : new[] { 
                    "/usr/bin/java", 
                    "/usr/local/bin/java",
                    "/opt/jdk/bin/java"
                  };
                
            foreach (var path in commonPaths)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (Directory.Exists(path))
                    {
                        // Parcourir les sous-répertoires pour trouver Java
                        foreach (var dir in Directory.GetDirectories(path))
                        {
                            string javaBin = Path.Combine(dir, "bin", "java.exe");
                            if (File.Exists(javaBin))
                                return javaBin;
                        }
                    }
                }
                else if (File.Exists(path))
                {
                    return path;
                }
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
            string directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Vérifier si le fichier existe
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings != null)
                {
                    return settings;
                }
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
            string directory = Path.GetDirectoryName(_settingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Sérialiser et sauvegarder les paramètres
            string json = JsonSerializer.Serialize(_currentSettings, new JsonSerializerOptions 
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
}

// Classe pour stocker les paramètres - renommée pour éviter les conflits
public class AppSettings
{
    // Paramètres généraux
    public string JavaPath { get; set; } = "";
    public int MemoryAllocationGB { get; set; } = 4;
    public int ThemeIndex { get; set; } = 0;
    
    // Paramètres Minecraft
    public bool CloseAfterLaunch { get; set; } = false;
    public bool ShowDebugOutput { get; set; } = false;
    public bool DeveloperMode { get; set; } = false;
}
