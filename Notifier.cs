using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class Notifier
    {
        protected string _changingProperty;
        protected object _previousValue;

        public delegate void NotifyChanged();

        [System.Xml.Serialization.XmlIgnore]
        public NotifyChanged notifyChanged { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        public bool isInitialised { get; set; }

        protected System.Windows.Forms.Timer watchTimer;

        public Notifier()
        {
            watchTimer = new System.Windows.Forms.Timer();
            watchTimer.Interval = 400;
            watchTimer.Tick += watchTimer_Tick;

            isInitialised = false;
        }

        void watchTimer_Tick(object sender, EventArgs e)
        {
            watchTimer.Stop();

            if (notifyChanged != null)
            {
                notifyChanged();
            }
        }

        public void propertyChanging(string variableName = "")
        {
            if (!isInitialised)
            {
                return;
            }

            watchTimer.Stop();

            watchTimer.Start();
        }
    }
}
