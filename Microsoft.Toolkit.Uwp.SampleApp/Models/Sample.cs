using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Microsoft.Toolkit.Uwp.SampleApp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Uwp.SampleApp
{
    /// <summary>
    /// A Sample consists of a named group of an example for a particular control.
    /// It can contain a compiled Page (.xaml+.xaml.cs), a .bind live-editable example, and a .code flat example (or any combination thereof).
    /// These are discovered automatically by any set of files sharing the same name and bundled as a sample.
    /// </summary>
    public class Sample
    {
        /// <summary>
        /// Gets or sets the name of the sample as it will appear in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the sample when the user clicks the (i) info icon next to the sample listing.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets tags for search.
        /// </summary>
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
