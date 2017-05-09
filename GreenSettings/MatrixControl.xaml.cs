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
    /// Interaction logic for MatrixControl.xaml
    /// </summary>
    public partial class MatrixControl : UserControl
    {
        FloatBox[,] Table;
        public MatrixControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(DataContext is MatrixSetting)) return;

            MatrixSetting MS = DataContext as MatrixSetting;
            Table = new FloatBox[MS.Rows, MS.Columns];
            GMatrix.RowDefinitions.Clear();
            for (int i = 0; i < MS.Rows; i++)
                GMatrix.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

            GMatrix.ColumnDefinitions.Clear();
            for (int i = 0; i < MS.Columns; i++)
                GMatrix.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1d, GridUnitType.Star) });

            for (int r = 0; r < MS.Rows; r++)
                for (int c = 0; c < MS.Columns; c++)
                {
                    FloatBox FB = new FloatBox();
                    FB.Margin = new Thickness(3);
                    FB.Value = MS.Value[r, c];
                    FB.TextAlignment = TextAlignment.Center;
                    FB.ValueChanged += FB_ValueChanged;
                    GMatrix.Children.Add(FB);
                    Grid.SetRow(FB, r);
                    Grid.SetColumn(FB, c);
                    Table[r, c] = FB;
                }

            MS.ValueChanged += MS_ValueChanged;
        }

        void MS_ValueChanged(object sender, EventArgs e)
        {
            if(!skipNext) UpdateTable((sender as MatrixSetting).Value);
        }

        bool skipNext = false;
        void FB_ValueChanged(object sender, EventArgs e)
        {
            if (!(DataContext is MatrixSetting)) return;
            MatrixSetting MS = DataContext as MatrixSetting;
            FloatBox FB = sender as FloatBox;
            int row = Grid.GetRow(FB);
            int col = Grid.GetColumn(FB);
            skipNext = true;
            MS[row, col] = FB.Value;
            skipNext = false;
        }

        private void UpdateTable(float[,] matrix)
        {
            for (int r = 0; r < matrix.GetLength(0); r++)
                for (int c = 0; c < matrix.GetLength(1); c++)
                {
                    Table[r, c].Value = matrix[r, c];
                }
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MatrixSetting)
                (DataContext as MatrixSetting).ResetValue();
        }
    }
}
