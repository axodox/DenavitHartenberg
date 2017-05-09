using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Green.Settings.UI
{
    public class IntBox : TextBox
    {
        int oldCaretPos;
        string oldText;
        bool skipNext;

        public IntBox()
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
            skipNext = true;
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
            if (skipNext)
            {
                skipNext = false;
                return;
            }
            int val;
            if (Text == "" || Text == "-" || int.TryParse(Text, out val))
            {
                oldText = Text;
                oldCaretPos = CaretIndex;
            }
            else
            {
                skipNext = true;
                Text = oldText;
                CaretIndex = oldCaretPos;
            }
            base.OnTextChanged(e);

            if (int.TryParse(Text, out val))
            {
                TempValue = val;
            }
        }

        int TempValue;
        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
    "Value", typeof(int), typeof(IntBox), new PropertyMetadata(0, ValueChangedCallback));

        private static void ValueChangedCallback(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            IntBox ib = obj as IntBox;
            ib.Text = e.NewValue.ToString();
            ib.TempValue = (int)e.NewValue;
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
    "Maximum", typeof(int), typeof(IntBox), new PropertyMetadata(int.MaxValue));
        
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
    "Minimum", typeof(int), typeof(IntBox), new PropertyMetadata(int.MinValue));

        void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        public event EventHandler ValueChanged;
    }
}