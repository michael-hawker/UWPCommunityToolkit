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
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Media;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Uwp.SampleApp.Services
{
    public static class XamlTemplateLoader
    {
        public static async Task<XamlTemplate> LoadTemplateFromPackagedFileAsync(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return null;
            }

            var template = new XamlTemplate();

            // Get Xaml code
            using (var codeStream = await StreamHelper.GetPackagedFileStreamAsync(filename))
            {
                template.Template = await codeStream.ReadTextAsync(Encoding.UTF8);

                LoadTemplateMetadata(template);
                LoadTemplateProperties(filename, template);
            }

            return template;
        }

        private static Regex _commentRegex = new Regex(@"^<!--.*-->", RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));

        private static void LoadTemplateMetadata(XamlTemplate template)
        {
            // Extract Comment with Metadata (if any)
            var match = _commentRegex.Match(template.Template);

            if (match.Success)
            {
                // Strip out comment from Template.
                template.Template = template.Template.Substring(match.Value.Length).Trim();

                var content = match.Value.Substring(4, match.Value.Length - 7).Trim();

                var lines = content.Split('\n');
                foreach (var line in lines)
                {
                    var data = line.Split(':');
                    if (data.Length >= 2)
                    {
                        // Combine back if they used ':' in text somewhere.
                        var row = string.Join(":", data.Skip(1)).Trim();
                        switch (data[0].ToLower())
                        {
                            case "name":
                                template.Name = row;
                                break;
                            case "description":
                                template.Description = row;
                                break;
                            case "tags":
                                template.Tags = row.Split(',').Select(t => t.Trim()).ToList();
                                break;
                        }
                    }
                }
            }
        }

        private static void LoadTemplateProperties(string filename, XamlTemplate template)
        {
            // Look for @[] values and generate associated properties
            var regularExpression = new Regex(@"@\[(?<name>.+?)(:(?<type>.+?):(?<value>.+?)(:(?<parameters>.+?))?(:(?<options>.*))*)?\]@?");

            template.Properties = new PropertyDescriptor { Expando = new ExpandoObject() };
            var proxy = (IDictionary<string, object>)template.Properties.Expando;

            foreach (Match match in regularExpression.Matches(template.Template))
            {
                var label = match.Groups["name"].Value;
                var name = label.Replace(" ", string.Empty); // Allow us to have nicer display names, but create valid properties.
                var type = match.Groups["type"].Value;
                var value = match.Groups["value"].Value;

                var existingOption = template.Properties.Options.Where(o => o.Name == name).FirstOrDefault();

                if (existingOption == null && string.IsNullOrWhiteSpace(type))
                {
                    throw new NotSupportedException($"Unrecognized short identifier '{name}'; Define type and parameters of property in first occurance in {filename}.");
                }

                if (Enum.TryParse(type, out PropertyKind kind))
                {
                    if (existingOption != null)
                    {
                        if (existingOption.Kind != kind)
                        {
                            throw new NotSupportedException($"Multiple options with same name but different type not supported: {filename}:{name}");
                        }

                        continue;
                    }

                    PropertyOptions options;

                    switch (kind)
                    {
                        case PropertyKind.Slider:
                        case PropertyKind.DoubleSlider:
                            try
                            {
                                var sliderOptions = new SliderPropertyOptions { DefaultValue = double.Parse(value, CultureInfo.InvariantCulture) };
                                var parameters = match.Groups["parameters"].Value;
                                var split = parameters.Split('-');
                                int minIndex = 0;
                                int minMultiplier = 1;
                                if (string.IsNullOrEmpty(split[0]))
                                {
                                    minIndex = 1;
                                    minMultiplier = -1;
                                }

                                sliderOptions.MinValue = minMultiplier * double.Parse(split[minIndex], CultureInfo.InvariantCulture);
                                sliderOptions.MaxValue = double.Parse(split[minIndex + 1], CultureInfo.InvariantCulture);
                                if (split.Length > 2 + minIndex)
                                {
                                    sliderOptions.Step = double.Parse(split[split.Length - 1], CultureInfo.InvariantCulture);
                                }

                                options = sliderOptions;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Unable to extract slider info from {value}({ex.Message})");
                                TrackingManager.TrackException(ex);
                                continue;
                            }

                            break;

                        case PropertyKind.TimeSpan:
                            try
                            {
                                var sliderOptions = new SliderPropertyOptions { DefaultValue = TimeSpan.FromMilliseconds(double.Parse(value, CultureInfo.InvariantCulture)) };
                                var parameters = match.Groups["parameters"].Value;
                                var split = parameters.Split('-');
                                int minIndex = 0;
                                int minMultiplier = 1;
                                if (string.IsNullOrEmpty(split[0]))
                                {
                                    minIndex = 1;
                                    minMultiplier = -1;
                                }

                                sliderOptions.MinValue = minMultiplier * double.Parse(split[minIndex], CultureInfo.InvariantCulture);
                                sliderOptions.MaxValue = double.Parse(split[minIndex + 1], CultureInfo.InvariantCulture);
                                if (split.Length > 2 + minIndex)
                                {
                                    sliderOptions.Step = double.Parse(split[split.Length - 1], CultureInfo.InvariantCulture);
                                }

                                options = sliderOptions;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Unable to extract slider info from {value}({ex.Message})");
                                TrackingManager.TrackException(ex);
                                continue;
                            }

                            break;

                        case PropertyKind.Enum:
                            try
                            {
                                options = new PropertyOptions();
                                var split = value.Split('.');
                                var typeName = string.Join(".", split.Take(split.Length - 1));
                                var enumType = LookForTypeByName(typeName);
                                options.DefaultValue = Enum.Parse(enumType, split.Last());
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Unable to parse enum from {value}({ex.Message})");
                                TrackingManager.TrackException(ex);
                                continue;
                            }

                            break;

                        case PropertyKind.Bool:
                            try
                            {
                                options = new PropertyOptions { DefaultValue = bool.Parse(value) };
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Unable to parse bool from {value}({ex.Message})");
                                continue;
                            }

                            break;

                        case PropertyKind.Brush:
                            try
                            {
                                options = new PropertyOptions { DefaultValue = value };
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Unable to parse bool from {value}({ex.Message})");
                                TrackingManager.TrackException(ex);
                                continue;
                            }

                            break;

                        case PropertyKind.Thickness:
                            try
                            {
                                var thicknessOptions = new ThicknessPropertyOptions { DefaultValue = value };
                                options = thicknessOptions;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Unable to extract slider info from {value}({ex.Message})");
                                TrackingManager.TrackException(ex);
                                continue;
                            }

                            break;

                        default:
                            options = new PropertyOptions { DefaultValue = value };
                            break;
                    }

                    options.Label = label;
                    options.Name = name;
                    options.OriginalString = match.Value;
                    options.Kind = kind;
                    options.IsTwoWayBinding = options.OriginalString.EndsWith("@");
                    proxy[name] = new ValueHolder(options.DefaultValue);

                    template.Properties.Options.Add(options);
                }
            }
        }

        private static Type LookForTypeByName(string typeName)
        {
            // First search locally
            var result = System.Type.GetType(typeName);

            if (result != null)
            {
                return result;
            }

            // Search in Windows
            var proxyType = VerticalAlignment.Center;
            var assembly = proxyType.GetType().GetTypeInfo().Assembly;

            foreach (var typeInfo in assembly.ExportedTypes)
            {
                if (typeInfo.Name == typeName)
                {
                    return typeInfo;
                }
            }

            // Search in Microsoft.Toolkit.Uwp.UI.Controls
            var controlsProxyType = GridSplitter.GridResizeDirection.Auto;
            assembly = controlsProxyType.GetType().GetTypeInfo().Assembly;

            foreach (var typeInfo in assembly.ExportedTypes)
            {
                if (typeInfo.Name == typeName)
                {
                    return typeInfo;
                }
            }

            // Search in Microsoft.Toolkit.Uwp.UI.Animations
            var animationsProxyType = EasingType.Default;
            assembly = animationsProxyType.GetType().GetTypeInfo().Assembly;
            foreach (var typeInfo in assembly.ExportedTypes)
            {
                if (typeInfo.Name == typeName)
                {
                    return typeInfo;
                }
            }

            // Search in Microsoft.Toolkit.Uwp.UI
            var uiProxyType = ImageBlendMode.Multiply;
            assembly = uiProxyType.GetType().GetTypeInfo().Assembly;
            foreach (var typeInfo in assembly.ExportedTypes)
            {
                if (typeInfo.Name == typeName)
                {
                    return typeInfo;
                }
            }

            return null;
        }
    }
}
