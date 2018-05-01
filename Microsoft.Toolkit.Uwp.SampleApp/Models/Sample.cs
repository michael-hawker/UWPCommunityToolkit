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
        /// <inheritdoc/>
        public string Name { get; set; }

        /// <inheritdoc/>
        public string Description { get; set; }

        /// <inheritdoc/>
        public List<string> Tags { get; set; }

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

        public async Task<string> GetCSharpSourceAsync()
        {
            using (var codeStream = await StreamHelper.GetPackagedFileStreamAsync($"SamplePages/{Name}/{CodeFile}"))
            {
                using (var streamreader = new StreamReader(codeStream.AsStream()))
                {
                    return await streamreader.ReadToEndAsync();
                }
            }
        }

        public async Task<string> GetJavaScriptSourceAsync()
        {
            using (var codeStream = await StreamHelper.GetPackagedFileStreamAsync($"SamplePages/{Name}/{JavaScriptCodeFile}"))
            {
                using (var streamreader = new StreamReader(codeStream.AsStream()))
                {
                    return await streamreader.ReadToEndAsync();
                }
            }
        }
    }
}
