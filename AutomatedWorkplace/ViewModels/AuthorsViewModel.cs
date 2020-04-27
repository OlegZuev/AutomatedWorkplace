using System;
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
    public class AuthorsViewModel : EntityViewModel<Author> {
        public Author SelectedAuthor { get; set; }

        public void OnSelectedAuthorChanged() {
            SelectedAuthorChanged?.Invoke(SelectedAuthor);
        }

        public AuthorsViewModel() {
            using (var context = new BookOrdersContext()) {
                Entities = new ObservableCollection<Author>(context.Authors);
            }

            foreach (Author author in Entities) {
                author.InitializeValidator(Entities);
            }
        }

        protected override void BookOrdersContext_NewEntityAdded() {
            using (var context = new BookOrdersContext()) {
                UpdateEntity(Entities, context.Authors.ToList(), Entities);
            }
        }

        protected override void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new Author();
            ((Author) e.NewItem).InitializeValidator();
        }

        protected override void PageLoaded(object sender) {
            throw new NotImplementedException();
        }

        public static event Action<Author> SelectedAuthorChanged;
    }
}