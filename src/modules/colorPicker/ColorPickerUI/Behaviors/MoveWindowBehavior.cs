﻿using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media.Animation;

namespace ColorPicker.Behaviors
{
    public class MoveWindowBehavior : Behavior<Window>
    {
        public static DependencyProperty LeftProperty = DependencyProperty.Register("Left", typeof(double), typeof(MoveWindowBehavior), new PropertyMetadata(new PropertyChangedCallback(LeftPropertyChanged)));

        private static void LeftPropertyChanged(DependencyObject d,
               DependencyPropertyChangedEventArgs e)
        {
            var sender = ((MoveWindowBehavior)d).AssociatedObject;
            var move = new DoubleAnimation(sender.Left, (double)e.NewValue, new Duration(TimeSpan.FromMilliseconds(150)), FillBehavior.Stop);
            move.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
            sender.BeginAnimation(Window.LeftProperty, move, HandoffBehavior.SnapshotAndReplace);
        }

        public static DependencyProperty TopProperty = DependencyProperty.Register("Top", typeof(double), typeof(MoveWindowBehavior), new PropertyMetadata(new PropertyChangedCallback(TopPropertyChanged)));

        private static void TopPropertyChanged(DependencyObject d,
               DependencyPropertyChangedEventArgs e)
        {
            var sender = ((MoveWindowBehavior)d).AssociatedObject;
            var move = new DoubleAnimation(sender.Top, (double)e.NewValue, new Duration(TimeSpan.FromMilliseconds(150)), FillBehavior.Stop);
            move.EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut };
            sender.BeginAnimation(Window.TopProperty, move, HandoffBehavior.SnapshotAndReplace);
        }

        public double Left
        {
            get
            {
                return (double)GetValue(LeftProperty);
            }
            set
            {
                SetValue(LeftProperty, value);
            }
        }

        public double Top
        {
            get
            {
                return (double)GetValue(TopProperty);
            }
            set
            {
                SetValue(TopProperty, value);
            }
        }
    }
}
