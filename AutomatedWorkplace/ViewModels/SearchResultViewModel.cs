using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using AutomatedWorkplace.Models;

namespace AutomatedWorkplace.ViewModels {
    public class SearchResultViewModel : EntityViewModel<BookAndAuthor> {
        public SearchResultViewModel() {
            MainWindowViewModel.ShowAllInfoBooksAndAuthorsRaised += MainWindowViewModel_ShowAllInfoBooksAndAuthorsRaised;
        }

        private void MainWindowViewModel_ShowAllInfoBooksAndAuthorsRaised(IList<BookAndAuthor> obj) {
            Entities = new ObservableCollection<BookAndAuthor>(obj);
        }

        protected override void BookOrdersContext_NewEntityAdded() {
            // Nothing
        }

        protected override void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            // Nothing
        }

        protected override void PageLoaded(object sender) {
            // Nothing
        }
    }
}