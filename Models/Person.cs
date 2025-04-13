using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using ZodiacSignUserStore.Exceptions;

namespace ZodiacSignUserStore.Models
{
    internal class Person : INotifyPropertyChanged, IDataErrorInfo
    {
        private const int MaximumAge = 135;


        private string _firstName = "";
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (_firstName == value)
                    return; // prevent duplicate call
                var previous = _firstName;
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentException("First name cannot be empty.");

                    _firstName = value;
                    OnPropertyChanged(nameof(FirstName));
                }
                catch (Exception ex)
                {
                    RaiseValidationError(ex.Message);
                    _firstName = previous;
                    OnPropertyChanged(nameof(FirstName));
                }
            }
        }

        private string _lastName = "";
        public string LastName
        {
            get => _lastName;
            set
            {
                if (_lastName == value)
                    return; // prevent duplicate call
                var previous = _lastName;
                try
                {
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentException("Last name cannot be empty.");

                    _lastName = value;
                    OnPropertyChanged(nameof(LastName));
                }
                catch (Exception ex)
                {
                    RaiseValidationError(ex.Message);
                    _lastName = previous;
                    OnPropertyChanged(nameof(LastName));
                }
            }
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set
            {
                if (_email == value)
                    return; // prevent duplicate call
                var previous = _email;
                try
                {
                    if (!string.IsNullOrWhiteSpace(value) && !EmailIsCorrect(value))
                        throw new InvalidEmailException();

                    _email = value;
                    OnPropertyChanged(nameof(Email));
                }
                catch (Exception ex)
                {
                    RaiseValidationError(ex.Message);
                    _email = previous;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }

        private DateOnly? _birthDate;
        public DateOnly? BirthDate
        {
            get => _birthDate;
            set
            {
                if (_birthDate == value)
                    return; // prevent duplicate call

                var previous = _birthDate;
                try
                {
                    if (!value.HasValue)
                        throw new NullReferenceException("Birth Date must be set.");
                    if (value.Value > DateOnly.FromDateTime(DateTime.Today))
                        throw new FutureBirthDateException();
                    if (CalculateAge(value.Value) > MaximumAge)
                        throw new TooOldBirthDateException();

                    _birthDate = value;
                    OnPropertyChanged(nameof(BirthDate));

                    // auto update dependent fields
                    IsAdult = CalculateAge(value.Value) >= 18;
                    SunSign = CalculateWesternZodiac(value.Value);
                    ChineseSign = CalculateChineseZodiac(value.Value);
                    IsBirthday = CheckBirthday(value.Value);
                }
                catch (Exception ex)
                {
                    RaiseValidationError(ex.Message);
                    _birthDate = previous;
                    OnPropertyChanged(nameof(BirthDate));
                }
            }
        }


        private bool? _isAdult;
        public bool? IsAdult
        {
            get => _isAdult;
            private set
            {
                if (_isAdult != value)
                {
                    _isAdult = value;
                    OnPropertyChanged(nameof(IsAdult));
                }
            }
        }

        private string? _sunSign;
        public string? SunSign
        {
            get => _sunSign;
            private set
            {
                if (_sunSign != value)
                {
                    _sunSign = value;
                    OnPropertyChanged(nameof(SunSign));
                }
            }
        }

        private string? _chineseSign;
        public string? ChineseSign
        {
            get => _chineseSign;
            private set
            {
                if (_chineseSign != value)
                {
                    _chineseSign = value;
                    OnPropertyChanged(nameof(ChineseSign));
                }
            }
        }

        private bool? _isBirthday;
        public bool? IsBirthday
        {
            get => _isBirthday;
            private set
            {
                if (_isBirthday != value)
                {
                    _isBirthday = value;
                    OnPropertyChanged(nameof(IsBirthday));
                }
            }
        }

        public string this[string columnName] => string.Empty;
        public string Error => string.Empty;


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        public Person()
        {
        }

        public Person(string firstName, string lastName, string? email, DateOnly? birthDate)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            BirthDate = birthDate;

            if (!birthDate.HasValue)
                throw new NullReferenceException("Birth Date was not selected");

            if (birthDate.Value > DateOnly.FromDateTime(DateTime.Today))
                throw new FutureBirthDateException();

            int age = CalculateAge(birthDate.Value);
            if (age > MaximumAge)
                throw new TooOldBirthDateException();

            if (!string.IsNullOrWhiteSpace(email) && !EmailIsCorrect(email))
                throw new InvalidEmailException();

            IsAdult = age >= 18;
            SunSign = CalculateWesternZodiac(birthDate.Value);
            ChineseSign = CalculateChineseZodiac(birthDate.Value);
            IsBirthday = CheckBirthday(birthDate.Value);
        }

        [JsonConstructor]
        public Person(string firstName, string lastName, string? email, DateOnly? birthDate,
                      bool? isAdult, string? sunSign, string? chineseSign, bool? isBirthday)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            BirthDate = birthDate;

            IsAdult = isAdult;
            SunSign = sunSign;
            ChineseSign = chineseSign;
            IsBirthday = isBirthday;
        }

        public Person(string firstName, string lastName, string email)
            : this(firstName, lastName, email, null) { }

        public Person(string firstName, string lastName, DateOnly birthDate)
            : this(firstName, lastName, null, birthDate) { }

        public event EventHandler<string>? ValidationFailed;

        private void RaiseValidationError(string message)
        {
            ValidationFailed?.Invoke(this, message);
        }

        private static bool EmailIsCorrect(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private static int CalculateAge(DateOnly birthdate)
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - birthdate.Year;
            if (today < birthdate.AddYears(age)) age--;
            return age;
        }

        private static bool CheckBirthday(DateOnly birthdate)
        {
            return DateTime.Today.ToString("MMdd") == birthdate.ToString("MMdd");
        }

        private static string CalculateWesternZodiac(DateOnly birthdate)
        {
            int day = birthdate.Day;
            int month = birthdate.Month;

            return (month == 3 && day >= 21 || month == 4 && day <= 19) ? "Aries" :
                   (month == 4 && day >= 20 || month == 5 && day <= 20) ? "Taurus" :
                   (month == 5 && day >= 21 || month == 6 && day <= 20) ? "Gemini" :
                   (month == 6 && day >= 21 || month == 7 && day <= 22) ? "Cancer" :
                   (month == 7 && day >= 23 || month == 8 && day <= 22) ? "Leo" :
                   (month == 8 && day >= 23 || month == 9 && day <= 22) ? "Virgo" :
                   (month == 9 && day >= 23 || month == 10 && day <= 22) ? "Libra" :
                   (month == 10 && day >= 23 || month == 11 && day <= 21) ? "Scorpio" :
                   (month == 11 && day >= 22 || month == 12 && day <= 21) ? "Sagittarius" :
                   (month == 12 && day >= 22 || month == 1 && day <= 19) ? "Capricorn" :
                   (month == 1 && day >= 20 || month == 2 && day <= 18) ? "Aquarius" :
                   (month == 2 && day >= 19 || month == 3 && day <= 20) ? "Pisces" :
                   "Unknown";
        }

        private static string CalculateChineseZodiac(DateOnly birthdate)
        {

            int year = birthdate.Year;
            string[] zodiacSigns =
            [
                "Monkey",   // 0
                "Rooster",  // 1
                "Dog",      // 2
                "Pig",      // 3
                "Rat",      // 4
                "Ox",       // 5
                "Tiger",    // 6
                "Rabbit",   // 7
                "Dragon",   // 8
                "Snake",    // 9
                "Horse",    // 10
                "Goat"      // 11
            ];

            int zodiacIndex = year % 12;
            return zodiacSigns[zodiacIndex];
        }

    }
}
