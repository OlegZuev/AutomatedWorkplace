using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using AutomatedWorkplace.Models;

namespace AutomatedWorkplace.ViewModels {
    public class UsersViewModel : EntityViewModel<User> {
        public UsersViewModel() {
            using (var context = new BookOrdersContext()) {
                Entities = new ObservableCollection<User>(context.Users);
            }

            foreach (User user in Entities) {
                user.InitializeValidator(Entities);
            }
        }

        protected override void BookOrdersContext_NewEntityAdded() {
            using (var context = new BookOrdersContext()) {
                UpdateEntity(Entities, context.Users.ToList(), Entities);
            }
        }

        protected override void DataGridAddingNewItem(AddingNewItemEventArgs e) {
            e.NewItem = new User {RegistrationDate = DateTime.Now};
            ((User) e.NewItem).InitializeValidator();
        }

        protected override void PageLoaded(object sender) {
            throw new NotImplementedException();
        }
    }
}