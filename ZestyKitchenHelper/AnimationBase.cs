﻿using System;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Forms;

namespace ZestyKitchenHelper
{
    public static class ViewExtensions
    {
        static short flightIndexer = 0;
        static double timer = 0;
        public static Task<bool> ColorChange(this VisualElement element, Color fromColor, Color toColor, Action<Color> callBack, uint length = 250, Easing easing = null)
        {
            Func<double, Color> transform = (t) =>
                Color.FromRgba(fromColor.R + t * (toColor.R - fromColor.R),
                                fromColor.G + t * (toColor.G - fromColor.G),
                                fromColor.B + t * (toColor.B - fromColor.B),
                                fromColor.A + t * (toColor.A - fromColor.A));
            return CallAnimation(element, "ColorAnimation", transform, callBack, length, easing);
        }

        public static Task<bool> QuadraticFlight(this VisualElement element, double power, double angle, double acceleration, double movementMultiplier, Action<Point> callBack,
            uint length = 250, Easing easing = null)
        {
            flightIndexer++;
            var rad = angle / 180 * Math.PI;
            Func<double, Point> transform = t => 
                new Point(Math.Cos(rad) * power * t * movementMultiplier, 
                -(0.5 * acceleration * t * t + Math.Sin(rad) * power * t) * movementMultiplier);
            return CallAnimation<Point>(element, "QuadraticAnimation" + flightIndexer.ToString(), transform, callBack, length, easing);
        }

        public static Task<bool> LinearInterpolator(this VisualElement element, double toValue, uint duration, Action<double> callback, Easing easing = null)
        {
            Func<double, double> transform = t => t / duration * toValue * 100;
            return CallAnimation(element, "LerpAnimation", transform, callback, duration, easing);
        }

        public static Task<bool> QuadraticInterpolator(this VisualElement element, double maxValue, uint duration, Action<double> callback, Easing easing = null)
        {
            var a = -4 * maxValue / (duration * duration);
            Func<double, double> transform = t => { var x = t * duration; return a * x * (x - duration) + 1; };
            return CallAnimation(element, "QuadraticLerpAnimation", transform, callback, duration, easing);
        }

        public static Task<bool> CallAnimation<t>(VisualElement element, string name, Func<double, t> transform, Action<t> callBack
            , uint length, Easing easing)
        {
            easing = easing ?? Easing.Linear;
            var taskCompletionSource = new TaskCompletionSource<bool>();

            element.Animate(name, transform, callBack, 16, length, easing, (v, c) => taskCompletionSource.SetResult(c));
            return taskCompletionSource.Task;
        }

    }
}