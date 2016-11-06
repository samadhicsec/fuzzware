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

namespace Fuzzware.Fuzzsaw.Common.View
{
    /// <summary>
    /// Interaction logic for OnOffControlView.xaml
    /// </summary>
    public partial class OnOffControlView : UserControl
    {
        DoubleAnimation m_oOffAnimation;
        DoubleAnimation m_oGlowOff;

        DoubleAnimation m_oOnAnimation;
        DoubleAnimation m_oGlowOn;

        Border m_bdOn;
        Border m_bdOff;
        Rectangle m_rectCover;

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (true == tbToggler.IsChecked)
                tbToggler.IsChecked = false;
            else
                tbToggler.IsChecked = true;
        }

        // There is a warning against using BitmapEffect which is going to become obsolete
        #pragma warning disable 618
        public OnOffControlView()
        {
            InitializeComponent();
        }

        // Ever since I changed to Collasping the work area instead of using its z-index to hide
        // the controls, the OnOffControl has been a pain.  Basically the tbToggler.Template.FindName
        // function has been returning null if I call LoadControl to early, so I hooked the Grid load
        // event as by then the function returns objects.  It's not a great solution, but I don't know
        // exactly when the template is getting applied.  It also means on some screens when you first
        // go to them you see the On animation.

        //private void UserControl_Loaded(object sender, RoutedEventArgs e)
        //{
        //    LoadControl();
        //}

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    LoadControl();
        //}

        //private void tbToggler_Initialized(object sender, EventArgs e)
        //{
        //    LoadControl();
        //}

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            LoadControl();
        }

        private void LoadControl()
        {
            if (null == m_bdOn)
                m_bdOn = tbToggler.Template.FindName("bdOn", tbToggler) as Border;
            if (null == m_bdOff)
                m_bdOff = tbToggler.Template.FindName("bdOff", tbToggler) as Border;
            if (null == m_rectCover)
                m_rectCover = tbToggler.Template.FindName("rectCover", tbToggler) as Rectangle;
            if ((null == m_bdOn) || (null == m_bdOff) || (null == m_rectCover))
                return;
            
            // Create the glow effect
            OuterGlowBitmapEffect oGlow = new OuterGlowBitmapEffect();
            oGlow.GlowColor = Colors.LightGreen;
            oGlow.GlowSize = 0;
            m_bdOn.BitmapEffect = oGlow;

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

            // Its default is Off, so if its On
            if (true == tbToggler.IsChecked)
                OnAnimation(null, null);
        }

        private void OnAnimation(object sender, RoutedEventArgs e)
        {
            if ((null == m_bdOn) || (null == m_bdOff) || (null == m_rectCover))
                return;

            // Animate to the on position
            m_rectCover.BeginAnimation(Canvas.LeftProperty, m_oOnAnimation);
            m_bdOn.BitmapEffect.BeginAnimation(OuterGlowBitmapEffect.GlowSizeProperty, m_oGlowOn);
        }

        private void OffAnimation(object sender, RoutedEventArgs e)
        {
            if ((null == m_bdOn) || (null == m_bdOff) || (null == m_rectCover))
                return;

            // Animate to the off position
            m_rectCover.BeginAnimation(Canvas.LeftProperty, m_oOffAnimation);
            m_bdOn.BitmapEffect.BeginAnimation(OuterGlowBitmapEffect.GlowSizeProperty, m_oGlowOff);
        }

        void Animation_Completed(object sender, EventArgs e)
        {
            double dEndValue = (double)m_rectCover.GetValue(Canvas.LeftProperty);
            m_rectCover.BeginAnimation(Canvas.LeftProperty, null);
            m_rectCover.SetValue(Canvas.LeftProperty, dEndValue);

            double dGlowSize = (m_bdOn.BitmapEffect as OuterGlowBitmapEffect).GlowSize;
            m_bdOn.BeginAnimation(OuterGlowBitmapEffect.GlowSizeProperty, null);
            (m_bdOn.BitmapEffect as OuterGlowBitmapEffect).GlowSize = dGlowSize;
        }

        #pragma warning restore 618
    }
}
