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
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Uwp.UI.Extensions
{
    /// <summary>
    /// Helper to retrieve the CloseCommandButton Attached Property from a Pivot for the PivotHeaderItem Style Templates.
    /// </summary>
    public class GetPivotExCloseButtonCommandConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            /*var pivotheader = value as PivotHeaderItem;

            var panel = pivotheader?.Parent as PivotHeaderPanel;
            var index = panel?.Children?.IndexOf(pivotheader);*/

            var pivot = (value as DependencyObject)?.FindAscendant<Pivot>();

            if (pivot != null)
            {
                return PivotEx.GetCloseButtonCommand(pivot);
            }

            /*if (index != null)
            {
                var pivotitem = pivot?.Items[index.Value] as PivotItem;

                if (pivotitem != null)
                {
                    var glyph = PivotEx.GetGlyph(pivotitem);

                    return glyph ?? string.Empty; // ""; // PivotEx.GetGlyph(pivot);
                }
            }*/

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
