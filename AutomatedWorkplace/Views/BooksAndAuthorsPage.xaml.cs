﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutomatedWorkplace.ViewModels;

namespace AutomatedWorkplace.Views {
    /// <summary>
    /// Interaction logic for BooksAndAuthorsPage.xaml
    /// </summary>
    public partial class BooksAndAuthorsPage : Page {
        public BooksAndAuthorsPage() {
            InitializeComponent();
            DataContext = new BooksAndAuthorsViewModel();
        }
    }
}
