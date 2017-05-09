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
using Microsoft.Win32;
using Green.Settings;

namespace Green.Settings.UI
{
    /// <summary>
    /// Interaction logic for SettingManagerControl.xaml
    /// </summary>
    public partial class SettingManagerControl : UserControl
    {
        public Visibility MenuVisibility
        {
            get { return SettingsMenu.Visibility; }
            set { SettingsMenu.Visibility = value; }
        }

        public SettingManagerControl()
        {
            InitializeComponent();
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            if (SM != null)
            {
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.Filter = "Setting files (*.ini)|*.ini";
                OFD.Title = "Import settings";
                if (OFD.ShowDialog() == true)
                {
                    if (!SM.Load(OFD.FileName))
                    {
                        MessageBox.Show("Import was unsuccessful.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void Export_Click(object sender, RoutedEventArgs e)
        {
            if (SM != null)
            {
                SaveFileDialog SFD = new SaveFileDialog();
                SFD.Filter = "Setting files (*.ini)|*.ini";
                SFD.Title = "Export settings";
                if (SFD.ShowDialog() == true)
                {
                    if (!SM.Save(SFD.FileName))
                    {
                        MessageBox.Show("Export was unsuccessful.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        SettingManager SM;
        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is SettingManager)
            {
                SM = DataContext as SettingManager;
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            foreach (SettingGroup sg in SettingGroups.Items) sg.ResetToDefault();
        }
    }
}
