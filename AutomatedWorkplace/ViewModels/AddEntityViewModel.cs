using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AutomatedWorkplace.Models;
using PropertyChanged;

namespace AutomatedWorkplace.ViewModels {
    public class AddEntityViewModel : BaseViewModel {
        public ObservableCollection<Book> Books { get; set; }

        public ObservableCollection<Author> Authors { get; set; }

        public ObservableCollection<Publisher> Publishers { get; set; }

        public BookAndAuthor NewBookAndAuthor { get; set; }

        public Book SelectedBook { get; set; }

        [DependsOn(nameof(SelectedBook))] public Book NewBook { get; set; }

        public void OnNewBookChanged() {
            if (SelectedBook == null) return;
            NewBook.CopyProperties(SelectedBook);
            SelectedPublisher = Publishers.First(publisher => publisher.Id == SelectedBook.PublisherId);
        }

        public Author SelectedAuthor { get; set; }

        [DependsOn(nameof(SelectedAuthor))] public Author NewAuthor { get; set; }

        public void OnNewAuthorChanged() {
            if (SelectedAuthor == null) return;
            NewAuthor.CopyProperties(SelectedAuthor);
            NewAuthor.Id = SelectedAuthor.Id;
        }

        public Publisher SelectedPublisher { get; set; }

        [DependsOn(nameof(SelectedPublisher))] public Publisher NewPublisher { get; set; }

        public int NewPublisherId;

        public void OnNewPublisherChanged() {
            if (SelectedPublisher == null) return;
            NewPublisher.PropertyChanged -= OnNewPublisherOnPropertyChanged;
            NewPublisher.CopyProperties(SelectedPublisher);
            NewPublisher.PropertyChanged += OnNewPublisherOnPropertyChanged;
            NewPublisher.Id = SelectedPublisher.Id;
        }

        public AddEntityViewModel() {
            using (var context = new BookOrdersContext()) {
                Books = new ObservableCollection<Book>(context.Books);
                Authors = new ObservableCollection<Author>(context.Authors);
                Publishers = new ObservableCollection<Publisher>(context.Publishers);
                NewPublisherId = (int) context.Database.SqlQuery<decimal>("SELECT last_value FROM publishers_id_seq")
                                              .First() + 1;
            }

            NewBook = new Book();
            NewAuthor = new Author();
            NewPublisher = new Publisher();
            NewBookAndAuthor = new BookAndAuthor();
            NewBook.InitializeValidator(null, new List<Publisher>(Publishers) {new Publisher {Id = NewPublisherId}});
            NewAuthor.InitializeValidator();
            NewPublisher.InitializeValidator();
            NewBookAndAuthor.InitializeValidator();
            AddNewEntityCommand = new DelegateCommand<object>(AddNewEntity, CanAddNewEntity);
            NewBook.PropertyChanged += delegate {
                if (NewBook.Validator.IsValid) {
                    NewPublisher.Id = ISBNUtils.GetPublisherId(NewBook.ISBN);
                }
            };

            NewPublisher.PropertyChanged += OnNewPublisherOnPropertyChanged;
        }

        [SuppressPropertyChangedWarnings]
        private void OnNewPublisherOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (!Publishers.Any(NewPublisher.Equals)) {
                NewPublisher.Id = NewPublisherId;
            }

            NewBook.PublisherId = NewPublisher.Id;
        }

        public ICommand AddNewEntityCommand { get; }

        private void AddNewEntity(object sender) {
            using (var context = new BookOrdersContext()) {
                using (DbContextTransaction transaction =
                    context.Database.BeginTransaction(IsolationLevel.RepeatableRead)) {
                    try {
                        if (!Publishers.Any(NewPublisher.Equals)) {
                            context.Publishers.Add(NewPublisher);
                        }

                        NewBook.PublisherId = NewPublisher.Id;
                        if (!Books.Any(NewBook.Equals)) {
                            context.Books.Add(NewBook);
                        }

                        if (!Authors.Any(NewAuthor.Equals)) {
                            NewAuthor.Id = 0;
                            context.Authors.Add(NewAuthor);
                        }

                        NewBookAndAuthor.AuthorId = NewAuthor.Id;
                        NewBookAndAuthor.ISBN = NewBook.ISBN;
                        context.BooksAndAuthors.Add(NewBookAndAuthor);
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

                    AddedNewEntity?.Invoke();
                }
            }
        }

        private bool CanAddNewEntity(object sender) {
            return NewBook.Validator.IsValid && NewAuthor.Validator.IsValid &&
                   NewPublisher.Validator.IsValid && NewBookAndAuthor.Validator.IsValid;
        }

        public event Action AddedNewEntity;
    }
}