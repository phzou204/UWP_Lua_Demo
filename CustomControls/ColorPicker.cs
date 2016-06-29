using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CustomControls
{
    public class ColorPicker:Control
    {
        public ColorPicker()
        {
            DefaultStyleKey = typeof(ColorPicker);
        }
        
        #region ColorProperty
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.Black, OnColorChanged));

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)d;

            Color oldColor = (Color)e.OldValue;
            Color newColor = (Color)e.NewValue;
            colorPicker.Red = newColor.R;
            colorPicker.Green = newColor.G;
            colorPicker.Blue = newColor.B;

            //colorPicker.previousColor = oldColor;
            //colorPicker.OnColorChanged(oldColor, newColor);
        }
        #endregion

        private static void OnColorRGBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)d;
            Color color = colorPicker.Color;
            if (e.Property == RedProperty)
                color.R = (byte)(double)e.NewValue;
            else if (e.Property == GreenProperty)
                color.G = (byte)(double)e.NewValue;
            else if (e.Property == BlueProperty)
                color.B = (byte)(double)e.NewValue;

            colorPicker.Color = color;
        }

        #region RedProperty
        public double Red
        {
            get { return (double)GetValue(RedProperty); }
            set { SetValue(RedProperty, value); }
        }
        public static readonly DependencyProperty RedProperty =
            DependencyProperty.Register("Red", typeof(double), typeof(ColorPicker), new PropertyMetadata(0, OnColorRGBChanged));

        #endregion

        #region GreenProperty
        public double Green
        {
            get { return (double)GetValue(GreenProperty); }
            set { SetValue(GreenProperty, value); }
        }
        public static readonly DependencyProperty GreenProperty =
            DependencyProperty.Register("Green", typeof(double), typeof(ColorPicker), new PropertyMetadata(0, OnColorRGBChanged));

        #endregion

        #region  BlueProperty
        public double Blue
        {
            get { return (double)GetValue(BlueProperty); }
            set { SetValue(BlueProperty, value); }
        }
        public static readonly DependencyProperty BlueProperty =
            DependencyProperty.Register("Blue", typeof(double), typeof(ColorPicker), new PropertyMetadata(0, OnColorRGBChanged));

        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RangeBase slider = GetTemplateChild("PART_RedSlider") as RangeBase;
            if (slider != null)
            {
                Binding binding = new Binding();
                binding.Path = new PropertyPath("Red");
                binding.Source = this;
                binding.Mode = BindingMode.TwoWay;
                slider.SetBinding(RangeBase.ValueProperty, binding);
            }
            slider = GetTemplateChild("PART_GreenSlider") as RangeBase;
            if (slider != null)
            {
                Binding binding = new Binding();
                binding.Path = new PropertyPath("Green");
                binding.Source = this;
                binding.Mode = BindingMode.TwoWay;
                slider.SetBinding(RangeBase.ValueProperty, binding);
            }
            slider = GetTemplateChild("PART_BlueSlider") as RangeBase;
            if (slider != null)
            {
                Binding binding = new Binding();
                binding.Path = new PropertyPath("Blue");
                binding.Source = this;
                binding.Mode = BindingMode.TwoWay;
                slider.SetBinding(RangeBase.ValueProperty, binding);
            }

            SolidColorBrush brush = GetTemplateChild("PART_PreviewBrush") as SolidColorBrush;
            if (brush != null)
            {
                Binding binding = new Binding();
                binding.Path = new PropertyPath("Color");
                binding.Source = brush;
                binding.Mode = BindingMode.TwoWay;
                this.SetBinding(ColorPicker.ColorProperty, binding);
            }
        }
    }
}
