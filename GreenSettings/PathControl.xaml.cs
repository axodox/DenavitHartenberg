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
using Green.Settings;

namespace Green.Settings.UI
{
    /// <summary>
    /// Interaction logic for PathControl.xaml
    /// </summary>
    public partial class PathControl : UserControl
    {
        public PathControl()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is PathSetting)) return;
            PathSetting ps = DataContext as PathSetting;
            using (System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.SelectedPath = ps.AbsolutePath;
                fbd.Description = string.Format(Green.Properties.Resources.BrowseDescription, ps.FriendlyName);
                if(fbd.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                {
                    ps.Value = fbd.SelectedPath;
                }
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is Setting) (DataContext as Setting).ResetValue();
        }
    }
}
