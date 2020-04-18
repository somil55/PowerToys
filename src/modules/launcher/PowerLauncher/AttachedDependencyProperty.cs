using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace PowerLauncher
{
    public class AttachedDependencyProperty : DependencyObject
    {
        public static readonly DependencyProperty SecurityIdProperty =
        DependencyProperty.RegisterAttached("SecurityId",
            typeof(Visibility),
            typeof(AttachedDependencyProperty),
            new PropertyMetadata(OnChanged));

        public static Visibility GetSecurityId(DependencyObject d)
        {
            return (Visibility)d.GetValue(SecurityIdProperty);
        }

        public static void SetSecurityId(DependencyObject d, Visibility value)
        {
            d.SetValue(SecurityIdProperty, value);
        }

        private static void OnChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            SetSecurityId(obj, (Visibility)args.NewValue);
            Control control = (Control)obj;

            TimeSpan duration = new TimeSpan(0, 0, 1);

            DoubleAnimation fadeInAnimation = new DoubleAnimation()
            { From = 0.0, To = 1.0, Duration = new Duration(duration) };

            DoubleAnimation fadeOutAnimation = new DoubleAnimation()
            { From = 1.0, To = 0.0, Duration = new Duration(duration) };

            if((Visibility)args.NewValue == Visibility.Visible)
            {
                Debug.WriteLine(DateTime.Now);
                control.Visibility = (Visibility)args.NewValue;
                Storyboard storyboard = new Storyboard();
                Storyboard.SetTargetName(fadeInAnimation, control.Name); ;
                Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(Control.OpacityProperty));
                storyboard.Children.Add(fadeInAnimation);
                storyboard.Begin(control);
                Debug.WriteLine(DateTime.Now);
            }

            if ((Visibility)args.NewValue == Visibility.Collapsed)
            {
                Debug.WriteLine(DateTime.Now);
                Storyboard storyboard = new Storyboard();
                Storyboard.SetTargetName(fadeOutAnimation, control.Name);
                Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(Control.OpacityProperty));
                storyboard.Children.Add(fadeOutAnimation);
                storyboard.Completed += delegate {
                    control.Visibility = (Visibility)args.NewValue;
                    Debug.WriteLine(DateTime.Now);
                };
                storyboard.Begin(control);
                
            }
        }
    }
}
