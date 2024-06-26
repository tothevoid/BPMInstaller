﻿using System.Windows;
using BPMInstaller.UI.Desktop.Models.Basics;
using BPMInstaller.UI.Desktop.ViewModels;

namespace BPMInstaller.UI.Desktop
{
    // ReSharper disable once UnusedMember.Global
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new InstallationConfigurationViewModel(new ViewModelContextSynchronizer(Dispatcher));
            DataContext = vm.GetModel();
        }
    }
}
