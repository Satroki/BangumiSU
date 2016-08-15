using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace BangumiSU.Controls
{
    public class GridViewEx : GridView
    {
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            var gridItem = element as GridViewItem;

            var binding = new Binding { Mode = BindingMode.TwoWay, Source = item, Path = new PropertyPath("IsSelected") };
            gridItem.SetBinding(SelectorItem.IsSelectedProperty, binding);
        }
    }
}
