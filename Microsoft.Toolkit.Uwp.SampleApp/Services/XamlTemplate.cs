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

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.SampleApp.Models;

namespace Microsoft.Toolkit.Uwp.SampleApp.Services
{
    public class XamlTemplate : ISampleMetadata
    {
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public List<string> Tags { get; set; }

        // TODO: Listen to all property changes and notify of property updates???
        public PropertyDescriptor Properties { get; set; }

        public string Template { get; set; }

        /// <summary>
        /// Gets a version of the XamlCode with the explicit values of the option controls.
        /// </summary>
        public string XamlWithValues
        {
            get
            {
                if (Properties == null)
                {
                    return string.Empty;
                }

                var result = Template.ToString(); // Copy
                var proxy = (IDictionary<string, object>)Properties.Expando;
                foreach (var option in Properties.Options)
                {
                    if (proxy[option.Name] is ValueHolder value)
                    {
                        var newString = value.Value is Windows.UI.Xaml.Media.SolidColorBrush brush ?
                                            brush.Color.ToString() : value.Value.ToString();

                        result = result.Replace(option.OriginalString, newString);
                        result = result.Replace("@[" + option.Label + "]@", newString);
                        result = result.Replace("@[" + option.Label + "]", newString);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Gets a version of the XamlCode bound directly to the slider/option controls.
        /// </summary>
        public string XamlWithBindings
        {
            get
            {
                if (Properties == null)
                {
                    return string.Empty;
                }

                var result = Template.ToString(); // Copy
                var proxy = (IDictionary<string, object>)Properties.Expando;
                foreach (var option in Properties.Options)
                {
                    if (proxy[option.Name] is ValueHolder value)
                    {
                        result = result.Replace(
                            option.OriginalString,
                            "{Binding " + option.Name + ".Value, Mode=" + (option.IsTwoWayBinding ? "TwoWay" : "OneWay") + "}");
                        result = result.Replace(
                            "@[" + option.Label + "]@",
                            "{Binding " + option.Name + ".Value, Mode=TwoWay}");
                        result = result.Replace(
                            "@[" + option.Label + "]",
                            "{Binding " + option.Name + ".Value, Mode=OneWay}"); // Order important here.
                    }
                }

                return result;
            }
        }
    }
}
