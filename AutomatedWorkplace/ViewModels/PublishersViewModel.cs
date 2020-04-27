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
    public class PublishersViewModel : EntityViewModel<Publisher> {
        public Publisher SelectedPublisher { get; set; }

        public void OnSelectedPublisherChanged() {
            SelectedAuthorChanged?.Invoke(SelectedPublisher);
        }

        public PublishersViewModel() {
            using (var context = new BookOrdersContext()) {
                Entities = new ObservableCollection<Publisher>(context.Publishers);
            }

            foreach (Publisher publisher in Entities) {
                publisher.InitializeValidator(Entities);
            }
        }

        protected override void BookOrdersContext_NewEntityAdded() {
            using (var context = new BookOrdersContext()) {
                UpdateEntity(Entities, context.Publishers.ToList(), Entities);
            }
        }

        protected override void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new Publisher();
            ((Publisher) e.NewItem).InitializeValidator();
        }

        protected override void PageLoaded(object sender) {
            throw new NotImplementedException();
        }

        public static event Action<Publisher> SelectedAuthorChanged;
    }
}