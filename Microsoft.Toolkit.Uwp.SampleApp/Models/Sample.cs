﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Microsoft.Toolkit.Uwp.SampleApp.Services;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Uwp.SampleApp
{
    /// <summary>
    /// A Sample consists of a named group of an example for a particular control.
    /// It can contain a compiled Page (.xaml+.xaml.cs), a .bind live-editable example, and a .code flat example (or any combination thereof).
    /// These are discovered automatically by any set of files sharing the same name and bundled as a sample.
    /// </summary>
    [Bindable]
    public class Sample : ISampleMetadata
    {
        /// <summary>
        /// Gets parent <see cref="SampleSet"/>.
        /// </summary>
        public SampleSet Parent { get; private set; }

        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <inheritdoc/>
        public string Description { get; set; } = string.Empty;

        /// <inheritdoc/>
        public List<string> Tags { get; set; } = new List<string>();

        /// <inheritdoc/>
        public string Icon => Parent.Icon;

        /// <inheritdoc/>
        public string BadgeUpdateVersionRequired => Parent.BadgeUpdateVersionRequired;

        /// <summary>
        /// Gets or sets the 'Page' type for this sample.
        /// </summary>
        public Type Type { get; set; }

        public string CodeFile { get; set; }

        public string JavaScriptCodeFile { get; set; }

        public string XamlCodeFile { get; set; }

        public XamlTemplate XamlTemplate { get; set; }

        public bool DisableXamlEditorRendering { get; set; }

        public string XamlCode { get; private set; }

        public bool HasXAMLCode => !string.IsNullOrEmpty(XamlCodeFile);

        public bool HasCSharpCode => !string.IsNullOrEmpty(CodeFile);

        public bool HasJavaScriptCode => !string.IsNullOrEmpty(JavaScriptCodeFile);

        public Sample(SampleSet suite)
        {
            Parent = suite;
        }

        public async Task<string> GetCSharpSourceAsync()
        {
            using (var codeStream = await StreamHelper.GetPackagedFileStreamAsync($"SamplePages/{Parent.Name}/{CodeFile}"))
            {
                using (var streamreader = new StreamReader(codeStream.AsStream()))
                {
                    return await streamreader.ReadToEndAsync();
                }
            }
        }

        public async Task<string> GetJavaScriptSourceAsync()
        {
            using (var codeStream = await StreamHelper.GetPackagedFileStreamAsync($"SamplePages/{Parent.Name}/{JavaScriptCodeFile}"))
            {
                using (var streamreader = new StreamReader(codeStream.AsStream()))
                {
                    return await streamreader.ReadToEndAsync();
                }
            }
        }
    }
}
