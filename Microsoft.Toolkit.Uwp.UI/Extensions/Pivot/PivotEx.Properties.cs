// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Uwp.UI.Extensions
{
    /// <summary>
    /// Set of extensions for the Pivot control.
    /// </summary>
    public partial class PivotEx
    {
        public static Style GetPivotHeaderItemStyle(Windows.UI.Xaml.Controls.Pivot obj)
        {
            return (Style)obj.GetValue(PivotHeaderItemStyleProperty);
        }

        public static void SetPivotHeaderItemStyle(Windows.UI.Xaml.Controls.Pivot obj, Style value)
        {
            obj.SetValue(PivotHeaderItemStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for PivotHeaderItemStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PivotHeaderItemStyleProperty =
            DependencyProperty.RegisterAttached("PivotHeaderItemStyle", typeof(Style), typeof(PivotEx), new PropertyMetadata(null, InitPivotStyle));

        /// <summary>
        /// Gets the Glyph attached property value.
        /// </summary>
        /// <param name="obj">PivotItem to get value from.</param>
        /// <returns>String value for FontIcon.</returns>
        public static string GetGlyph(PivotItem obj)
        {
            return (string)obj.GetValue(GlyphProperty);
        }

        /// <summary>
        /// Sets the attached property value for Glyph.
        /// </summary>
        /// <param name="obj">PivotItem to set value for.</param>
        /// <param name="value">String value for FontIcon.</param>
        public static void SetGlyph(PivotItem obj, string value)
        {
            obj.SetValue(GlyphProperty, value);
        }

        /// <summary>
        /// Used with the Pivot Styles to specify a FontIcon Glyph using the Segoe MDL2 Assets font.
        /// </summary>
        public static readonly DependencyProperty GlyphProperty =
            DependencyProperty.RegisterAttached("Glyph", typeof(string), typeof(PivotEx), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets the CloseButtonCommand attached property value.
        /// </summary>
        /// <param name="obj">Pivot to get value from.</param>
        /// <returns>Command value for the MSEdgeTabStyle Close Button.</returns>
        public static ICommand GetCloseButtonCommand(Windows.UI.Xaml.Controls.Pivot obj) // TODO: Should this be per PivotHeaderItem? Thinking no?
        {
            return (ICommand)obj.GetValue(CloseButtonCommandProperty);
        }

        /// <summary>
        /// Sets the attached property value for CloseButtonCommand.
        /// </summary>
        /// <param name="obj">Pivot to set value for.</param>
        /// <param name="value">Command value for the MSEdgeTabStyle Close Button.</param>
        public static void SetCloseButtonCommand(Windows.UI.Xaml.Controls.Pivot obj, ICommand value)
        {
            obj.SetValue(CloseButtonCommandProperty, value);
        }

        /// <summary>
        /// Used with the MSEdgeTab Pivot Styles to specify a Close Command for the Tab's Close Button.
        /// </summary>
        public static readonly DependencyProperty CloseButtonCommandProperty =
            DependencyProperty.RegisterAttached("CloseButtonCommand", typeof(ICommand), typeof(PivotEx), new PropertyMetadata(null));
    }
}
