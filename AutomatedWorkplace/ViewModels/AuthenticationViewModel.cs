using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutomatedWorkplace.Models;
using _BCrypt = BCrypt.Net.BCrypt;

namespace AutomatedWorkplace.ViewModels {
    public class AuthenticationViewModel : BaseViewModel {
        public User NewUser { get; set; }

        public AuthenticationViewModel() {
            using (var context = new BookOrdersContext()) {
                NewUser = new User {Password = "Password", Role = "USER"};
            }

            SignInCommand = new DelegateCommand<PasswordBox>(SignIn);
            SignUpCommand = new DelegateCommand<object>(SignUp);
            SkipSignInCommand = new DelegateCommand<object>(SkipSignIn);
        }

        public ICommand SignInCommand { get; }

        public ICommand SignUpCommand { get; }

        public ICommand SkipSignInCommand { get; }

        private void SignIn(PasswordBox passwordBox) {
            using (var context = new BookOrdersContext()) {
                User possibleUser = context.Users.FirstOrDefault(user => user.Login == NewUser.Login);
                if (possibleUser != null && _BCrypt.Verify(passwordBox.Password.Trim(), possibleUser.Password)) {
                    SignInConfirmed?.Invoke(possibleUser);
                } else {
                    MessageBox.Show("Invalid Login or Password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SignUp(object sender) {
            SignUpConfirmed?.Invoke();
        }

        private void SkipSignIn(object sender) {
            SignInSkipped?.Invoke();
        }

        public event Action<User> SignInConfirmed;

        public event Action SignUpConfirmed;

        public event Action SignInSkipped;
    }
}