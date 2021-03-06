﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AutomatedWorkplace.Models;
using AutomatedWorkplace.Views;

namespace AutomatedWorkplace.ViewModels {
    public class MainWindowViewModel : BaseViewModel {
        public bool IsAdmin { get; set; }

        private User _currentUser;

        private int _dialogResult;

        private readonly AuthenticationWindow _authenticationWindow;

        private RegistrationWindow _registrationWindow;

        private AddEntityWindow _addEntityWindow;

        private readonly MainWindow _mainWindow;

        private Author _selectedAuthor;

        private Publisher _selectedPublisher;

        public MainWindowViewModel(MainWindow window) {
            AddNewEntityCommand = new DelegateCommand<object>(AddNewEntity, CanAddNewEntity);
            ShowAllInfoAboutSelectedAuthorCommand =
                new DelegateCommand<object>(ShowAllInfoAboutSelectedAuthor, CanShowAllInfoAboutSelectedAuthor);
            ShowAllInfoAboutSelectedPublisherCommand =
                new DelegateCommand<object>(ShowAllInfoAboutSelectedPublisher, CanShowAllInfoAboutSelectedPublisher);

            _mainWindow = window;
            var authenticationViewModel = new AuthenticationViewModel();
            authenticationViewModel.SignInConfirmed += AuthenticationViewModel_SignInConfirmed;
            authenticationViewModel.SignInSkipped += AuthenticationViewModel_SignInSkipped;
            authenticationViewModel.SignUpConfirmed += AuthenticationViewModel_SignUpConfirmed;
            _authenticationWindow = new AuthenticationWindow(authenticationViewModel);
            _mainWindow.SourceInitialized += delegate {
                _mainWindow.ShowActivated = false;
                _mainWindow.Hide();
                _authenticationWindow.Closed += AuthenticationWindow_Closed;
                _authenticationWindow.ShowDialog();
            };
        }

        private void AuthenticationWindow_Closed(object sender, EventArgs e) {
            if (_dialogResult == 0) {
                Environment.Exit(0);
            }
        }

        private void AuthenticationCompleted() {
            BookOrdersContext.RaiseUserStatusLoaded(_currentUser);
            IsAdmin = _currentUser?.Role == "ADMIN";
            AuthorsViewModel.SelectedAuthorChanged += AuthorsViewModel_SelectedAuthorChanged;
            PublishersViewModel.SelectedAuthorChanged += PublishersViewModel_SelectedAuthorChanged;
            _mainWindow.Show();
        }

        private void AuthorsViewModel_SelectedAuthorChanged(Author obj) {
            _selectedAuthor = obj;
        }

        private void PublishersViewModel_SelectedAuthorChanged(Publisher obj) {
            _selectedPublisher = obj;
        }

        public ICommand AddNewEntityCommand { get; }

        public ICommand ShowAllInfoAboutSelectedAuthorCommand { get; }

        public ICommand ShowAllInfoAboutSelectedPublisherCommand { get; }

        public static event Action<IList<BookAndAuthor>> ShowAllInfoBooksAndAuthorsRaised;

        private void AddNewEntity(object sender) {
            var addEntityViewModel = new AddEntityViewModel();
            addEntityViewModel.AddedNewEntity += AddEntityViewModel_AddedNewEntity;
            _addEntityWindow = new AddEntityWindow(addEntityViewModel);
            _addEntityWindow.ShowDialog();
        }

        private bool CanAddNewEntity(object sender) {
            return _currentUser != null && new[] {"ADMIN", "USER"}.Contains(_currentUser.Role);
        }

        private void ShowAllInfoAboutSelectedAuthor(object sender) {
            IList<BookAndAuthor> results;
            using (var context = new BookOrdersContext()) {
                results = (from bookAndAuthor in context.BooksAndAuthors
                           where bookAndAuthor.AuthorId == _selectedAuthor.Id
                           select bookAndAuthor)
                          .Include(entity => entity.Author)
                          .Include(entity => entity.Book)
                          .Include(entity => entity.Book.Publisher)
                          .ToList();
            }

            ShowAllInfoBooksAndAuthorsRaised?.Invoke(results);
        }

        private bool CanShowAllInfoAboutSelectedAuthor(object sender) {
            return _selectedAuthor != null;
        }

        private void ShowAllInfoAboutSelectedPublisher(object sender) {
            IList<BookAndAuthor> results;
            using (var context = new BookOrdersContext()) {
                results = (from bookAndAuthor in context.BooksAndAuthors
                           join book in context.Books on bookAndAuthor.ISBN equals book.ISBN
                           where book.PublisherId == _selectedPublisher.Id
                           select bookAndAuthor)
                          .Include(entity => entity.Author)
                          .Include(entity => entity.Book)
                          .Include(entity => entity.Book.Publisher)
                          .ToList();
            }

            ShowAllInfoBooksAndAuthorsRaised?.Invoke(results);
        }

        private bool CanShowAllInfoAboutSelectedPublisher(object sender) {
            return _selectedPublisher != null;
        }

        private void AddEntityViewModel_AddedNewEntity() {
            BookOrdersContext.RaiseNewEntityAdded();
            _addEntityWindow.Close();
        }

        private void AuthenticationViewModel_SignInConfirmed(User user) {
            _currentUser = user;
            _dialogResult = 1;
            _authenticationWindow.Close();
            AuthenticationCompleted();
        }

        private void AuthenticationViewModel_SignInSkipped() {
            _dialogResult = 1;
            _authenticationWindow.Close();
            AuthenticationCompleted();
        }

        private void AuthenticationViewModel_SignUpConfirmed() {
            _authenticationWindow.Hide();
            var registrationViewModel = new RegistrationViewModel();
            registrationViewModel.RegisterConfirmed += RegistrationViewModel_RegisterConfirmed;
            _registrationWindow = new RegistrationWindow(registrationViewModel);
            _registrationWindow.Closed += RegistrationWindow_Closed;
            _registrationWindow.ShowDialog();
        }

        private void RegistrationWindow_Closed(object sender, EventArgs e) {
            if (_dialogResult != 1) {
                _authenticationWindow.ShowDialog();
            }
        }

        private void RegistrationViewModel_RegisterConfirmed(User user) {
            _currentUser = user;
            _dialogResult = 1;
            _authenticationWindow.ShowDialog();
            _registrationWindow.Close();
        }
    }
}