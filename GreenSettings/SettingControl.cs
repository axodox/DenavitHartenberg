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

namespace Green.Settings.UI
{
    /// <summary>
    /// Interaction logic for CheckBox.xaml
    /// </summary>
    public partial class SettingControl : UserControl
    {
        public Setting TargetSetting { get; private set; }
        public SettingControl()
        {
            DataContextChanged += SettingControl_DataContextChanged;
            MouseDoubleClick += SettingControl_MouseDoubleClick;
        }

        void SettingControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is Setting) TargetSetting = DataContext as Setting;
        }

        void SettingControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TargetSetting != null) TargetSetting.ResetValue();
        }
    }
}
