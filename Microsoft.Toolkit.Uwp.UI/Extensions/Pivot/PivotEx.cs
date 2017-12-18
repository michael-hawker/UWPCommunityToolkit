﻿// ******************************************************************
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
    public partial class PivotEx
    {
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
