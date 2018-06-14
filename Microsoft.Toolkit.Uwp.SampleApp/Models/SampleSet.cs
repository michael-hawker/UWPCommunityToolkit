// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Newtonsoft.Json;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Microsoft.Toolkit.Uwp.SampleApp
{
    /// <summary>
    /// A SampleSet consists of a named group of examples for a particular control.
    /// It can contain multiple compiled Pages (.xaml+.xaml.cs), .bind live-editable examples, and a .code flat examples (or any combination thereof).
    /// These are discovered automatically by any set of files sharing the same name within a directory and bundled as a sample.
    /// </summary>
    public class SampleSet : ISampleMetadata
    {
        private const string _repoOnlineRoot = "https://raw.githubusercontent.com/Microsoft/WindowsCommunityToolkit/";
        private const string _docsOnlineRoot = "https://raw.githubusercontent.com/MicrosoftDocs/UWPCommunityToolkitDocs/";
        private const string _cacheSHAKey = "docs-cache-sha";

        private static HttpClient client = new HttpClient();

        public static async void EnsureCacheLatest()
        {
            var settingsStorage = new LocalObjectStorageHelper();

            var onlineDocsSHA = await GetDocsSHA();
            var cacheSHA = settingsStorage.Read<string>(_cacheSHAKey);

            bool outdatedCache = onlineDocsSHA != null && cacheSHA != null && onlineDocsSHA != cacheSHA;
            bool noCache = onlineDocsSHA != null && cacheSHA == null;

            if (outdatedCache || noCache)
            {
                // Delete everything in the Cache Folder. Could be Pre 3.0.0 Cache data.
                foreach (var item in await ApplicationData.Current.LocalCacheFolder.GetItemsAsync())
                {
                    try
                    {
                        await item.DeleteAsync(StorageDeleteOption.Default);
                    }
                    catch
                    {
                    }
                }

                // Update Cache Version info.
                settingsStorage.Save(_cacheSHAKey, onlineDocsSHA);
            }
        }

        private string _cachedDocumentation = string.Empty;
        private string _cachedPath = string.Empty;

        internal static async Task<SampleSet> FindAsync(string category, string name)
        {
            var categories = await SampleLoader.GetCategoriesAsync();
            return categories?
                .FirstOrDefault(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase))?
                .Samples
                .FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets or sets name of sample directory for sample and name of sample which appears in Sample App UI lists.
        /// </summary>
        public string Name { get; set; }

        public string Description { get; set; }

        /// <inheritdoc/>
        public List<string> Tags { get; set; } = new List<string>();

        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the namespace suffix containing Sample Pages (.xaml+.xaml.cs) files, each one to be tied to an optional .bind/.code files as a 'Sample'.
        /// This will be appended to 'Microsoft.Toolkit.Uwp.SampleApp.SamplePages.'
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets the Page Type.
        /// </summary>
        ////public Type PageType => System.Type.GetType("Microsoft.Toolkit.Uwp.SampleApp.SamplePages." + Type);
        public string FullyQualifiedNamespace { get { return "Microsoft.Toolkit.Uwp.SampleApp.SamplePages." + Namespace; } }

        public List<Sample> Samples { get; set; }

        private string _codeUrl;

        public string CodeUrl
        {
            get
            {
                return _codeUrl;
            }

            set
            {
#if DEBUG
                _codeUrl = value;
#else
                var regex = new Regex("^https://github.com/Microsoft/WindowsCommunityToolkit/(tree|blob)/(?<branch>.+?)/(?<path>.*)");
                var docMatch = regex.Match(value);

                var branch = string.Empty;
                var path = string.Empty;
                if (docMatch.Success)
                {
                    branch = docMatch.Groups["branch"].Value;
                    path = docMatch.Groups["path"].Value;
                }

                if (string.IsNullOrWhiteSpace(branch))
                {
                    _codeUrl = value;
                }
                else
                {
                    _codeUrl = $"https://github.com/Microsoft/WindowsCommunityToolkit/tree/master/{path}";
                }
#endif
            }
        }

        public string DocumentationUrl { get; set; }

        public string Icon { get; set; }

        public string BadgeUpdateVersionRequired { get; set; }

        public string DeprecatedWarning { get; set; }

        public string ApiCheck { get; set; }

        public bool HasDocumentation => !string.IsNullOrEmpty(DocumentationUrl);

        public bool IsSupported
        {
            get
            {
                if (ApiCheck == null)
                {
                    return true;
                }

                return ApiInformation.IsTypePresent(ApiCheck);
            }
        }

#pragma warning disable SA1009 // Doesn't like ValueTuples.
        public async Task<(string contents, string path)> GetDocumentationAsync()
#pragma warning restore SA1009 // Doesn't like ValueTuples.
        {
            if (!string.IsNullOrWhiteSpace(_cachedDocumentation))
            {
                return (_cachedDocumentation, _cachedPath);
            }

            var filepath = string.Empty;
            var filename = string.Empty;
            var localPath = string.Empty;

            var docRegex = new Regex("^" + _repoOnlineRoot + "(?<branch>.+?)/docs/(?<file>.+)");
            var docMatch = docRegex.Match(DocumentationUrl);
            if (docMatch.Success)
            {
                filepath = docMatch.Groups["file"].Value;
                filename = Path.GetFileName(filepath);
                localPath = $"ms-appx:///docs/{Path.GetDirectoryName(filepath)}/";
            }

#if !DEBUG // use the docs repo in release mode
            string modifiedDocumentationUrl = $"{_docsOnlineRoot}master/docs/{filepath}";

            _cachedPath = modifiedDocumentationUrl.Replace(filename, string.Empty);

            // Read from Cache if available.
            try
            {
                _cachedDocumentation = await StorageFileHelper.ReadTextFromLocalCacheFileAsync(filename);
            }
            catch (Exception)
            {
            }

            // Grab from docs repo if not.
            if (string.IsNullOrWhiteSpace(_cachedDocumentation))
            {
                try
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(modifiedDocumentationUrl)))
                    {
                        using (var response = await client.SendAsync(request).ConfigureAwait(false))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                _cachedDocumentation = ProcessDocs(result);

                                if (!string.IsNullOrWhiteSpace(_cachedDocumentation))
                                {
                                    await StorageFileHelper.WriteTextToLocalCacheFileAsync(_cachedDocumentation, filename);
                                }
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }
            }
#endif

            // Grab the local copy in Debug mode, allowing you to preview changes made.
            if (string.IsNullOrWhiteSpace(_cachedDocumentation))
            {
                try
                {
                    using (var localDocsStream = await StreamHelper.GetPackagedFileStreamAsync($"docs/{filepath}"))
                    {
                        var result = await localDocsStream.ReadTextAsync();
                        _cachedDocumentation = ProcessDocs(result);
                        _cachedPath = localPath;
                    }
                }
                catch (Exception)
                {
                }
            }

            return (_cachedDocumentation, _cachedPath);
        }

        /// <summary>
        /// Gets the image data from a Uri, with Caching.
        /// </summary>
        /// <param name="uri">Image Uri</param>
        /// <returns>Image Stream</returns>
        public async Task<IRandomAccessStream> GetImageStream(Uri uri)
        {
            async Task<Stream> CopyStream(HttpContent source)
            {
                var stream = new MemoryStream();
                await source.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                return stream;
            }

            IRandomAccessStream imageStream = null;
            var localpath = $"{uri.Host}/{uri.LocalPath}";

            // Cache only in Release
#if !DEBUG
            try
            {
                imageStream = await StreamHelper.GetLocalCacheFileStreamAsync(localpath, Windows.Storage.FileAccessMode.Read);
            }
            catch
            {
            }
#endif

            if (imageStream == null)
            {
                try
                {
                    using (var response = await client.GetAsync(uri))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var imageCopy = await CopyStream(response.Content);
                            imageStream = imageCopy.AsRandomAccessStream();

                            // Cache only in Release
#if !DEBUG
                            // Takes a second copy of the image stream, so that is can save the image data to cache.
                            using (var saveStream = await CopyStream(response.Content))
                            {
                                await SaveImageToCache(localpath, saveStream);
                            }
#endif
                        }
                    }
                }
                catch
                {
                }
            }

            return imageStream;
        }

        private async Task SaveImageToCache(string localpath, Stream imageStream)
        {
            var folder = ApplicationData.Current.LocalCacheFolder;
            localpath = Path.Combine(folder.Path, localpath);

            // Resort to creating using traditional methods to avoid iteration for folder creation.
            Directory.CreateDirectory(Path.GetDirectoryName(localpath));

            using (var filestream = File.Create(localpath))
            {
                await imageStream.CopyToAsync(filestream);
            }
        }

        private string ProcessDocs(string docs)
        {
            string result = docs;

            var metadataRegex = new Regex("^---(.+?)---", RegexOptions.Singleline);
            var metadataMatch = metadataRegex.Match(result);
            if (metadataMatch.Success)
            {
                result = result.Remove(metadataMatch.Index, metadataMatch.Index + metadataMatch.Length);
            }

            // Images
            var regex = new Regex("## Example Image.+?##", RegexOptions.Singleline);
            result = regex.Replace(result, "##");

            return result;
        }

        private static async Task<string> GetDocsSHA()
        {
            try
            {
                var branchEndpoint = "https://api.github.com/repos/microsoftdocs/uwpcommunitytoolkitdocs/git/refs/heads/live";

                var request = new HttpRequestMessage(HttpMethod.Get, branchEndpoint);
                request.Headers.Add("User-Agent", "Windows Community Toolkit Sample App");

                using (request)
                {
                    using (var response = await client.SendAsync(request))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var raw = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(raw);
                            var json = JsonConvert.DeserializeObject<GitRef>(raw);
                            return json?.RefObject?.Sha;
                        }
                    }
                }
            }
            catch
            {
            }

            return null;
        }

        public class GitRef
        {
            [JsonProperty("object")]
            public GitRefObject RefObject { get; set; }
        }

        public class GitRefObject
        {
            [JsonProperty("sha")]
            public string Sha { get; set; }
        }
    }
}