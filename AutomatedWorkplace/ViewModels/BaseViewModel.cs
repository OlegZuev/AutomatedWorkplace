using System.ComponentModel;
using System.Runtime.CompilerServices;
using AutomatedWorkplace.Annotations;
using PropertyChanged;

namespace AutomatedWorkplace.ViewModels {
    [AddINotifyPropertyChangedInterface]
    public class BaseViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}