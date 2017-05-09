using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Green.Settings
{
    public abstract class SettingAvailabilityProvider
    {
        public event EventHandler AvailabilityChanged;
        private bool isAvailable = false;
        public bool IsAvailable 
        {
            get
            {
                return isAvailable;
            }
            protected set
            {
                if (isAvailable != value)
                {
                    isAvailable = value;
                    if (AvailabilityChanged != null)
                        AvailabilityChanged(this, EventArgs.Empty);
                }
            }
        }        
    }
    
    public class DependentAvailability : SettingAvailabilityProvider
    {
        private Setting[] Settings;
        private string[][] Values;
        public DependentAvailability(Setting setting, string value)
            : this(new Setting[] { setting }, new string[] { value }) { }

        public DependentAvailability(Setting[] settings, string[] values)
        {
            if (settings.Length != values.Length) throw new ArgumentException("The count of settings and values must be equal.");
            Settings = settings;            
            foreach (Setting setting in settings)
            {
                setting.ValueChanged += setting_ValueChanged;
            }
            Values = new string[values.Length][];
            for(int i = 0; i<Values.Length;i++)
            {
                Values[i] = values[i].Split('|');
            }
            EvaluateAvailability();
        }

        void setting_ValueChanged(object sender, EventArgs e)
        {
            EvaluateAvailability();
        }

        void EvaluateAvailability()
        {
            bool available = true;
            for (int i = 0; i < Settings.Length; i++)
            {
                if (!Values[i].Contains(Settings[i].StringValue))
                {
                    available = false;
                    break;
                }
            }
            IsAvailable = available;
        }
    }

    public abstract class Setting : INotifyPropertyChanged
    {
        public bool IsAvailable { get; private set; }

        private SettingAvailabilityProvider availabilityProvider = null;
        public SettingAvailabilityProvider AvailabilityProvider
        {
            get
            {
                return availabilityProvider;
            }
            set
            {
                availabilityProvider = value;
                availabilityProvider.AvailabilityChanged += AvailabilityProvider_AvailabilityChanged;
                IsAvailable = availabilityProvider.IsAvailable;
            }
        }

        void AvailabilityProvider_AvailabilityChanged(object sender, EventArgs e)
        {
            IsAvailable = availabilityProvider.IsAvailable;
            OnPropertyChanged("IsAvailable");
        }

        public static readonly CultureInfo Culture = new CultureInfo("en-US");

        public Setting(string name)
        {
            Name = name;
            IsAvailable = true;
        }

        public string Name { get; protected set; }
        public SettingGroup Group { get; internal set; }

        [Flags]
        public enum Types : uint { 
            Unknown = 0, 
            Boolean = 1, 
            Enum = 2, 
            SByte = 4, 
            Int16 = 8, 
            Int32 = 16, 
            Int64 = 32, 
            Byte = 64, 
            UInt16 = 128, 
            UInt32 = 256, 
            UInt64 = 512, 
            Single = 1024, 
            Double = 2048, 
            Matrix = 4096, 
            String = 8192, 
            Path = 16384, 
            Rectangle = 32768, 
            Size = 65536,
            Numeric = 4092, 
            Integer = 1020 
        };
        public Types Type { get; protected set; }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler ValueChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        protected virtual void OnValueChanged()
        {
            OnPropertyChanged("Value");
            OnPropertyChanged("StringValue");
            OnPropertyChanged("HasDefaultValue");
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }

        public abstract string StringValue { get; set; }
        public abstract bool HasDefaultValue { get; }
        public abstract void ResetValue();

        private string friendlyName = null;
        public string FriendlyName
        {
            get
            {
                if (friendlyName == null) return Name;
                else return friendlyName;
            }
            set
            {
                friendlyName = value;
            }
        }

        private bool isHidden;
        public bool IsHidden
        {
            get { return isHidden; }
            set
            {
                isHidden = value;
                OnPropertyChanged("IsHidden");
            }
        }

        public string StoredValue { get; private set; }
        public void StoreValue()
        {
            StoredValue = StringValue;
        }

        public void RestoreValue()
        {
            if (StoredValue != null && !IsReadOnly)
            {
                StringValue = StoredValue;
                StoredValue = null;
            }
        }

        private bool isReadOnly;
        public bool IsReadOnly
        {
            get { return isReadOnly; }
            set
            {
                isReadOnly = value;
                OnPropertyChanged("IsReadOnly");
            }
        }
    }
    public class ColorSetting : Setting
    {
        public ColorSetting(string name, float r, float g, float b, float a)
            : base(name)
        {
            Value = DefaultValue = new Color(r, g, b, a);
        }

        public struct Color
        {
            public float R, G, B, A;
            public byte bR
            {
                get { return (byte)(R * 255); }
                set { R = value / 255f; }
            }
            public byte bG
            {
                get { return (byte)(G * 255); }
                set { G = value / 255f; }
            }
            public byte bB
            {
                get { return (byte)(B * 255); }
                set { B = value / 255f; }
            }
            public byte bA
            {
                get { return (byte)(A * 255); }
                set { A = value / 255f; }
            }

            public Color(float a, float r, float g, float b)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }
            public Color(byte a, byte r, byte g, byte b)
            {
                R = r / 255f;
                G = g / 255f;
                B = b / 255f;
                A = a / 255f;
            }

            public static bool operator ==(Color a, Color b)
            {
                return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A;
            }

            public static bool operator !=(Color a, Color b)
            {
                return !(a == b);
            }

            public override string ToString()
            {
                return string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}", bA, bR, bG, bB);
            }

            public static Color Parse(string text)
            {
                return new Color(
                    byte.Parse(text.Substring(1, 2), NumberStyles.HexNumber),
                    byte.Parse(text.Substring(3, 2), NumberStyles.HexNumber),
                    byte.Parse(text.Substring(5, 2), NumberStyles.HexNumber),
                    byte.Parse(text.Substring(7, 2), NumberStyles.HexNumber));
            }

            public float[] ToArray()
            {
                return new float[] { R, G, B, A };
            }
        }

        public Color DefaultValue { get; private set; }
        private Color value;
        public Color Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                OnValueChanged();
            }
        }

        public override string StringValue
        {
            get
            {
                return Value.ToString();
            }
            set
            {
                Value = Color.Parse(value);
            }
        }

        public override bool HasDefaultValue
        {
            get { return DefaultValue == Value; }
        }

        public override void ResetValue()
        {
            Value = DefaultValue;
        }
    }

    public class MatrixSetting : Setting
    {
        public float[,] DefaultValue { get; private set; }
        private float[,] value;
        public float[,] Value
        {
            get { return value; }
            set
            {
                if (IsReadOnly) return;
                this.value = value;
                OnValueChanged();
            }
        }

        public float this[int x, int y]
        {
            get
            {
                return value[x, y];
            }
            set
            {
                if (IsReadOnly) return;
                this.value[x, y] = value;
                OnValueChanged();
            }
        }

        public override string StringValue
        {
            get
            {
                string s = "";
                for (int r = 0; r < Rows; r++)
                {
                    if (r != 0) s += "|";
                    for (int c = 0; c < Columns; c++)
                    {
                        if (c != 0) s += ";";
                        s += value[r, c].ToString(Culture);
                    }
                }
                return s;
            }
            set
            {
                if (IsReadOnly) return;
                try
                {
                    string[] lines = value.Split('|');
                    float[,] newValue = new float[Rows, Columns];
                    for (int r = 0; r < Rows; r++)
                    {
                        string[] line = lines[r].Split(';');
                        for (int c = 0; c < Columns; c++)
                        {
                            newValue[r, c] = float.Parse(line[c], Culture);
                        }
                    }
                    Value = newValue;
                }
                catch { }
            }
        }

        public int Rows { get; private set; }
        public int Columns { get; private set; }
        public MatrixSetting(string name, float[,] value)
            : base(name)
        {
            Type = Types.Matrix | Types.Single;
            Rows = value.GetLength(0);
            Columns = value.GetLength(1);
            this.value = value;
            DefaultValue = new float[Rows, Columns];
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                    DefaultValue[r, c] = value[r, c];
        }

        public override bool HasDefaultValue
        {
            get
            {
                for (int r = 0; r < Rows; r++)
                    for (int c = 0; c < Columns; c++)
                        if (DefaultValue[r, c] != value[r, c]) return false;
                return true;
            }
        }

        public override void ResetValue()
        {
            if (IsReadOnly) return;
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Columns; c++)
                    value[r, c] = DefaultValue[r, c];
            OnValueChanged();
        }
    }

    public abstract class NumericSetting : Setting 
    {
        public int Decimals { get; protected set; }
        public NumericSetting(string name) : base(name) { }
    }

    public class NumericSetting<T> : NumericSetting where T : struct
    {
        private T value;
        public T Value 
        {
            get { return value; }
            set
            {
                if (IsReadOnly) return;
                if ((int)typeof(T).InvokeMember("CompareTo", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, value, new object[] { Maximum }) > 0)
                    this.value = Maximum;
                else if ((int)typeof(T).InvokeMember("CompareTo", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, value, new object[] { Minimum }) < 0)
                    this.value = Minimum;
                else
                    this.value = value;
                OnValueChanged();
            }
        }
        public T Maximum { get; private set; }
        public T Minimum { get; private set; }
        public T DefaultValue { get; private set; }        
        
        public NumericSetting(string name, T value, T min, T max, int decimals = 0) : base(name)
        {
            Types type;
            if (Enum.TryParse<Types>(typeof(T).Name, out type) && Types.Numeric.HasFlag(type))
                Type = type;
            else
                throw new NotSupportedException("The given datatype is not supported!");

            Maximum = max;
            Minimum = min;
            DefaultValue = Value = value;
            Decimals = decimals;
        }

        public override string StringValue
        {
            get
            {
                return (string)typeof(T).InvokeMember("ToString", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, value, new object[] { "F" + Decimals, Culture });
            }
            set
            {
                if (IsReadOnly) return;
                try
                {
                    Value = (T)typeof(T).InvokeMember("Parse", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, new object[] { value, Culture });
                }
                catch { }
            }
        }

        public override bool HasDefaultValue
        {
            get { return DefaultValue.Equals(value); }
        }

        public override void ResetValue()
        {
            if (IsReadOnly) return;
            Value = DefaultValue;
        }
    }

    public class StringSetting : Setting
    {
        public char[] InvalidChars { get; private set; }
        public string DefaultValue { get; private set; }
        private string value;
        public string Value
        {
            get { return value; }
            set 
            {
                if (IsReadOnly) return;
                if (InvalidChars != null)
                {
                    string s = "";
                    for (int i = 0; i < value.Length; i++)
                    {
                        if (InvalidChars.Contains(value[i]))
                            s += '_';
                        else
                            s += value[i];
                    }
                    this.value = s;
                }
                else
                    this.value = value;
                OnValueChanged();
            }
        }

        public override string StringValue
        {
            get { return value; }
            set 
            { 
                if (IsReadOnly) return; 
                Value = value; 
            }
        }

        public override bool HasDefaultValue
        {
            get { return DefaultValue == value; }
        }

        public override void ResetValue()
        {
            if (IsReadOnly) return;
            Value = DefaultValue;
        }

        public StringSetting(string name, string value, char[] invalidChars = null) : base(name)
        {
            DefaultValue = Value = value;
            Type = Types.String;
            InvalidChars = invalidChars;
        }
    }

    public class PathSetting : StringSetting
    {
        public PathSetting(string name, string value)
            : base(name, value, Path.GetInvalidPathChars())
        {
            Type = Types.Path;
        }

        protected override void OnValueChanged()
        {
            base.OnValueChanged();
            OnPropertyChanged("AbsolutePath");
            OnPropertyChanged("Exists");
        }

        public static string GetAbsolutePath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                if (!path.EndsWith("\\")) path += "\\";
                return path;
            }
            else
            {
                string cd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!cd.EndsWith("\\")) cd += "\\";
                string value = path;
                if (value.StartsWith("\\")) value = path.Substring(1);
                if (value != "" && !value.EndsWith("\\")) value += "\\";
                return cd + value;
            }
        }

        public string AbsolutePath
        {
            get
            {
                return GetAbsolutePath(Value);
            }
        }

        public bool Exists
        {
            get
            {
                return Directory.Exists(AbsolutePath);
            }
        }
    }

    public class SizeSetting : Setting
    {
        public int MinWidth { get; set; }
        public int MinHeight { get; set; }
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }

        private int width, height;
        public int Width
        {
            get { return width; }
            set
            {
                if (IsReadOnly) return;
                if (value < MinWidth) value = MinWidth;
                if (value > MaxWidth) value = MaxWidth;
                width = value;
                OnValueChanged();
                OnPropertyChanged("Width");
                OnPropertyChanged("StringValue");
                OnPropertyChanged("HasDefaultValue");
            }
        }
        public int Height
        {
            get { return height; }
            set
            {
                if (IsReadOnly) return;
                if (value < MinHeight) value = MinHeight;
                if (value > MaxHeight) value = MaxHeight;
                height = value;
                OnValueChanged();
                OnPropertyChanged("Height");
                OnPropertyChanged("StringValue");
                OnPropertyChanged("HasDefaultValue");
            }
        }

        public int DefaultWidth { get; private set; }
        public int DefaultHeight { get; private set; }

        public SizeSetting(string name, int width, int height, int minWidth, int minHeight, int maxWidth, int maxHeight)
            : base(name)
        {
            MinWidth = minWidth;
            MinHeight = minHeight;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            Type = Types.Size | Types.Int32;
            DefaultWidth = Width = width;
            DefaultHeight = Height = height;
        }

        public override string StringValue
        {
            get
            {
                return String.Format(Culture, "{0}x{1}", new object[] { Width, Height});
            }
            set
            {
                if (IsReadOnly) return;
                string[] items = value.Split('x');
                try
                {
                    Width = int.Parse(items[0], Culture);
                    Height = int.Parse(items[1], Culture);
                }
                catch { }
            }
        }

        public override bool HasDefaultValue
        {
            get { return DefaultWidth == Width && DefaultHeight == Height; }
        }

        public override void ResetValue()
        {
            if (IsReadOnly) return;
            Width = DefaultWidth;
            Height = DefaultHeight;
        }
    }

    public class RectangleSetting : Setting
    {
        private Rect value;
        public Rect Value
        {
            get { return value; }
            set
            {
                if (IsReadOnly) return;
                this.value = value;
                OnValueChanged();
            }
        }

        public Rect DefaultValue { get; private set; }

        public RectangleSetting(string name, Rect value)
            : base(name)
        {
            Type = Types.Rectangle | Types.Double;
            DefaultValue = Value = value;
        }

        public override string StringValue
        {
            get
            {
                return String.Format(Culture, "{0};{1};{2};{3}", new object[] { Value.X, Value.Y, Value.Width, Value.Height });
            }
            set
            {
                if (IsReadOnly) return;
                string[] items = value.Split(';');
                try
                {
                    Value = new Rect(
                        double.Parse(items[0], Culture),
                        double.Parse(items[1], Culture),
                        double.Parse(items[2], Culture),
                        double.Parse(items[3], Culture)
                        );

                }
                catch { }
            }
        }

        public override bool HasDefaultValue
        {
            get { return DefaultValue == Value; }
        }

        public override void ResetValue()
        {
            if (IsReadOnly) return;
            DefaultValue = Value;
        }
    }

    public class BooleanSetting : Setting
    {
        private bool value;
        public bool Value
        {
            get { return value; }
            set
            {
                if (IsReadOnly) return;
                this.value = value;
                OnValueChanged();
            }
        }

        public bool DefaultValue { get; private set; }

        public BooleanSetting(string name, bool value) : base(name)
        {
            Type = Types.Boolean;
            DefaultValue=Value = value;
        }

        public override string StringValue
        {
            get { return value.ToString(); }
            set
            {
                if (IsReadOnly) return;
                bool newValue;
                if (Boolean.TryParse(value, out newValue))
                {
                    Value = newValue;
                }
            }
        }

        public override bool HasDefaultValue
        {
            get { return DefaultValue == value; }
        }

        public override void ResetValue()
        {
            if (IsReadOnly) return;
            Value = DefaultValue;
        }
    }

    public abstract class EnumSetting : Setting
    {
        public EnumSetting(string name) : base(name) { }
        public string[] StringOptions { get; protected set; }

        private string[] friendlyOptions = null;
        public string[] FriendlyOptions
        {
            get
            {
                if (friendlyOptions == null) return StringOptions;
                else return friendlyOptions;
            }
            set
            {
                if (IsReadOnly) return;
                if (value.Length == StringOptions.Length)
                    friendlyOptions = value;
            }
        }

        public int SelectedIndex
        {
            get
            {
                string strval = StringValue;
                for (int i = 0; i < StringOptions.Length; i++)
                {
                    if (StringOptions[i] == strval)
                    {
                        return i;
                    }
                }
                return -1;
            }
            set
            {
                if (IsReadOnly) return;
                if (value > 0 && value < StringOptions.Length)
                    StringValue = StringOptions[value];
            }
        }

        public string FriendlyValue
        {
            get
            {
                return FriendlyOptions[SelectedIndex];
            }
            set
            {
                if (IsReadOnly) return;
                for (int i = 0; i < FriendlyOptions.Length; i++)
                {
                    if (FriendlyOptions[i] == value)
                    {
                        StringValue = StringOptions[i];
                        break;
                    }
                }
            }
        }
        protected override void OnValueChanged()
        {
            OnPropertyChanged("FriendlyValue");
            OnPropertyChanged("SelectedIndex");
            base.OnValueChanged();
        }
    }

    public class EnumSetting<T> : EnumSetting where T : struct
    {
        private T value;
        public T Value 
        {
            get { return value; }
            set {
                if (IsReadOnly) return;
                this.value = value;
                OnValueChanged();
            }
        }
        public T DefaultValue { get; private set; }

        public override string StringValue
        {
            get { return value.ToString(); }
            set
            {
                if (IsReadOnly) return;
                T newValue;
                if(Enum.TryParse<T>(value, out newValue))
                {
                    Value = newValue;
                }
            }
        }

        public EnumSetting(string name, T value) : base(name)
        {
            if (!typeof(T).IsEnum) throw new NotSupportedException("The given datatype is not an enumeration!");
            Type = Types.Enum;
            DefaultValue = Value = value;
            StringOptions = Enum.GetNames(typeof(T));
        }

        public override bool HasDefaultValue
        {
            get { return DefaultValue.Equals(value); }
        }

        public override void ResetValue()
        {
            if (IsReadOnly) return;
            Value = DefaultValue;
        }
    }

    public class SettingSetter
    {
        public List<Setting> Settings { get; private set; }
        public void SetReadOnly(bool value)
        {
            foreach (Setting setting in Settings)
                setting.IsReadOnly = value;
        }

        public void SetHidden(bool value)
        {
            foreach (Setting setting in Settings)
                setting.IsHidden = value;
        }

        public SettingSetter()
        {
            Settings = new List<Setting>();
        }
    }

    public class SettingGroup : INotifyPropertyChanged
    {
        private bool isReadOnly;
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
            set
            {
                isReadOnly = value;
                OnPropertyChanged("IsReadOnly");
            }
        }

        private bool isHidden;
        public bool IsHidden
        {
            get { return isHidden || !AnyItemsVisible; }
            set
            {
                isHidden = value;
                OnPropertyChanged("IsHidden");
            }
        }

        private bool hasDefaultValues;
        public bool HasDefaultValues 
        {
            get { return hasDefaultValues; }
            private set
            {
                hasDefaultValues = value;
                OnPropertyChanged("HasDefaultValues");
            }
        }
        public string Name { get; private set; }
        public string Footer { get; set; }
        public ObservableCollection<Setting> Settings { get; private set; }
        public SettingGroup(string name)
        {
            Name = name;
            Settings = new ObservableCollection<Setting>();
            Settings.CollectionChanged += Settings_CollectionChanged;
            HasDefaultValues = true;
        }

        void Settings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (Setting s in e.NewItems)
            {
                s.PropertyChanged += Setting_PropertyChanged;
                s.ValueChanged += Setting_ValueChanged;                
                s.Group = this;
                if (!s.HasDefaultValue) HasDefaultValues = false;
            }
            if(!isHidden) OnPropertyChanged("IsHidden");
        }

        public bool AnyItemsVisible
        {
            get
            {
                foreach (Setting s in Settings)
                {
                    if (!s.IsHidden && s.IsAvailable)
                        return true;
                }
                return false;
            }
        }

        public event EventHandler ValueChanged;
        void Setting_ValueChanged(object sender, EventArgs e)
        {
            if (!SuppressValueChanged && ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }

        void Setting_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool hdv = true;
            switch (e.PropertyName)
            {
                case "IsAvailable":
                    if (!isHidden) OnPropertyChanged("IsHidden");
                    break;
            }
            foreach (Setting s in Settings)
            {
                if (!s.HasDefaultValue)
                {
                    hdv = false;
                    break;
                }
            }
            HasDefaultValues = hdv;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        private string friendlyName = null;
        public string FriendlyName
        {
            get
            {
                if (friendlyName == null) return Name;
                else return friendlyName;
            }
            set
            {
                friendlyName = value;
            }
        }

        private bool SuppressValueChanged = false;
        public void StoreAllValues()
        {
            foreach (Setting s in Settings) s.StoreValue();
        }

        public void RestoreAllValues()
        {
            SuppressValueChanged = true;
            foreach (Setting s in Settings) s.RestoreValue();
            SuppressValueChanged = false;
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }

        public void ResetToDefault()
        {
            SuppressValueChanged = true;
            foreach (Setting s in Settings) s.ResetValue();
            SuppressValueChanged = false;
            if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
        }
    }

    public class SettingManager
    {
        private bool isReadOnly;
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
            set
            {
                isReadOnly = value;
                foreach (SettingGroup sg in SettingGroups)
                {
                    sg.IsReadOnly = isReadOnly;
                }
            }
        }

        public ObservableCollection<SettingGroup> SettingGroups { get; private set; }
        public SettingManager()
        {
            SettingGroups = new ObservableCollection<SettingGroup>();
            SettingGroups.CollectionChanged += SettingGroups_CollectionChanged;
        }

        void SettingGroups_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (SettingGroup sg in e.NewItems)
            {
                sg.IsReadOnly = isReadOnly;
            }
        }

        public Dictionary<string, Setting> GetSettingsDictionary()
        {
            Dictionary<string, Setting> settings = new Dictionary<string, Setting>();
            foreach(SettingGroup settingGroup in SettingGroups)
                foreach (Setting setting in settingGroup.Settings)
                {
                    settings.Add(settingGroup.Name + '.' + setting.Name, setting);
                }
            return settings;
        }

        public string ToString(IEnumerable<SettingGroup> settingGroups = null)
        {
            string text = "";
            if (settingGroups == null) settingGroups = SettingGroups;
            foreach (SettingGroup SG in settingGroups)
            {
                text += string.Format("[{0}]\r\n", SG.Name);
                foreach (Setting S in SG.Settings)
                {
                    text += string.Format("{0}={1}\r\n", S.Name, S.StringValue);
                }
                text += "\r\n";
            }
            return text;
        }

        public bool Save(string path)
        {
            try
            {
                File.WriteAllText(path, ToString());
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public void FromString(string text)
        {
            string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, Setting> settings = GetSettingsDictionary();
            string group = "", setting, value;
            string[] items;
            foreach (string line in lines)
            {
                try
                {
                    if (line.Length == 0) continue;
                    if (line[0] == '[')
                    {
                        group = line.Substring(1, line.Length - 2);
                    }
                    else
                    {
                        items = line.Split('=');
                        setting = group + '.' + items[0];
                        value = items[1];
                        if (settings.ContainsKey(setting))
                        {
                            try
                            {
                                settings[setting].StringValue = value;
                            }
                            catch
                            {
                                settings[setting].ResetValue();
                            }
                        }
                    }
                }
                catch { }
            }
        }

        public bool Load(string path)
        {
            try
            {
                FromString(File.ReadAllText(path));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    #region Other settings
    public class DenavitHartenbergSetting : Setting
    {
        public event EventHandler ActiveJointChanged;
        private Joint activeJoint;
        public Joint ActiveJoint
        {
            get
            {
                return activeJoint;
            }
            set
            {
                if (Joints.Contains(value) || value == null)
                {
                    activeJoint = value;
                    OnPropertyChanged("ActiveJoint");
                    if (ActiveJointChanged != null) ActiveJointChanged(this, EventArgs.Empty);
                }
            }
        }

        public class Joint : INotifyPropertyChanged
        {
            public float Maximum
            {
                get
                {
                    switch (parameter)
                    {
                        case Parameters.Q:
                        case Parameters.Alpha:
                            return (float)Math.PI;
                        case Parameters.D:
                        case Parameters.A:
                            return 1;
                        default:
                            return 0;
                    }
                }
            }
            public float Minimum
            {
                get
                {
                    switch (parameter)
                    {
                        case Parameters.Q:
                        case Parameters.Alpha:
                            return -(float)Math.PI;
                        case Parameters.D:
                        case Parameters.A:
                            return -1;
                        default:
                            return 0;
                    }
                }
            }

            public string[] StringParameterValues
            {
                get
                {
                    return new string[] { "Q", "D", "A", "Alpha" };
                }
            }
            public string StringParameter
            {
                get
                {
                    return Parameter.ToString();
                }
                set
                {
                    Parameters parameter;
                    if (Enum.TryParse<Parameters>(value, out parameter))
                        Parameter = parameter;
                }
            }
            private int number;
            public int Number { get { return number; } set { number = value; OnPropertyChanged("Number"); } }
            public enum Parameters : byte { Q, D, A, Alpha };
            private Parameters parameter;
            public Parameters Parameter 
            { 
                get 
                { 
                    return parameter; 
                } 
                set 
                { 
                    parameter = value; 
                    OnPropertyChanged("Parameter"); 
                    OnPropertyChanged("StringParameter"); 
                    OnPropertyChanged("X"); 
                    OnPropertyChanged("Minimum"); 
                    OnPropertyChanged("Maximum");
                    if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                } 
            }
            private float q, d, a, alpha;
            public float Q { get { return q; } set { q = value; OnParameterChanged(Parameters.Q); } }
            public float D { get { return d; } set { d = value; OnParameterChanged(Parameters.D); } }
            public float A { get { return a; } set { a = value; OnParameterChanged(Parameters.A); } }
            public float Alpha { get { return alpha; } set { alpha = value; OnParameterChanged(Parameters.Alpha); } }
            public float X 
            { 
                get 
                {
                    switch (parameter)
                    {
                        case Parameters.Q:
                            return Q;
                        case Parameters.D:
                            return D;
                        case Parameters.A:
                            return A;
                        case Parameters.Alpha:
                            return Alpha;
                        default:
                            return 0;
                    }
                }
                set
                {
                    switch (parameter)
                    {
                        case Parameters.Q:
                            Q = value;
                            break;
                        case Parameters.D:
                            D = value;
                            break;
                        case Parameters.A:
                            A = value;
                            break;
                        case Parameters.Alpha:
                            Alpha = value;
                            break;
                    }
                }
            }
            public event EventHandler ValueChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            private void OnParameterChanged(Parameters parameter)
            {
                if (ValueChanged != null) ValueChanged(this, EventArgs.Empty);
                if (PropertyChanged != null)
                {
                    OnPropertyChanged(parameter.ToString());
                    if (Parameter == parameter) OnPropertyChanged("X");
                }
            }

            private void OnPropertyChanged(string name)
            {
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(name));
            }

            public static Joint Parse(string s)
            {
                string[] items = s.Split(';');
                return new Joint()
                {
                    Q = float.Parse(items[0], Culture),
                    D = float.Parse(items[1], Culture),
                    A = float.Parse(items[2], Culture),
                    Alpha = float.Parse(items[3], Culture),
                    Parameter = (Parameters)Enum.Parse(typeof(Parameters), items[4])                    
                };
            }

            public override string ToString()
            {
                return string.Format(Culture, "{0};{1};{2};{3};{4}", Q, D, A, Alpha, Parameter);
            }
        }

        public int JointCount
        {
            get
            {
                return Joints.Count;
            }
        }

        public float[] ToJointArray()
        {
            float[] values = new float[Joints.Count * 4];
            int id;
            for (int i = 0; i < Joints.Count; i++)
            {
                id = i * 4;
                values[id] = Joints[i].Q;
                values[id + 1] = Joints[i].D;
                values[id + 2] = Joints[i].A;
                values[id + 3] = Joints[i].Alpha;
            }
            return values;
        }

        public byte[] ToParameterArray()
        {
            byte[] values = new byte[Joints.Count * 4];
            for (int i = 0; i < Joints.Count; i++)
            {
                values[i] = (byte)Joints[i].Parameter;
            }
            return values;
        }

        public ObservableCollection<Joint> Joints { get; private set; }
        public DenavitHartenbergSetting(string name)
            : base(name)
        {
            Joints = new ObservableCollection<Joint>();
            Joints.CollectionChanged += Items_CollectionChanged;
        }

        void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                foreach (Joint item in e.NewItems)
                {
                    item.ValueChanged += (object o, EventArgs ea) => { OnValueChanged(); };
                }
            if (e.OldItems != null)
            {
                foreach (Joint item in e.OldItems)
                {
                    if (item == ActiveJoint) ActiveJoint = null;
                }
            }
            int i = 0;
            foreach(Joint joint in Joints)
            {
                joint.Number = i++;
            }
            OnValueChanged();
        }

        public override string StringValue
        {
            get
            {
                return string.Join<Joint>("|", Joints);
            }
            set
            {
                try
                {
                    Joints.Clear();
                    string[] items = value.Split('|');
                    foreach (string item in items) Joints.Add(Joint.Parse(item));
                }
                catch { }
            }
        }

        public override bool HasDefaultValue
        {
            get { return Joints.Count == 0; }
        }

        public override void ResetValue()
        {
            Joints.Clear();
        }
    }
    #endregion
}
