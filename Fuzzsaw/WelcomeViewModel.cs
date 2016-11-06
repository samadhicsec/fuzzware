using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Fuzzware.Fuzzsaw.Common;

namespace Fuzzware.Fuzzsaw
{
    public class WelcomeViewModel : ViewModelBase
    {
        #region Dependency Properties

        static readonly DependencyProperty WelcomeMessageProperty = DependencyProperty.Register("WelcomeMessage", typeof(string), typeof(WelcomeViewModel));
        /// <summary>
        /// The welcome message on initialisation
        /// </summary>
        public string WelcomeMessage
        {
            get { return (string)GetValue(WelcomeMessageProperty); }
            set { SetValue(WelcomeMessageProperty, value); }
        }

        #endregion

        public WelcomeViewModel()
        {
            // Set up welcome message
            string[] Welcomes = { 
                                    "\"Find an 0-day.  Save the World.\"",
                                    "\"Watch out here comes the fuzz!\"",
                                    "\"It's a beautiful 0-day!\"",
                                    "\"Fuzz the Planet\"",
                                    "\"Size Matters\"",
                                    "\"Where do all the a's come from?\"",
                                    "\"Never send a human to do a machines job\"",
                                    "\"Find the bugs, before they find you...\""
                                };
            System.Security.Cryptography.RNGCryptoServiceProvider oRandom = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] indexbytes = new byte[1];
            oRandom.GetBytes(indexbytes);
            int index = (int)(indexbytes[0] % Welcomes.Length);
            WelcomeMessage = Welcomes[index];
        }
    }
}
