using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using AutomatedWorkplace.Models;

namespace AutomatedWorkplace.ViewModels {
    public class BooksAndAuthorsViewModel : EntityViewModel<BookAndAuthor> {
        public ObservableCollection<Book> Books { get; set; }

        public ObservableCollection<Author> Authors { get; set; }

        public BooksAndAuthorsViewModel() {
            using (var context = new BookOrdersContext()) {
                Entities = new ObservableCollection<BookAndAuthor>(context.BooksAndAuthors);
                Books = new ObservableCollection<Book>(context.Books);
                Authors = new ObservableCollection<Author>(context.Authors);
            }

            foreach (BookAndAuthor booksAndAuthors in Entities) {
                booksAndAuthors.AuthorBookNumberText = booksAndAuthors.AuthorBookNumber.ToString();
                booksAndAuthors.InitializeValidator(Books, Authors, Entities);
            }

            DataGridCellEditEndingCommand =
                new DelegateCommand<DataGridCellEditEndingEventArgs>(DataGridCellEditEnding);
        }

        public ICommand DataGridCellEditEndingCommand { get; }

        protected override void BookOrdersContext_NewEntityAdded() {
            PageLoaded(null);
            using (var context = new BookOrdersContext()) {
                UpdateEntity(Entities, context.BooksAndAuthors.ToList(), Books, Authors, Entities);
                foreach (BookAndAuthor bookAndAuthor in Entities) {
                    bookAndAuthor.AuthorBookNumberText = bookAndAuthor.AuthorBookNumber.ToString();
                }
            }
        }

        protected override void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new BookAndAuthor();
            ((BookAndAuthor) e.NewItem).InitializeValidator(Books, Authors, Entities);
        }

        private void DataGridCellEditEnding(DataGridCellEditEndingEventArgs e) {
            ResizeComboBoxColumnToContent(e, "Book", "Name", Books);
            ResizeComboBoxColumnToContent(e, "Author", "FIO", Authors);
        }

        protected override void PageLoaded(object sender) {
            UpdateDependentEntities();
        }

        private void UpdateDependentEntities() {
            using (var context = new BookOrdersContext()) {
                foreach (DbEntityEntry entity in context.ChangeTracker.Entries()) {
                    entity.Reload();
                }
                UpdateEntity(Books, context.Books.ToList());
                UpdateEntity(Authors, context.Authors.ToList());
            }
        }
    }
}