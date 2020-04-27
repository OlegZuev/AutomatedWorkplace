using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AutomatedWorkplace.Models;

namespace AutomatedWorkplace.ViewModels {
    public class BooksViewModel : EntityViewModel<Book> {
        public ObservableCollection<Publisher> Publishers { get; set; }

        public BooksViewModel() {
            using (var context = new BookOrdersContext()) {
                Entities = new ObservableCollection<Book>(context.Books);
                Publishers = new ObservableCollection<Publisher>(context.Publishers);
            }

            foreach (Book book in Entities) {
                book.InitializeValidator(Entities, Publishers);
            }

            DataGridCellEditEndingCommand =
                new DelegateCommand<DataGridCellEditEndingEventArgs>(DataGridCellEditEnding);
        }

        public ICommand DataGridCellEditEndingCommand { get; }

        protected override void BookOrdersContext_NewEntityAdded() {
            using (var context = new BookOrdersContext()) {
                foreach (DbEntityEntry entity in context.ChangeTracker.Entries()) {
                    entity.Reload();
                }
                UpdateEntity(Entities, context.Books.ToList(), Entities, Publishers);
            }

            PageLoaded(null);
        }

        protected override void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new Book();
            ((Book) e.NewItem).InitializeValidator(Entities, Publishers);
        }

        private void DataGridCellEditEnding(DataGridCellEditEndingEventArgs e) {
            ResizeComboBoxColumnToContent(e, "Publisher", "Name", Publishers);
        }

        protected override void PageLoaded(object sender) {
            UpdateDependentEntities();
        }

        private void UpdateDependentEntities() {
            using (var context = new BookOrdersContext()) {
                UpdateEntity(Publishers, context.Publishers.ToList());
            }
        }
    }
}