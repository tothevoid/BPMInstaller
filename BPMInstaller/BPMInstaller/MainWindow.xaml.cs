using BPMInstaller.Core.Model.Enums;
using BPMInstaller.Core.Services;
using System;
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
using System.Xml.Linq;

namespace BPMInstaller
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            var service = new PostgresDatabaseService(new Core.Model.DatabaseConfig { Host = "localhost", UserName = "postgres", Password = "admin", 
                BackupPath = "", DatabaseMode = DatabaseMode.Docker,
                DatabaseName = "bpm"});

            if (!service.ValidateConnection())
            {
                return;
            }

            if (!service.CreateDatabase())
            {
                return;
            }

            service.RestoreDatabase();
        }
    }
}
