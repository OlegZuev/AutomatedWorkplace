using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutomatedWorkplace.Models;
using _BCrypt = BCrypt.Net.BCrypt;

namespace AutomatedWorkplace.ViewModels {
    public class RegistrationViewModel : BaseViewModel {
        public User NewUser { get; set; }

        private readonly Dictionary<int, string> _imgStrengthPaths = new Dictionary<int, string> {
            {0, "../Images/PasswordStrength0.png"},
            {1, "../Images/PasswordStrength1.png"},
            {2, "../Images/PasswordStrength2.png"},
            {3, "../Images/PasswordStrength3.png"},
            {4, "../Images/PasswordStrength4.png"}
        };


        public string ImgPasswordStrengthPath => _imgStrengthPaths[PasswordStrength];

        public int PasswordStrength { get; set; }

        public string PasswordError { get; set; }

        public Visibility ImgPasswordVisibility { get; set; }

        public RegistrationViewModel() {
            using (var context = new BookOrdersContext()) {
                NewUser = new User {Password = "Password", Role = "USER", RegistrationDate = DateTime.Now};
                NewUser.InitializeValidator(context.Users.ToList());
            }

            PasswordChangedCommand = new DelegateCommand<PasswordBox>(PasswordChanged);
            PasswordLostFocusCommand = new DelegateCommand<object>(PasswordLostFocus);
            RegisterUserCommand = new DelegateCommand<PasswordBox>(RegisterUser, CanRegisterUser);
        }

        public ICommand PasswordChangedCommand { get; }

        public ICommand PasswordLostFocusCommand { get; }

        public ICommand RegisterUserCommand { get; }

        private void PasswordChanged(PasswordBox passwordBox) {
            CheckPassword(passwordBox.Password);
            ImgPasswordVisibility = Visibility.Visible;
        }

        private void PasswordLostFocus(object sender) {
            ImgPasswordVisibility = Visibility.Hidden;
        }

        private void CheckPassword(string password) {
            int result = Zxcvbn.Zxcvbn.MatchPassword(password).Score;
            PasswordError = result < 2 ? "Слабый пароль" : "";
            result = result < 1 ? 1 : result;
            if (string.IsNullOrEmpty(password)) {
                result = 0;
            }

            PasswordStrength = result;
        }

        private void RegisterUser(PasswordBox sender) {
            using (var context = new BookOrdersContext()) {
                using (DbContextTransaction transaction =
                    context.Database.BeginTransaction(IsolationLevel.RepeatableRead)) {
                    try {
                        NewUser.Login = Regex.Replace(NewUser.Login.Trim(), @"\s+", " ");
                        NewUser.Password = _BCrypt.HashPassword(sender.Password.Trim(), 12);
                        context.Users.Add(NewUser);
                        context.SaveChanges();
                        transaction.Commit();
                    } catch (Exception e) {
                        Exception innerException = e;
                        while (innerException?.InnerException != null) {
                            innerException = innerException.InnerException;
                        }

                        transaction.Rollback();
                        MessageBox.Show(innerException?.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    RegisterConfirmed?.Invoke(NewUser);
                }
            }
        }

        private bool CanRegisterUser(PasswordBox sender) {
            return NewUser.Validator.IsValid && PasswordStrength > 1;
        }

        public event Action<User> RegisterConfirmed;
    }
}