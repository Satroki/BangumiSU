using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;


namespace BangumiSU.Controls
{
    public sealed class ScoreControl : Control
    {
        private List<SymbolIcon> Icons;
        private static Brush Gold = new SolidColorBrush(Colors.Gold);
        private static Brush LightGray = new SolidColorBrush(Colors.LightGray);

        public ScoreControl()
        {
            this.DefaultStyleKey = typeof(ScoreControl);
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(ScoreControl), new PropertyMetadata(default(string)));

        public int Score
        {
            get { return (int)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }

        public Brush Brushs { get; private set; }

        public static readonly DependencyProperty ScoreProperty =
            DependencyProperty.Register(nameof(Score), typeof(int), typeof(ScoreControl), new PropertyMetadata(default(int), OnScoreChanged));

        private static void OnScoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ScoreControl)?.ScoreChanged((int)e.NewValue);
        }

        private void ScoreChanged(int newValue)
        {
            SetStars(newValue);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var sp = GetTemplateChild("IconPanel") as StackPanel;
            sp.PointerExited += Sp_PointerExited;
            Icons = sp.Children.OfType<SymbolIcon>().ToList();
            for (int i = 0; i < Icons.Count; i++)
            {
                Icons[i].PointerEntered += ScoreControl_PointerEntered;
                Icons[i].Tapped += ScoreControl_Tapped;
                Icons[i].Tag = i + 1;
            }
            SetStars(Score);
        }

        private void ScoreControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var value = (int)(sender as FrameworkElement).Tag;
            Score = value;
        }

        private void ScoreControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var value = (int)(sender as FrameworkElement).Tag;
            SetStars(value);
        }

        private void Sp_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            SetStars(Score);
        }

        private void SetStars(int value)
        {
            for (int i = 0; i < 5; i++)
            {
                if (i < value)
                {
                    Icons[i].Foreground = Gold;
                    Icons[i].Symbol = Symbol.SolidStar;
                }
                else
                {
                    Icons[i].Foreground = LightGray;
                    Icons[i].Symbol = Symbol.OutlineStar;
                }
            }
        }
    }
}
