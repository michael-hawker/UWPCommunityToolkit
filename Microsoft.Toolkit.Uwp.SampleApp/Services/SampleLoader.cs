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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Microsoft.Toolkit.Uwp.SampleApp.Services;
using Newtonsoft.Json;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Search;

namespace Microsoft.Toolkit.Uwp.SampleApp
{
    public static class SampleLoader
    {
        private const string _recentSamplesStorageKey = "uct-recent-samples";

        private static List<SampleCategory> _samplesCategories;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private static LinkedList<SampleSet> _recentSamples;
        private static RoamingObjectStorageHelper _roamingObjectStorageHelper = new RoamingObjectStorageHelper();

        public static async Task<SampleCategory> GetCategoryBySample(SampleSet sample)
        {
            return (await GetCategoriesAsync()).FirstOrDefault(c => c.Samples.Contains(sample));
        }

        public static async Task<SampleCategory> GetCategoryByName(string name)
        {
            return (await GetCategoriesAsync()).FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<SampleSet> GetSampleByName(string name)
        {
            return (await GetCategoriesAsync()).SelectMany(c => c.Samples).FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<SampleSet[]> FindSamplesByName(string name)
        {
            var query = name.ToLower();
            return (await GetCategoriesAsync()).SelectMany(c => c.Samples).Where(s => s.Name.ToLower().Contains(query)).ToArray();
        }

        /// <summary>
        /// Searches all <see cref="Sample"/> and <see cref="SampleSet"/> for the query provided within Name, Description, and Tags.
        /// </summary>
        /// <param name="nameOrTag">Search query text.</param>
        /// <returns>Array of samples.</returns>
        public static async Task<ISampleMetadata[]> SearchSamples(string nameOrTag)
        {
            // TODO: Don't provide the Sample + SampleSet if the set only contains one sample (or vice versa?)
            var query = nameOrTag.ToLower();
            return (await GetCategoriesAsync()).SelectMany(c => c.Samples.SelectMany(set => set.Samples.ToList<ISampleMetadata>()).Union(c.Samples)).Where(s => s.Name.ToLower().Contains(query) || s.Description.ToLower().Contains(query) || s.Tags.Contains(query, StringComparer.CurrentCultureIgnoreCase)).ToArray();
        }

        public static async Task<List<SampleCategory>> GetCategoriesAsync()
        {
            await _semaphore.WaitAsync();
            if (_samplesCategories == null)
            {
                try
                {
                    // Get our ordered list of categories to start us off
                    List<SampleCategory> allCategories = new List<SampleCategory>();
                    using (var jsonStream = await StreamHelper.GetPackagedFileStreamAsync("categories.json"))
                    {
                        var jsonString = await jsonStream.ReadTextAsync();
                        allCategories = JsonConvert.DeserializeObject<List<SampleCategory>>(jsonString);
                    }

                    // Search for all 'definition.json' files
                    var query = new QueryOptions(CommonFileQuery.DefaultQuery, new string[] { ".json" })
                    {
                        FolderDepth = FolderDepth.Deep
                    };

                    StorageFolder install = Package.Current.InstalledLocation;
                    var pages = await install.GetFolderAsync("SamplePages");
                    var search = pages.CreateFileQueryWithOptions(query);

                    // Load each sample and add it to our dictionary based on the sample's category
                    foreach (var file in await search.GetFilesAsync())
                    {
                        if (file.Name.ToLower() == "definition.json")
                        {
                            using (var jsonStream = await file.OpenAsync(FileAccessMode.Read))
                            {
                                var jsonString = await jsonStream.ReadTextAsync();
                                var sample = JsonConvert.DeserializeObject<SampleSet>(jsonString);
                                var cat = allCategories.FirstOrDefault(c => c.Name == sample.Category);
                                if (cat != null)
                                {
                                    cat.Samples.Add(sample);
                                }
                                else
                                {
#if DEBUG
                                    // Category doesn't exist, make sure it matches the ones in 'categories.json'
                                    Debugger.Break();
#endif
                                }
                            }
                        }
                    }

                    // Check API for each sample and add to category
                    var supportedCategories = new List<SampleCategory>();
                    foreach (var category in allCategories)
                    {
                        var finalSamples = new List<SampleSet>();

                        foreach (var sample in category.Samples)
                        {
                            if (sample.IsSupported)
                            {
                                sample.Samples = await GetSamplesAsync(sample);
                                if (sample.Samples.Count > 0)
                                {
                                    finalSamples.Add(sample);
                                }
                                else
                                {
#if DEBUG
                                    // Sample with no samples...?
                                    Debugger.Break();
#endif
                                }
                            }
                        }

                        if (finalSamples.Count > 0)
                        {
                            supportedCategories.Add(category);
                            category.Samples = finalSamples.OrderBy(s => s.Name).ToList();
                        }
                        else
                        {
#if DEBUG
                            // Empty Category...
                            Debugger.Break();
#endif
                        }
                    }

                    _samplesCategories = supportedCategories.ToList();
                }
                catch (Exception e)
                {
                    _samplesCategories = new List<SampleCategory>();
                }
            }

            _semaphore.Release();
            return _samplesCategories;
        }

        public static async Task<List<Sample>> GetSamplesAsync(SampleSet sample)
        {
            var dict = new Dictionary<string, Sample>();

            try
            {
                StorageFolder install = Package.Current.InstalledLocation;
                var pages = await install.GetFolderAsync("SamplePages");
                var sampleFolder = await pages.GetFolderAsync(sample.Name);

                // Load each sample and add it to our dictionary based on the sample's category
                foreach (var file in await sampleFolder.GetFilesAsync())
                {
                    if (!(file.FileType == ".bind" || file.FileType == ".code"))
                    {
                        continue;
                    }

                    Debug.WriteLine(file.Name);
                    if (!dict.ContainsKey(file.DisplayName))
                    {
                        dict[file.DisplayName] = new Sample(sample);
                    }

                    switch (file.FileType)
                    {
                        case ".bind":
                            dict[file.DisplayName].XamlCodeFile = file.Name;
                            var template = await XamlTemplateLoader.LoadTemplateFromPackagedFileAsync($"SamplePages/{sample.Name}/{file.Name}");
                            dict[file.DisplayName].XamlTemplate = template;

                            if (string.IsNullOrWhiteSpace(dict[file.DisplayName].Name) && !string.IsNullOrWhiteSpace(template.Name))
                            {
                                dict[file.DisplayName].Name = template.Name;
                                dict[file.DisplayName].Description = template.Description;
                                dict[file.DisplayName].Tags = template.Tags;
                            }

                            break;
                        case ".code":
                            dict[file.DisplayName].CodeFile = file.Name;

                            // TODO: Figure out metadata here as well...?
                            break;
                    }
                }
            }
            catch (Exception e)
            {
            }

            // Get list of types
            foreach (var type in sample.GetType().GetTypeInfo().Assembly.ExportedTypes)
            {
                if (type.Namespace == sample.FullyQualifiedNamespace)
                {
                    var name = type.Name.Replace("Page", string.Empty);
                    if (!dict.ContainsKey(name))
                    {
                        dict[name] = new Sample(sample);
                    }

                    dict[name].Type = type;

                    if (string.IsNullOrWhiteSpace(dict[name].Name) && type.GetTypeInfo().GetCustomAttribute(typeof(SampleMetadataAttribute)) is SampleMetadataAttribute attribute)
                    {
                        dict[name].Name = attribute.Name;
                        dict[name].Description = attribute.Description;
                        dict[name].Tags = attribute.Tags;
                    }
                }
            }

            //// TODO: Check if no name provided, if so, set to key name.

            return dict.Values.ToList();
        }

        public static async Task<LinkedList<SampleSet>> GetRecentSamples()
        {
            if (_recentSamples == null)
            {
                _recentSamples = new LinkedList<SampleSet>();
                var savedSamples = _roamingObjectStorageHelper.Read<string>(_recentSamplesStorageKey);

                if (savedSamples != null)
                {
                    var sampleNames = savedSamples.Split(';').Reverse();
                    foreach (var name in sampleNames)
                    {
                        var sample = await GetSampleByName(name);
                        if (sample != null)
                        {
                            _recentSamples.AddFirst(sample);
                        }
                    }
                }
            }

            return _recentSamples;
        }

        public static async Task PushRecentSample(SampleSet sample)
        {
            var samples = await GetRecentSamples();

            var duplicates = samples.Where(s => s.Name == sample.Name).ToList();
            foreach (var duplicate in duplicates)
            {
                samples.Remove(duplicate);
            }

            samples.AddFirst(sample);
            while (samples.Count > 10)
            {
                samples.RemoveLast();
            }

            SaveRecentSamples();
        }

        private static void SaveRecentSamples()
        {
            if (_recentSamples == null)
            {
                return;
            }

            var str = string.Join(";", _recentSamples.Take(10).Select(s => s.Name).ToArray());
            _roamingObjectStorageHelper.Save<string>(_recentSamplesStorageKey, str);
        }
    }
}
