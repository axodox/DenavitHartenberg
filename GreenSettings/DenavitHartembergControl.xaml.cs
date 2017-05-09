using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Green.Settings;
using System.Globalization;
using System.Windows.Input;
using System.Windows.Media;

namespace Green.Settings.UI
{
    public partial class DenavitHartembergControl : UserControl
    {
        public DenavitHartembergControl()
        {
            InitializeComponent();
        }

        private void AddTransformation_Click(object sender, RoutedEventArgs e)
        {
            DenavitHartenbergSetting system = DataContext as DenavitHartenbergSetting;
            if (system == null) return;
            system.Joints.Add(new DenavitHartenbergSetting.Joint());
        }

        private void RemoveTransformation_Click(object sender, RoutedEventArgs e)
        {
            DenavitHartenbergSetting system = DataContext as DenavitHartenbergSetting;
            if (system == null) return;
            system.Joints.Remove((sender as Button).Tag as DenavitHartenbergSetting.Joint);
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is Setting) (DataContext as Setting).ResetValue();
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            Grid grid = sender as Grid;
            int index = (grid.Tag as DenavitHartenbergSetting.Joint).Number;
            DenavitHartenbergSetting dhs = DataContext as DenavitHartenbergSetting;
            dhs.ActiveJoint = grid.Tag as DenavitHartenbergSetting.Joint;
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            DenavitHartenbergSetting dhs = DataContext as DenavitHartenbergSetting;
            dhs.ActiveJoint = null;
        }

        private void Minus90Degrees_Click(object sender, RoutedEventArgs e)
        {
            DenavitHartenbergSetting.Joint joint = (DenavitHartenbergSetting.Joint)(sender as Button).Tag;
            joint.X -= (float)(Math.PI / 2);
        }

        private void Plus90Degrees_Click(object sender, RoutedEventArgs e)
        {
            DenavitHartenbergSetting.Joint joint = (DenavitHartenbergSetting.Joint)(sender as Button).Tag;
            joint.X += (float)(Math.PI / 2);
        }

        private void Zero_Click(object sender, RoutedEventArgs e)
        {
            DenavitHartenbergSetting.Joint joint = (DenavitHartenbergSetting.Joint)(sender as Button).Tag;
            joint.X = 0f;
        }

        private void MinusHalf_Click(object sender, RoutedEventArgs e)
        {
            DenavitHartenbergSetting.Joint joint = (DenavitHartenbergSetting.Joint)(sender as Button).Tag;
            joint.X -= 0.5f;
        }

        private void PlusHalf_Click(object sender, RoutedEventArgs e)
        {
            DenavitHartenbergSetting.Joint joint = (DenavitHartenbergSetting.Joint)(sender as Button).Tag;
            joint.X += 0.5f;
        }
    }
}
