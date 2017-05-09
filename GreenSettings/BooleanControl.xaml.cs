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

namespace Green.Settings.UI
{
    /// <summary>
    /// Interaction logic for CheckBox.xaml
    /// </summary>
    public partial class BooleanControl : UserControl
    {
        public BooleanControl()
        {
            InitializeComponent();
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is Setting) (DataContext as Setting).ResetValue();
        }
    }
}
