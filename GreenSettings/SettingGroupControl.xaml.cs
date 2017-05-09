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
    /// Interaction logic for SettingGroupControl.xaml
    /// </summary>
    public partial class SettingGroupControl : UserControl
    {
        public SettingGroupControl()
        {
            InitializeComponent();
        }

        public void Expand()
        {
            Expander.IsExpanded = true;
        }

        public void Collapse()
        {
            Expander.IsExpanded = false;
        }
    }
}
