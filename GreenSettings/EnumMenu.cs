using Green.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Green.Settings.UI
{
    public class EnumMenu : MenuItem
    {
        public EnumMenu()
            : base()
        {
            DataContextChanged += EnumMenu_DataContextChanged;
        }

        void EnumMenu_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!(DataContext is EnumSetting)) return;

            EnumSetting ES = DataContext as EnumSetting;
            string[] values = ES.FriendlyOptions;
            int count = values.Length;
            if (count == 0) return;

            string friendlyValue = ES.FriendlyValue;
            for (int i = 0; i < count; i++)
            {
                MenuItem MI = new MenuItem() { Header = values[i], IsChecked = friendlyValue == values[i] };
                MI.Click += MenuItem_Click;
                Items.Add(MI);
            }

            ES.ValueChanged+=ES_ValueChanged;
        }

        void ES_ValueChanged(object sender, EventArgs e)
        {
            EnumSetting s = (EnumSetting)sender;
            SetCheck(Items[s.SelectedIndex] as MenuItem);
        }

        void MenuItem_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MenuItem MI = sender as MenuItem;
            EnumSetting s = (EnumSetting)DataContext;
            s.FriendlyValue = (string)MI.Header;
            SetCheck(MI);
        }

        void SetCheck(MenuItem mi)
        {
            foreach (MenuItem i in Items) i.IsChecked = false;
            mi.IsChecked = true;
        }
    }
}
