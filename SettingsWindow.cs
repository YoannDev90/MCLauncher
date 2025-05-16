using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace MCLauncher;

public partial class SettingsWindow : Window
{
    private TextBlock? memoryValueText;
    private TextBlock? availableMemoryText;
    private Slider? memorySlider;
    private CheckBox? devModeCheckBox;
    private StackPanel? devModeOptionsPanel;
    private TextBox? javaPathTextBox;
    private ComboBox? themeComboBox;
    
    // Notifications temporaires
    private Border? notificationBorder;

    // Chemin du fichier de configuration
    private readonly string _settingsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "MCLauncher", 
        "settings.json");
    
    // Configuration actuelle
    private Settings _currentSettings;

    public SettingsWindow()
    {
        try
        {
            AvaloniaXamlLoader.Load(this);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de l'initialisation de la fenêtre des paramètres: {ex}");
        }
        Title = "Paramètres";
        // Ici, ajoute les contrôles pour les options du launcher

        // Initialiser les contrôles après le chargement
        Loaded += SettingsWindow_Loaded;

        // Créer la notification
        SetupNotification();

        // Charger les paramètres
        _currentSettings = LoadSettings();
    }

    private void SetupNotification()
    {
        // Création de la notification
        notificationBorder = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#333333")),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 12),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 20),
            Opacity = 0,
            IsVisible = false,
            BoxShadow = new BoxShadows(new BoxShadow
            {
                OffsetX = 0,
                OffsetY = 2,
                Blur = 10,
                Spread = 0,
                Color = Color.Parse("#40000000")
            }),
        };

        // Transitions pour animation
        notificationBorder.Transitions = new Avalonia.Animation.Transitions
        {
            new Avalonia.Animation.DoubleTransition
            {
                Property = Border.OpacityProperty,
                Duration = TimeSpan.FromSeconds(0.3)
            }
        };

        var notificationText = new TextBlock
        {
            Name = "NotificationText",
            FontWeight = FontWeight.Medium
        };
        
        notificationBorder.Child = notificationText;
        
        // Ajout de la notification au Visual Tree
        this.AttachedToVisualTree += (s, e) =>
        {
            var overlayPanel = new Panel();
            overlayPanel.Children.Add(notificationBorder);
            
            // On ajoute le panel d'overlay après le content principal
            if (this.Content is Grid mainGrid)
            {
                var parentPanel = new Panel();
                this.Content = parentPanel;
                
                parentPanel.Children.Add(mainGrid);
                parentPanel.Children.Add(overlayPanel);
            }
        };
    }

    private void SettingsWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        memoryValueText = this.FindControl<TextBlock>("MemoryValueText");
        availableMemoryText = this.FindControl<TextBlock>("AvailableMemoryText");
        memorySlider = this.FindControl<Slider>("MemorySlider");
        devModeCheckBox = this.FindControl<CheckBox>("DevModeCheckBox");
        devModeOptionsPanel = this.FindControl<StackPanel>("DevModeOptionsPanel");
        javaPathTextBox = this.FindControl<TextBox>("JavaPathTextBox");
        themeComboBox = this.FindControl<ComboBox>("ThemeComboBox");
        
        // Initialiser les valeurs
        if (memorySlider != null)
        {
            memorySlider.Value = _currentSettings.MemoryAllocationGB;
            UpdateMemoryText((double)memorySlider.Value);
        }
        
        // Mettre à jour l'information sur la mémoire disponible
        UpdateAvailableMemoryInfo();
        
        // Charger le chemin Java
        if (javaPathTextBox != null)
        {
            javaPathTextBox.Text = _currentSettings.JavaPath;
        }
        
        // Définir l'état du mode développeur
        if (devModeCheckBox != null && devModeOptionsPanel != null)
        {
            bool devModeEnabled = _currentSettings.DeveloperMode;
            devModeCheckBox.IsChecked = devModeEnabled;
            devModeOptionsPanel.IsVisible = devModeEnabled;
        }
        
        // Charger le thème
        if (themeComboBox != null)
        {
            themeComboBox.SelectedIndex = _currentSettings.ThemeIndex;
        }

        // Configurer le bouton Lucky pour ouvrir le lien
        var luckyButton = this.FindControl<Button>("LuckyButton");
        if (luckyButton != null)
        {
            luckyButton.Click += OnLuckyLinkClicked;
        }
        
        // Configurer le changement de la valeur du slider mémoire
        if (memorySlider != null && memoryValueText != null)
        {
            memorySlider.ValueChanged += (s, args) => 
            {
                int memoryValue = (int)args.NewValue;
                memoryValueText.Text = $"{memoryValue} Go alloués";
                _currentSettings.MemoryAllocationGB = memoryValue;
                SaveSettings();
            };
        }
        
        // Configurer la visibilité du panel de mode développeur
        if (devModeCheckBox != null && devModeOptionsPanel != null)
        {
            devModeCheckBox.IsCheckedChanged += (s, args) =>
            {
                devModeOptionsPanel.IsVisible = devModeCheckBox.IsChecked ?? false;
                _currentSettings.DeveloperMode = devModeCheckBox.IsChecked ?? false;
                SaveSettings();
            };
        }

        // Configurer les autres CheckBox
        var closeAfterLaunchCheckBox = this.FindControl<CheckBox>("CloseAfterLaunchCheckBox");
        if (closeAfterLaunchCheckBox != null)
        {
            // Appliquer la valeur sauvegardée
            closeAfterLaunchCheckBox.IsChecked = _currentSettings.CloseAfterLaunch;
            
            // Configurer l'événement
            closeAfterLaunchCheckBox.IsCheckedChanged += (s, args) =>
            {
                _currentSettings.CloseAfterLaunch = closeAfterLaunchCheckBox.IsChecked ?? false;
                SaveSettings();
            };
        }
        
        var showDebugOutputCheckBox = this.FindControl<CheckBox>("ShowDebugOutputCheckBox");
        if (showDebugOutputCheckBox != null)
        {
            // Appliquer la valeur sauvegardée
            showDebugOutputCheckBox.IsChecked = _currentSettings.ShowDebugOutput;
            
            // Configurer l'événement
            showDebugOutputCheckBox.IsCheckedChanged += (s, args) =>
            {
                _currentSettings.ShowDebugOutput = showDebugOutputCheckBox.IsChecked ?? false;
                SaveSettings();
            };
        }
        
        // Configurer la ComboBox du thème
        if (themeComboBox != null)
        {
            // Appliquer la valeur sauvegardée
            themeComboBox.SelectedIndex = _currentSettings.ThemeIndex;
            
            // Configurer l'événement
            themeComboBox.SelectionChanged += (s, args) =>
            {
                _currentSettings.ThemeIndex = themeComboBox.SelectedIndex;
                SaveSettings();
            };
        }

        // Configurer la détection automatique de Java
        var autoDetectButton = this.FindControl<Button>("AutoDetectButton");
        if (autoDetectButton != null)
        {
            autoDetectButton.Click += AutoDetectJava;
        }
        
        // Configurer le bouton parcourir pour Java
        var browseButton = this.FindControl<Button>("BrowseButton");
        if (browseButton != null)
        {
            browseButton.Click += SelectJavaPath;
        }
    }
    
    // Gestionnaire pour le slider de mémoire
    private void OnMemorySliderChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        if (sender is Slider slider)
        {
            UpdateMemoryText(slider.Value);
            SaveMemoryValue(slider.Value);
        }
    }
    
    private void UpdateMemoryText(double value)
    {
        if (memoryValueText != null)
        {
            int memoryValue = (int)Math.Round(value);
            memoryValueText.Text = $"{memoryValue} Go";
        }
    }
    
    private void UpdateAvailableMemoryInfo()
    {
        if (availableMemoryText != null)
        {
            try
            {
                long totalMemoryMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
                long totalMemoryGB = totalMemoryMB / 1024;
                availableMemoryText.Text = $"Mémoire système disponible : {totalMemoryGB} Go";
            }
            catch
            {
                availableMemoryText.Text = "Mémoire système disponible : Information non disponible";
            }
        }
    }

    // Gestion du mode développeur
    private void OnDevModeChanged(object? sender, RoutedEventArgs e)
    {
        if (devModeCheckBox != null && devModeOptionsPanel != null)
        {
            bool isChecked = devModeCheckBox.IsChecked ?? false;
            devModeOptionsPanel.IsVisible = isChecked;
            
            SaveDevModeEnabled(isChecked);

            if (isChecked)
            {
                ShowNotification("Mode développeur activé");
            }
            else
            {
                ShowNotification("Mode développeur désactivé");
            }
        }
    }
    
    // Sélection du chemin Java
    private async void SelectJavaPath(object? sender, RoutedEventArgs e)
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
        else
        {
            // Aucun filtre spécifique pour Linux/macOS
        }

        var result = await dialog.ShowAsync(this);
        
        if (result != null && result.Length > 0)
        {
            string path = result[0];
            if (javaPathTextBox != null)
            {
                javaPathTextBox.Text = path;
                _currentSettings.JavaPath = path;
                SaveSettings();
                ShowNotification("Chemin Java mis à jour");
            }
        }
    }
    
    // Détection automatique de Java
    private void AutoDetectJava(object? sender, RoutedEventArgs e)
    {
        string? javaPath = FindJavaPath();
        
        if (javaPath != null && javaPathTextBox != null)
        {
            javaPathTextBox.Text = javaPath;
            _currentSettings.JavaPath = javaPath;
            SaveSettings();
            ShowNotification("Java détecté automatiquement");
        }
        else
        {
            ShowNotification("Impossible de détecter Java automatiquement", isError: true);
        }
    }
    
    private string? FindJavaPath()
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
        
        // Vérifier dans les chemins communs (simplifié)
        string[] commonPaths = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { @"C:\Program Files\Java", @"C:\Program Files (x86)\Java" }
            : new[] { "/usr/bin/java", "/usr/local/bin/java" };
            
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
        
        return null;
    }
    
    // Gestion du thème
    private void OnThemeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (themeComboBox != null)
        {
            SaveThemeIndex(themeComboBox.SelectedIndex);
            ApplyTheme(themeComboBox.SelectedIndex);
            
            // Notification pour le changement de thème
            ShowNotification("Thème appliqué");
        }
    }
    
    private void ApplyTheme(int themeIndex)
    {
        // Appliquer le thème sélectionné
        // Dans une vraie application, ce code serait plus élaboré
        // et affecterait l'application dans son ensemble
        
        string themeName = themeIndex switch
        {
            0 => "Sombre",
            1 => "Clair",
            2 => "Système",
            _ => "Sombre"
        };
        
        Debug.WriteLine($"Thème appliqué : {themeName}");
    }
    
    // Liens et boutons dans l'onglet À propos
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
            // Gérer silencieusement l'erreur
            Debug.WriteLine($"Impossible d'ouvrir l'URL: {ex.Message}");
        }
    }
    
    private void OpenGitHub(object? sender, RoutedEventArgs e)
    {
        var url = "https://github.com/YoannDumont/MCLauncher";
        try
        {
            var psi = new ProcessStartInfo(url) { UseShellExecute = true };
            Process.Start(psi);
        }
        catch
        {
            ShowNotification("Impossible d'ouvrir le lien GitHub", isError: true);
        }
    }
    
    private async void CheckForUpdates(object? sender, RoutedEventArgs e)
    {
        ShowNotification("Recherche de mises à jour...");
        
        // Simuler une vérification des mises à jour
        await Task.Delay(2000);
        
        // Dans un cas réel, on vérifierait vraiment les mises à jour
        // Ici on simule juste une réponse
        ShowNotification("Votre application est à jour");
    }
    
    // Système de notification
    private void ShowNotification(string message, bool isError = false)
    {
        if (notificationBorder != null && notificationBorder.Child is TextBlock textBlock)
        {
            // Configurer le message
            textBlock.Text = message;
            
            // Configurer la couleur en fonction du type de message
            notificationBorder.Background = new SolidColorBrush(
                isError ? Color.Parse("#E53935") : Color.Parse("#333333"));
                
            // Afficher avec animation
            notificationBorder.IsVisible = true;
            notificationBorder.Opacity = 0;
            notificationBorder.Opacity = 1;
            
            // Planifier la disparition
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            
            timer.Tick += (s, e) =>
            {
                notificationBorder.Opacity = 0;
                timer.Stop();
                
                // Timer pour cacher complètement après la fin de l'animation
                var hideTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(0.3)
                };
                
                hideTimer.Tick += (s2, e2) =>
                {
                    notificationBorder.IsVisible = false;
                    hideTimer.Stop();
                };
                
                hideTimer.Start();
            };
            
            timer.Start();
        }
    }
    
    // Méthodes de persistance (simulées ici)
    private double GetSavedMemoryValue() => 4.0; // Simulé
    private void SaveMemoryValue(double value) => Debug.WriteLine($"Sauvegarde de la mémoire : {value}");
    
    private string GetSavedJavaPath() => ""; // Simulé
    private void SaveJavaPath(string path) => Debug.WriteLine($"Sauvegarde du chemin Java : {path}");
    
    private bool GetDevModeEnabled() => false; // Simulé
    private void SaveDevModeEnabled(bool enabled) => Debug.WriteLine($"Sauvegarde du mode développeur : {enabled}");
    
    private int GetSavedThemeIndex() => 0; // Simulé
    private void SaveThemeIndex(int index) => Debug.WriteLine($"Sauvegarde de l'index du thème : {index}");

    // Chargement des paramètres depuis le fichier
    private Settings LoadSettings()
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
                return JsonSerializer.Deserialize<Settings>(json) ?? new Settings();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors du chargement des paramètres: {ex.Message}");
        }
        
        // Retourner les paramètres par défaut si le fichier n'existe pas ou en cas d'erreur
        return new Settings();
    }
    
    private async void ShowErrorDialog(string title, string message)
    {
        await MessageBoxManager.ShowErrorAsync(this, title, message);
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

// Classe pour stocker les paramètres
public class Settings
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