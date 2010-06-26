using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Sirius
{
    public class WheelObserver
    {
        public UIElement Target { get; private set; }

        public WheelObserver(UIElement target)
        {
            Target = target;
            Target.MouseWheel += new MouseWheelEventHandler(Target_MouseWheel);
        }

        private void Target_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Handled) return;

            var sv = GetChildVisual<ScrollViewer>(Target);
            if (sv == null) return;

            double of;
            if (Target is ListBox)
                of = sv.VerticalOffset - e.Delta / 40;
            else
                of = sv.VerticalOffset - e.Delta / 3;
            if (of < 0) of = 0;
            if (of > sv.ScrollableHeight) of = sv.ScrollableHeight;
            sv.ScrollToVerticalOffset(of);
            e.Handled = true;
        }

        private static T GetChildVisual<T>(DependencyObject dobj) where T : DependencyObject
        {
            if (dobj is T) return dobj as T;
            int count = VisualTreeHelper.GetChildrenCount(dobj);
            for (int i = 0; i < count; i++)
            {
                var ret = GetChildVisual<T>(VisualTreeHelper.GetChild(dobj, i));
                if (ret != null) return ret;
            }
            return null;
        }
    }
}
