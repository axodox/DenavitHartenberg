using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Green.Settings.UI
{
    public class FloatBox : TextBox
    {
        int oldCaretPos;
        string oldText;
        //bool skipNext;

        public FloatBox()
            : base()
        {
            Text = "0";
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (TempValue > Maximum) TempValue = Maximum;
            if (TempValue < Minimum) TempValue = Minimum;
            Value = TempValue;
            //skipNext = true;
            Text = TempValue.ToString();
            OnValueChanged();
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            oldCaretPos = CaretIndex;
            oldText = Text;            
            base.OnKeyDown(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            //if (skipNext)
            //{
            //    skipNext = false;
            //    return;
            //}
            float val;
            if (Text == "" || Text == "-" || float.TryParse(Text, out val))
            {
                oldText = Text;
                oldCaretPos = CaretIndex;
            }
            else
            {
                //skipNext = true;
                Text = oldText;
                CaretIndex = oldCaretPos;
            }
            base.OnTextChanged(e);

            if (float.TryParse(Text, out val))
            {
                TempValue = val;
            }
            
        }        

        float TempValue;
        public float Value
        {
            get { return (float)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
    "Value", typeof(float), typeof(FloatBox), new PropertyMetadata(0f, ValueChangedCallback));

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FloatBox fb = obj as FloatBox;
            fb.Text = e.NewValue.ToString();
            fb.TempValue = (float)e.NewValue;
        }

        public float Maximum
        {
            get { return (float)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
    "Maximum", typeof(float), typeof(FloatBox), new PropertyMetadata(float.PositiveInfinity));

        public float Minimum
        {
            get { return (float)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
    "Minimum", typeof(float), typeof(FloatBox), new PropertyMetadata(float.NegativeInfinity));

        void OnValueChanged()
        {            
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        public event EventHandler ValueChanged;
    }
}