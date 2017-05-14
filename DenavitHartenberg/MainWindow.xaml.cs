using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Green.Settings;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Threading;
using System.IO;
using System.Windows.Data;

namespace DenavitHartenberg
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            InitSettings();
            InitPresets();

            GraphicsCore.Loaded += GraphicsCore_Loaded;
        }

        void GraphicsCore_Loaded(object sender, RoutedEventArgs e)
        {
            SetView();
            SetModel();
            SetShading();
            SetActiveJoint();

            InitViewManipulation();
        }

        #region Presets
        void InitPresets()
        {
            Presets.ItemsSource = Directory.GetFiles(Directory.GetCurrentDirectory(), PresetToMenuConverter.PresetPrefix + "*.ini");
        }

        private void Preset_Click(object sender, RoutedEventArgs e)
        {
            Settings.Load((string)(sender as Button).Tag);
        }
        #endregion

        #region Settings
        SettingManager Settings;
        SettingGroup ModelSettingGroup;
        DenavitHartenbergSetting Model;
        NumericSetting<float> ComponentSize;

        SettingGroup ViewProperties;
        NumericSetting<float> RotationHorizontal, RotationVertical, Distance;

        SettingGroup ShadingProperties;
        NumericSetting<float> AmbientLight, DiffuseLight, SpecularLight, Reflectivity;
        BooleanSetting CoordSystemsAreVisible, MoveLightWithCamera, SkyboxVisible;
        ColorSetting BackgroundColor, BaseColor, ManipulatorColor, HighlightColor, FloorColor;
        

        void InitSettings()
        {
            Model = new DenavitHartenbergSetting("Model") { FriendlyName = "Denavit-Hartenberg system" };
            Model.ActiveJointChanged += (object o, EventArgs e) => { SetActiveJoint(); };
            ComponentSize = new NumericSetting<float>("ComponentSize", 1f, 0f, 10f, 2) { FriendlyName = "Component size" };

            RotationHorizontal = new NumericSetting<float>("RotationX", 0f, -180f, 180f, 2) { FriendlyName = "Horizontal rotation" };
            RotationVertical = new NumericSetting<float>("RotationY", 0f, -90f, 90f, 2) { FriendlyName = "Vertical rotation" };
            Distance = new NumericSetting<float>("Distance", 3f, 1f, 8f, 2) { FriendlyName = "Distance" };
            
            CoordSystemsAreVisible = new BooleanSetting("CoordSystemsAreVisible", false) { FriendlyName = "Coordinate systems are visible" };
            BackgroundColor = new ColorSetting("BackgroundColor", 1f, 1f, 1f, 1f) { FriendlyName = "Background color" };
            FloorColor = new ColorSetting("FloorColor", 1f, 1f, 1f, 1f) { FriendlyName = "Floor color" };
            BaseColor = new ColorSetting("BaseColor", 1f, 1f, 1f, 1f) { FriendlyName = "Base color" };
            ManipulatorColor = new ColorSetting("ManipulatorColor", 1f, 0f, 1f, 1f) { FriendlyName = "Manipulator color" };
            HighlightColor = new ColorSetting("HighlightColor", 1f, 1f, 0f, 1f) { FriendlyName = "Highlight color" };
            MoveLightWithCamera = new BooleanSetting("MoveLightWithCamera", true) { FriendlyName = "Move light with camera" };
            AmbientLight = new NumericSetting<float>("AmbientLight", 0.1f, 0f, 1f, 2) { FriendlyName = "Ambient light intensity" };
            DiffuseLight = new NumericSetting<float>("DiffuseLight", 4f, 0f, 10f, 2) { FriendlyName = "Diffuse light intensity" };
            SpecularLight = new NumericSetting<float>("SpecularLight", 2f, 0f, 10f, 2) { FriendlyName = "Specular light intensity" };
            Reflectivity = new NumericSetting<float>("Reflectivity", 0.2f, 0f, 1f, 2) { FriendlyName = "Reflectivity" };
            SkyboxVisible = new BooleanSetting("SkyboxVisible", false) { FriendlyName = "Skybox visible" };

            ModelSettingGroup = new SettingGroup("Model") { FriendlyName = "Model" };
            ModelSettingGroup.ValueChanged += (object o, EventArgs e) => { SetModel(); };
            ModelSettingGroup.Settings.Add(Model);
            ModelSettingGroup.Settings.Add(ComponentSize);

            ViewProperties = new SettingGroup("View") { FriendlyName = "View"};
            ViewProperties.ValueChanged += (object o, EventArgs e) => { SetView(); };
            ViewProperties.Settings.Add(RotationHorizontal);
            ViewProperties.Settings.Add(RotationVertical);
            ViewProperties.Settings.Add(Distance);

            ShadingProperties = new SettingGroup("Shading") { FriendlyName = "Shading" };
            ShadingProperties.ValueChanged += (object o, EventArgs e) => { SetShading(); };
            ShadingProperties.Settings.Add(CoordSystemsAreVisible);
            ShadingProperties.Settings.Add(BackgroundColor);
            ShadingProperties.Settings.Add(FloorColor);
            ShadingProperties.Settings.Add(BaseColor);
            ShadingProperties.Settings.Add(ManipulatorColor);
            ShadingProperties.Settings.Add(HighlightColor);
            ShadingProperties.Settings.Add(MoveLightWithCamera);
            ShadingProperties.Settings.Add(AmbientLight);
            ShadingProperties.Settings.Add(DiffuseLight);
            ShadingProperties.Settings.Add(SpecularLight);
            ShadingProperties.Settings.Add(Reflectivity);
            ShadingProperties.Settings.Add(SkyboxVisible);
            
            Settings = new SettingManager();
            Settings.SettingGroups.Add(ModelSettingGroup);
            Settings.SettingGroups.Add(ViewProperties);
            Settings.SettingGroups.Add(ShadingProperties);
            Settings.Load("Settings.ini");
            SMC.DataContext = Settings;
        }

        private void SetView()
        {
            GraphicsCore.SetView(
                RotationHorizontal.Value,
                RotationVertical.Value,
                Distance.Value);
        }

        private void SetShading()
        {
            GraphicsCore.SetShading(
                CoordSystemsAreVisible.Value,
                BackgroundColor.Value.ToArray(),
                FloorColor.Value.ToArray(),
                BaseColor.Value.ToArray(),
                ManipulatorColor.Value.ToArray(),
                HighlightColor.Value.ToArray(),
                MoveLightWithCamera.Value,
                AmbientLight.Value,
                DiffuseLight.Value,
                SpecularLight.Value,
                Reflectivity.Value,
                SkyboxVisible.Value);
        }

        private void SetActiveJoint()
        {
            GraphicsCore.ActiveJoint = 
                Model.ActiveJoint == null ? -1 : Model.ActiveJoint.Number;
        }

        private void SetModel()
        {
            GraphicsCore.SetModel(
                Model.ToJointArray(),
                Model.ToParameterArray(),
                Model.JointCount,
                ComponentSize.Value);
        }
        #endregion

        #region View manipulation
        DispatcherTimer MainDispatcher;
        private void InitViewManipulation()
        {
            MainDispatcher = new DispatcherTimer(DispatcherPriority.Send);
            MainDispatcher.Interval = new TimeSpan(0, 0, 0, 0, 30);
            MainDispatcher.Tick += DT_Tick;
            MainDispatcher.IsEnabled = true;
        }

        const float ZoomStep = 1.1f;
        private void Window_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta < 0)
                Distance.Value *= ZoomStep;
            else
                Distance.Value /= ZoomStep;
        }

        bool IsMouseOnCanvas
        {
            get
            {
                Point p = Mouse.GetPosition(GraphicsCore);
                return p.X > 0 && p.Y > 0 && p.X < GraphicsCore.ActualWidth && p.Y < GraphicsCore.ActualHeight;
            }
        }

        bool ViewMouseManipulation = false;
        Point CursorStartPos;
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOnCanvas) return;
            CursorStartPos = Mouse.GetPosition(null);
            ViewMouseManipulation = true;
            Mouse.Capture(this);
        }

        void DT_Tick(object sender, EventArgs e)
        {
            if (!ViewMouseManipulation) return;
            if (Mouse.LeftButton == MouseButtonState.Released)
            {
                ViewMouseManipulation = false;
                Mouse.Capture(null);
                return;
            }
            Point cursorPos = Mouse.GetPosition(null);
            float deltaX = (float)(cursorPos.X - CursorStartPos.X);
            float deltaY = (float)(cursorPos.Y - CursorStartPos.Y);

            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                RotationHorizontal.Value -= deltaX * 0.2f;
                RotationVertical.Value += deltaY * 0.2f;
            }
            CursorStartPos = cursorPos;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseOnCanvas) return;
            ViewProperties.ResetToDefault();
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Save("Settings.ini");
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class PresetToMenuConverter : IValueConverter
    {
        public const string PresetPrefix = "Preset";
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Path.GetFileNameWithoutExtension(value as string).Substring(PresetPrefix.Length);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
