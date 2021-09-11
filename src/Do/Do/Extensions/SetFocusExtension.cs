using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Accessibility;

namespace Do.Extensions
{
    public static class SetFocusExtension
    {
        public static DependencyPropertyChangedEventHandler SetFocus(this FrameworkElement elem)
        {
            void _handler(object o, DependencyPropertyChangedEventArgs e)
            {
                if ((bool) e.NewValue)
                {
                    elem.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
                    {
                        Keyboard.Focus(elem);
                        elem.Focus();
                    }));
                }
            }

            return _handler;
        }
    }
}