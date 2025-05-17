using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using Avalonia.Media;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;

namespace MCLauncher;

public class MinecraftInstance
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public string Loader { get; set; } = "Vanilla";
    
    public string VersionInfo => $"{Version} • {Loader}";
    
    // Autres propriétés importantes pour le lancement
    public string Path { get; set; } = "";
    public string JavaPath { get; set; } = "";
    public string JavaArgs { get; set; } = "";
    public int Memory { get; set; } = 2048;
}

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Func<object, bool> _canExecute;

    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

    public void Execute(object parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

public partial class MainWindow : Window
{
    private bool editModeEnabled = false;
    private readonly string instancesFilePath;
    
    // Collection des instances Minecraft
    public ObservableCollection<MinecraftInstance> Instances { get; } = new ObservableCollection<MinecraftInstance>();
    
    // Commandes pour les boutons
    public ICommand LaunchCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public MainWindow()
    {
        InitializeComponent();
        
        // Définir les commandes
        LaunchCommand = new RelayCommand(Launch);
        EditCommand = new RelayCommand(Edit);
        DeleteCommand = new RelayCommand(Delete);
        
        // Chemin du fichier des instances
        instancesFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MCLauncher", 
            "instances.json");
        
        // S'assurer que le dossier existe
        Directory.CreateDirectory(Path.GetDirectoryName(instancesFilePath));
        
        // Charger les instances
        LoadInstances();
        
        // Définir le DataContext pour la liaison de données
        DataContext = this;
    }

    private void LoadInstances()
    {
        try
        {
            if (File.Exists(instancesFilePath))
            {
                var json = File.ReadAllText(instancesFilePath);
                var instances = JsonSerializer.Deserialize<MinecraftInstance[]>(json);
                
                if (instances != null)
                {
                    Instances.Clear();
                    foreach (var instance in instances)
                    {
                        Instances.Add(instance);
                    }
                    
                    StatusText.Text = $"Instances chargées : {Instances.Count}";
                }
            }
            else
            {
                StatusText.Text = "Aucune instance trouvée. Cliquez sur 'New instance' pour en créer une.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors du chargement des instances : {ex}");
            StatusText.Text = "Erreur lors du chargement des instances";
        }
    }
    
    private void SaveInstances()
    {
        try
        {
            var json = JsonSerializer.Serialize(Instances, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(instancesFilePath, json);
            StatusText.Text = "Instances sauvegardées";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur lors de la sauvegarde des instances : {ex}");
            StatusText.Text = "Erreur lors de la sauvegarde des instances";
        }
    }

    private void OpenGitHub(object? sender, RoutedEventArgs e)
    {
        var url = "https://github.com/YoannDev90/MCLauncher";
        var psi = new ProcessStartInfo(url) { UseShellExecute = true };
        Process.Start(psi);
        StatusText.Text = "Ouverture de GitHub...";
    }

    private void FoldersButton_Click(object? sender, RoutedEventArgs routedEventArgs)
    {
        StatusText.Text = "Ouverture du dossier Minecraft...";
    }

    private async void NewInstanceButton_Click(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Création d'une nouvelle instance...";
        var newInstanceWindow = new NewInstanceWindow();
        
        // Abonnement à l'événement pour récupérer la nouvelle instance
        newInstanceWindow.InstanceCreated += (s, instance) =>
        {
            if (instance != null)
            {
                Instances.Add(instance);
                SaveInstances();
                StatusText.Text = $"Instance '{instance.Name}' créée avec succès";
            }
        };
        
        await newInstanceWindow.ShowDialog(this);
    }

    private void AccountsButton_Click(object? sender, RoutedEventArgs e)
    {
        StatusText.Text = "Gestion des comptes...";
        var accountsWindow = new AccountsWindow();
        accountsWindow.Show();
    }

    private void SettingsButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "Ouverture des paramètres...";
            
            // Créer une nouvelle instance de la fenêtre des paramètres
            var settingsWindow = new SettingsWindow();
            
            // Afficher la fenêtre
            settingsWindow.Show();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erreur d'ouverture des paramètres: {ex}");
            StatusText.Text = "Erreur: impossible d'ouvrir les paramètres";
        }
    }

    private void Launch(object parameter)
    {
        if (parameter is MinecraftInstance instance)
        {
            StatusText.Text = $"Lancement de l'instance : {instance.Name}";
            // Logique de lancement à ajouter
        }
    }

    private void Edit(object parameter)
    {
        if (parameter is MinecraftInstance instance)
        {
            StatusText.Text = $"Modification de l'instance : {instance.Name}";
            // Logique d'édition à ajouter
        }
    }

    private void Delete(object parameter)
    {
        if (parameter is MinecraftInstance instance)
        {
            Instances.Remove(instance);
            SaveInstances();
            StatusText.Text = $"Instance supprimée : {instance.Name}";
        }
    }
    
    private void ToggleEditMode(object? sender, RoutedEventArgs e)
    {
        editModeEnabled = !editModeEnabled;
        EditModeEnabled.IsChecked = editModeEnabled;
        
        // Mettre à jour l'apparence du bouton de mode édition
        if (editModeEnabled)
        {
            EditModeButton.Background = new SolidColorBrush(Color.Parse("#CC5068B0"));
            StatusText.Text = "Mode édition activé";
        }
        else
        {
            EditModeButton.Background = new SolidColorBrush(Color.Parse("#7F17181C"));
            StatusText.Text = "Mode édition désactivé";
        }
    }
}
