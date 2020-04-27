using System.Collections.Generic;
using System.ComponentModel;
using PropertyChanged;
using ReactiveValidation;

namespace AutomatedWorkplace.Models {
    [AddINotifyPropertyChangedInterface]
    public abstract class Entity : ValidatableObject, IEditableObject {
        public bool IsDirty => !this.Equals(BackupCopy);

        protected Entity BackupCopy { get; set; }

        protected bool InEdit;

        public abstract void BeginEdit();

        public void CancelEdit() {
            if (!InEdit) return;
            InEdit = false;
            CopyProperties(BackupCopy);
        }

        public void EndEdit() {
            if (!InEdit) return;
            InEdit = false;
            BackupCopy = null;
        }

        public abstract void InitializeValidator(params object[] entities);

        public abstract void CopyProperties<T>(T source) where T : Entity;

        public abstract bool EqualsPrimaryKey<T>(T source) where T : Entity;
    }
}