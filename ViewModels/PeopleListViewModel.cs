using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using ZodiacSignUserStore.Models;

namespace ZodiacSignUserStore.ViewModels;

internal partial class PeopleListViewModel : ObservableObject
{
    private const string FilePath = "C:\\Users\\tetre\\Documents\\mydocs\\study\\sem8\\sharp\\ZodiacSignUserStore\\people.json";

    [ObservableProperty]
    private ObservableCollection<Person> people;

    [ObservableProperty]
    private string filterText;

    [ObservableProperty]
    private Person? selectedPerson;

    public IRelayCommand AddCommand { get; }
    public IRelayCommand EditCommand { get; }
    public IRelayCommand DeleteCommand { get; }
    public IRelayCommand<string> SortCommand { get; }


    public PeopleListViewModel()
    {
        People = LoadOrGeneratePeople();

        foreach (var person in People)
        {
            person.ValidationFailed += OnValidationFailed;
            person.PropertyChanged += OnPersonChanged;
        }

        AddCommand = new RelayCommand(AddPerson);
        EditCommand = new RelayCommand(EditPerson, () => SelectedPerson != null);
        DeleteCommand = new RelayCommand(DeletePerson, () => SelectedPerson != null);
        SortCommand = new RelayCommand<string>(SortBy);

        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SelectedPerson))
            {
                EditCommand.NotifyCanExecuteChanged();
                DeleteCommand.NotifyCanExecuteChanged();
            }
            if (e.PropertyName == nameof(FilterText))
            {
                ApplyFilter();
            }
        };
    }

    private ObservableCollection<Person> LoadOrGeneratePeople()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<ObservableCollection<Person>>(json)!;
        }
        else
        {
            var people = GeneratePeople();
            SavePeople(people);
            return people;
        }
    }

    private void SavePeople(ObservableCollection<Person> people)
    {
        var json = JsonSerializer.Serialize(people);
        File.WriteAllText(FilePath, json);
    }

    private ObservableCollection<Person> GeneratePeople()
    {
        var rng = new Random();
        var list = new ObservableCollection<Person>();
        for (int i = 1; i <= 50; i++)
        {
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(-rng.Next(365 * 10, 365 * 60)));
            var person = new Person($"Name{i}", $"Last{i}", $"user{i}@mail.com", date);
            list.Add(person);
        }
        return list;
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            People = LoadOrGeneratePeople(); // Reset
        }
        else
        {
            var lower = FilterText.ToLower();
            People = new ObservableCollection<Person>(People.Where(p =>
                p.FirstName.ToLower().Contains(lower) ||
                p.LastName.ToLower().Contains(lower) ||
                (p.Email?.ToLower().Contains(lower) ?? false) ||
                (p.SunSign?.ToLower().Contains(lower) ?? false) ||
                (p.ChineseSign?.ToLower().Contains(lower) ?? false)
            ));
        }
    }

    private void SortBy(string property)
    {
        var sorted = People
            .OrderBy(p => p.GetType().GetProperty(property)?.GetValue(p))
            .ToList();
        People = new ObservableCollection<Person>(sorted);
    }

    private void AddPerson()
    {
        var newPerson = new Person("New", "User", "new@mail.com", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)));
        People.Add(newPerson);
        SavePeople(People);
    }

    private void DeletePerson()
    {
        if (SelectedPerson != null)
        {
            People.Remove(SelectedPerson);
            SavePeople(People);
        }
    }


    private void OnValidationFailed(object? sender, string message)
    {
        MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void OnPersonChanged(object? sender, PropertyChangedEventArgs e)
    {
        SavePeople(People); // auto-save whenever something changes
    }
}
