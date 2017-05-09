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
    /// Interaction logic for ColorControl.xaml
    /// </summary>
    public partial class ColorControl : UserControl
    {
        public ColorControl()
        {
            InitializeComponent();
        }

        private void Rectangle_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ColorSetting setting = DataContext as ColorSetting;
            using (System.Windows.Forms.ColorDialog colorDialog = new System.Windows.Forms.ColorDialog())
            {
                colorDialog.Color = System.Drawing.Color.FromArgb(
                    setting.Value.bA,
                    setting.Value.bR,
                    setting.Value.bG,
                    setting.Value.bB);
                if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    setting.Value = new ColorSetting.Color(
                        colorDialog.Color.A,
                        colorDialog.Color.R,
                        colorDialog.Color.G,
                        colorDialog.Color.B);
                }
            }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            (DataContext as ColorSetting).ResetValue();
        }
    }
}
