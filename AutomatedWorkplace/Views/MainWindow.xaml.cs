using System.Windows;
using System.Windows.Controls;
using AutomatedWorkplace.Models;
using AutomatedWorkplace.ViewModels;

namespace AutomatedWorkplace.Views {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel(this);
        }
    }
}