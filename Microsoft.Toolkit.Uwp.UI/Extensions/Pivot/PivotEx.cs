using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Uwp.UI.Extensions
{
    /// <summary>
    /// Set of extensions for the Pivot control.
    /// </summary>
    [Bindable]
    public class PivotEx
    {


        public static Style GetPivotHeaderItemStyle(Pivot obj)
        {
            return (Style)obj.GetValue(PivotHeaderItemStyleProperty);
        }

        public static void SetPivotHeaderItemStyle(Pivot obj, Style value)
        {
            obj.SetValue(PivotHeaderItemStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for PivotHeaderItemStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PivotHeaderItemStyleProperty =
            DependencyProperty.RegisterAttached("PivotHeaderItemStyle", typeof(Style), typeof(PivotEx), new PropertyMetadata(null, InitPivotStyle));

        private static void InitPivotStyle(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pivot = d as Pivot;

            if (pivot == null)
            {
                return;
            }

            pivot.Loaded -= Pivot_Loaded;
            pivot.Loaded += Pivot_Loaded; ;
        }

        private static void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            var pivot = sender as Pivot;

            if (pivot == null)
            {
                return;
            }

            var panel = pivot.FindDescendant<PivotHeaderPanel>();

            var style = GetPivotHeaderItemStyle(pivot);

            foreach (var child in panel.Children)
            {
                var phi = child as PivotHeaderItem;
                phi.Style = style;
            }

            // TODO: Listen to Children count change:
            // https://toresenneseth.wordpress.com/2010/10/29/uielementcollection-changed-notification/
            // Add as extension type event to UIElementCollection?
        }
    }
}
