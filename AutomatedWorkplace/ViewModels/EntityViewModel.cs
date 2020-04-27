using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutomatedWorkplace.Models;

namespace AutomatedWorkplace.ViewModels {
    public abstract class EntityViewModel<TEntity> : BaseViewModel where TEntity : Entity {
        public ObservableCollection<TEntity> Entities { get; set; }

        public bool IsReadOnly { get; set; }

        protected EntityViewModel() {
            DataGridAddingNewItemCommand = new DelegateCommand<AddingNewItemEventArgs>(DataGridAddingNewItem);
            PreviewKeyDownCommand = new DelegateCommand<object>(PreviewKeyDown);
            DataGridRowEditEndingCommand = new DelegateCommand<object>(DataGridRowEditEnding);
            PageLoadedCommand = new DelegateCommand<object>(PageLoaded);

            BookOrdersContext.NewEntityAdded += BookOrdersContext_NewEntityAdded;
            BookOrdersContext.UserStatusLoaded += BookOrdersContext_UserStatusLoaded;
        }

        private void BookOrdersContext_UserStatusLoaded(User user) {
            IsReadOnly = user == null || !new []{"ADMIN", "USER"}.Contains(user.Role);
        }

        protected abstract void BookOrdersContext_NewEntityAdded();

        public ICommand DataGridAddingNewItemCommand { get; set; }

        public ICommand PreviewKeyDownCommand { get; set; }

        public ICommand DataGridRowEditEndingCommand { get; set; }

        public ICommand PageLoadedCommand { get; set; }

        protected abstract void DataGridAddingNewItem(AddingNewItemEventArgs e);

        protected static void PreviewKeyDown(object eventArgs) {
            var tempEventArgs = (object[]) eventArgs;
            if (!(tempEventArgs[0] is KeyEventArgs e && tempEventArgs[1] is DataGrid sender)) {
                throw new ArgumentException("Bad arguments in PreviewKeyDown");
            }

            switch (e.Key) {
                case Key.Escape:
                    sender.CancelEdit();
                    sender.CancelEdit();
                    return;
                case Key.Delete:
                    if (!(sender.SelectedItem is TEntity entity)) return;
                    using (var context = new BookOrdersContext()) {
                        try {
                            PropertyInfo property =
                                context.GetType().GetProperty(typeof(TEntity).Name.Replace("And", "sAnd") + 's');
                            if (property != null) {
                                ((DbSet<TEntity>) property.GetValue(context)).Attach(entity);
                                ((DbSet<TEntity>) property.GetValue(context)).Remove(entity);
                            }

                            context.SaveChanges();
                        } catch (DbUpdateException exception) {
                            if (!(exception.InnerException is UpdateException innerException)) throw;
                            if (!(innerException.InnerException is Npgsql.PostgresException innerInnerException))
                                throw;
                            if (innerInnerException.SqlState == "23503") {
                                MessageBox.Show(innerInnerException.MessageText,
                                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                e.Handled = true;
                            } else {
                                throw;
                            }
                        }
                    }

                    return;
            }
        }

        protected static void DataGridRowEditEnding(object eventArgs) {
            var tempEventArgs = (object[])eventArgs;
            if (!(tempEventArgs[0] is DataGridRowEditEndingEventArgs e && tempEventArgs[1] is DataGrid sender)) {
                throw new ArgumentException("Bad arguments in PreviewKeyDown");
            }

            if (e.EditAction == DataGridEditAction.Cancel || !(e.Row.Item is TEntity entity) ||
                !entity.Validator.IsValid ||
                !entity.IsDirty)
                return;

            using (var context = new BookOrdersContext()) {
                try {
                    if (e.Row.IsNewItem) {
                        PropertyInfo property =
                            context.GetType().GetProperty(typeof(TEntity).Name.Replace("And", "sAnd") + 's');
                        if (property != null) ((DbSet<TEntity>)property.GetValue(context)).Add(entity);
                    } else {
                        context.Entry(entity).State = EntityState.Modified;
                    }

                    context.SaveChanges();
                } catch (Exception exception) {
                    sender.CancelEdit();
                    sender.CancelEdit();
                    Exception innerException = exception;
                    while (innerException?.InnerException != null) {
                        innerException = innerException.InnerException;
                    }

                    MessageBox.Show(innerException?.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        protected abstract void PageLoaded(object sender);

        protected void UpdateEntity<T>(IList<T> targetList, IList<T> sourceList, params object[] entities)
            where T : Entity {
            for (int i = targetList.Count - 1; i >= 0; --i) {
                if (!sourceList.Any(targetList[i].EqualsPrimaryKey)) {
                    targetList.Remove(targetList[i]);
                }
            }

            foreach (T entity in sourceList) {
                if (targetList.FirstOrDefault(entity.EqualsPrimaryKey) is T
                    tempEntity) {
                    tempEntity.CopyProperties(entity);
                } else {
                    targetList.Add(entity);
                    entity.InitializeValidator(entities);
                }
            }

            foreach (T entity in targetList) {
                entity.Validator?.Revalidate();
            }
        }

        protected static void ResizeComboBoxColumnToContent<T>(DataGridCellEditEndingEventArgs e, string columnName,
                                                               string propertyName, IList<T> sourceList)
            where T : Entity {
            if ((string) e.Column.Header != columnName) return;
            var comboBox = (ComboBox) e.EditingElement;
            if (comboBox.SelectedItem == null) return;
            PropertyInfo targetProperty = comboBox.SelectedItem.GetType().GetProperty(propertyName);
            if (targetProperty == null) return;
            T entityWithMaxString = sourceList
                                    .OrderByDescending(entity => targetProperty.GetValue(entity).ToString().Length)
                                    .First();
            double contentWidth =
                MeasureString(targetProperty.GetValue(entityWithMaxString).ToString(), comboBox).Width;
            double headerWidth = MeasureString(e.Column.Header.ToString(), comboBox).Width;
            e.Column.Width = (contentWidth > headerWidth ? contentWidth : headerWidth) + 12;
        }

        protected static Size MeasureString(string candidate, Control element) {
            var formattedText = new FormattedText(candidate,
                                                  CultureInfo.CurrentCulture,
                                                  FlowDirection.LeftToRight,
                                                  new Typeface(element.FontFamily, element.FontStyle,
                                                               element.FontWeight, element.FontStretch),
                                                  element.FontSize,
                                                  Brushes.Black,
                                                  new NumberSubstitution(),
                                                  1);

            return new Size(formattedText.Width, formattedText.Height);
        }
    }
}