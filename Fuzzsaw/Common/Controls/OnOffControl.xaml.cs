using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Fuzzware.Fuzzsaw.Common.Controls
{
    /// <summary>
    /// Interaction logic for OnOffControl.xaml
    /// </summary>
    public partial class OnOffControl : UserControl
    {
        DoubleAnimation m_oOffAnimation;
        DoubleAnimation m_oGlowOff;

        DoubleAnimation m_oOnAnimation;
        DoubleAnimation m_oGlowOn;

        public static DependencyProperty OnProperty;

        static OnOffControl()
        {
            OnProperty = DependencyProperty.Register("On", typeof(bool), typeof(OnOffControl), 
                new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnValueChanged)));
        }

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            OnOffControl control = (OnOffControl)obj;

            if ((bool)args.NewValue)
                control.OnAnimation();
            else
                control.OffAnimation();
        }


        public bool On
        {
            get { return (bool)GetValue(OnProperty); }
            set { SetValue(OnProperty, value); }
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            On = !On;
        }

        // There is a warning against using BitmapEffect which is going to become obsolete
        #pragma warning disable 618
        public OnOffControl()
        {
            InitializeComponent();
            
            // Create the glow effect
            OuterGlowBitmapEffect oGlow = new OuterGlowBitmapEffect();
            oGlow.GlowColor = Colors.LightGreen;
            oGlow.GlowSize = 0;
            bdOn.BitmapEffect = oGlow;

            TimeSpan oTimeSpan = new TimeSpan(0, 0, 0, 0, 500);
            // Create the On animations
            m_oOnAnimation = new DoubleAnimation();
            m_oOnAnimation.From = 0;
            m_oOnAnimation.To = 30;
            m_oOnAnimation.Duration = oTimeSpan;
            m_oOnAnimation.Completed += new EventHandler(Animation_Completed);
            m_oGlowOn = new DoubleAnimation();
            m_oGlowOn.From = 0;
            m_oGlowOn.To = 20;
            m_oGlowOn.Duration = oTimeSpan;

            // Create the Off animations
            m_oOffAnimation = new DoubleAnimation();
            m_oOffAnimation.From = 30;
            m_oOffAnimation.To = 0;
            m_oOffAnimation.Duration = oTimeSpan;
            m_oOffAnimation.Completed += new EventHandler(Animation_Completed);
            m_oGlowOff = new DoubleAnimation();
            m_oGlowOff.From = 20;
            m_oGlowOff.To = 0;
            m_oGlowOff.Duration = oTimeSpan;
        }

        private void OnAnimation()
        {
            // Animate to the on position
            rectCover.BeginAnimation(Canvas.LeftProperty, m_oOnAnimation);
            bdOn.BitmapEffect.BeginAnimation(OuterGlowBitmapEffect.GlowSizeProperty, m_oGlowOn);
        }

        private void OffAnimation()
        {
            // Animate to the off position
            rectCover.BeginAnimation(Canvas.LeftProperty, m_oOffAnimation);
            bdOn.BitmapEffect.BeginAnimation(OuterGlowBitmapEffect.GlowSizeProperty, m_oGlowOff);
        }

        void Animation_Completed(object sender, EventArgs e)
        {
            double dEndValue = (double)rectCover.GetValue(Canvas.LeftProperty);
            rectCover.BeginAnimation(Canvas.LeftProperty, null);
            rectCover.SetValue(Canvas.LeftProperty, dEndValue);

            double dGlowSize = (bdOn.BitmapEffect as OuterGlowBitmapEffect).GlowSize;
            bdOn.BeginAnimation(OuterGlowBitmapEffect.GlowSizeProperty, null);
            (bdOn.BitmapEffect as OuterGlowBitmapEffect).GlowSize = dGlowSize;
        }
        #pragma warning restore 618
    }
}
