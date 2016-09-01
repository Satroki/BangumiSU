using BangumiSU.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BangumiSU.Controls
{
    public class CommentBlock : Control
    {
        private static readonly Size MaxSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan FiveSeconds = TimeSpan.FromSeconds(5);

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(CommentBlock), new PropertyMetadata(default(string)));

        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(nameof(TextColor), typeof(Brush), typeof(CommentBlock), new PropertyMetadata(default(Brush)));

        public Brush TextShadow
        {
            get { return (Brush)GetValue(TextShadowProperty); }
            set { SetValue(TextShadowProperty, value); }
        }

        public static readonly DependencyProperty TextShadowProperty =
            DependencyProperty.Register(nameof(TextShadow), typeof(Brush), typeof(CommentBlock), new PropertyMetadata(default(Brush)));

        public double StartTime { get; set; }

        public CommentBlock()
        {
        }

        public CommentBlock(Comment comment)
        {
            Text = comment.Message;
            var r = (byte)(comment.Color >> 16 & 255);
            var g = (byte)(comment.Color >> 8 & 255);
            var b = (byte)(comment.Color & 255);
            TextColor = new SolidColorBrush(Color.FromArgb(255, r, g, b));
            TextShadow = new SolidColorBrush(Color.FromArgb(255, (byte)(255 - r), (byte)(255 - g), (byte)(255 - b)));
            StartTime = comment.Time;
        }

        private Size _Size;
        public Size Size
        {
            get
            {
                if (_Size == default(Size))
                    MeasureSize();
                return _Size;
            }
        }

        public void MeasureSize()
        {
            Measure(MaxSize);
            _Size = DesiredSize;
        }

        public void Add1s() => StartTime = StartTime + 1;
        public void Add5s() => StartTime = StartTime + 5;
        public void Subtract1s() => StartTime = StartTime - 1;
        public void Subtract5s() => StartTime = StartTime - 5;
    }
}
