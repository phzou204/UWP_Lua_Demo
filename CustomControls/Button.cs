using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CustomControls
{
    public class MyButton : Button
    {
        public MyButton()
        {
            DefaultStyleKey = typeof(MyButton);
        }
        #region CornerRadius
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(MyButton), null);

        #endregion
        #region LeftImageSource
        public ImageSource LeftImageSource
        {
            get { return (ImageSource)GetValue(LeftImageSourceProperty); }
            set { SetValue(LeftImageSourceProperty, value); }
        }
        public static readonly DependencyProperty LeftImageSourceProperty =
            DependencyProperty.Register("LeftImageSource", typeof(ImageSource), typeof(MyButton), null);

        #endregion
        #region RightImageSource
        public ImageSource RightImageSource
        {
            get { return (ImageSource)GetValue(RightImageSourceProperty); }
            set { SetValue(RightImageSourceProperty, value); }
        }
        public static readonly DependencyProperty RightImageSourceProperty =
            DependencyProperty.Register("RightImageSource", typeof(ImageSource), typeof(MyButton), null);

        #endregion
        #region LeftAndRightImageReference
        private Image _leftImage;
        public Image LeftImage
        {
            get
            {
                if (_leftImage != null)
                {
                    return _leftImage;
                }
                else
                {
                    return null;
                }
            }
            private set
            {
                _leftImage = value;
            }
        }
        private Image _rightImage;
        public Image RightImage
        {
            get
            {
                if (_rightImage != null)
                {
                    return _rightImage;
                }
                else
                {
                    return null;
                }
            }
            private set
            {
                _rightImage = value;
            }
        }
        #endregion


        #region LeftImageWidthProperty
        public double LeftImageWidth
        {
            get { return (double)GetValue(LeftImageWidthProperty); }
            set { SetValue(LeftImageWidthProperty, value); }
        }
        public static readonly DependencyProperty LeftImageWidthProperty =
            DependencyProperty.Register("LeftImageWidth", typeof(double), typeof(MyButton), new PropertyMetadata(double.NaN));
        #endregion
        #region LeftImageHeightProperty
        public double LeftImageHeight
        {
            get { return (double)GetValue(LeftImageHeightProperty); }
            set { SetValue(LeftImageHeightProperty, value); }
        }
        public static readonly DependencyProperty LeftImageHeightProperty =
            DependencyProperty.Register("LeftImageHeight", typeof(double), typeof(MyButton), new PropertyMetadata(double.NaN));
        #endregion

        #region RightImageWidthProperty
        public double RightImageWidth
        {
            get { return (double)GetValue(RightImageWidthProperty); }
            set { SetValue(RightImageWidthProperty, value); }
        }
        public static readonly DependencyProperty RightImageWidthProperty =
            DependencyProperty.Register("RightImageWidth", typeof(double), typeof(MyButton), new PropertyMetadata(double.NaN));
        #endregion
        #region RightImageHeightProperty
        public double RightImageHeight
        {
            get { return (double)GetValue(RightImageHeightProperty); }
            set { SetValue(RightImageHeightProperty, value); }
        }
        public static readonly DependencyProperty RightImageHeightProperty =
            DependencyProperty.Register("RightImageHeight", typeof(double), typeof(MyButton), new PropertyMetadata(double.NaN));
        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.LeftImage = GetTemplateChild("LeftImage") as Image;
            this.RightImage = GetTemplateChild("RightImage") as Image;
        }
    }
}
