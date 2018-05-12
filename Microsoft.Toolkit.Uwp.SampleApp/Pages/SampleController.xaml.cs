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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.SampleApp.Common;
using Microsoft.Toolkit.Uwp.SampleApp.Controls;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.Toolkit.Uwp.SampleApp
{
    /// <summary>
    /// A wrapper for the Sample Page.
    /// </summary>
    public sealed partial class SampleController : Page, INotifyPropertyChanged
    {
        public static SampleController Current { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public SampleSet SampleSuite { get; private set; }

        public Sample CurrentSample {get; private set; }

        public ObservableCollection<SampleCommand> Commands { get; } = new ObservableCollection<SampleCommand>();

        public bool DisplayWaitRing
        {
            set { waitRing.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        public PaneState SidePaneState
        {
            get => _paneState;
            set
            {
                _paneState = value;
                UpdateProperty();
            }
        }

        public bool UseBackground
        {
            get
            {
                return _useBackground;
            }

            set
            {
                _useBackground = value;
                UpdateProperty(nameof(UseBackground));
            }
        }

        private Page SamplePage => SampleContent.Content as Page;

        private bool CanChangePaneState => _hasDocumentation && !_onlyDocumentation;

        private XamlRenderService _xamlRenderer = new XamlRenderService();
        private bool _lastRenderedProperties = true;
        private bool _xamlCodeRendererSupported = false;

        private bool _useBackground = false;

        private PaneState _paneState;
        private bool _hasDocumentation = true;
        private bool _onlyDocumentation;
        private string documentationPath;

        public SampleController()
        {
            this.InitializeComponent();
            Current = this;
            Shell.Current.ThemeChanged += Current_ThemeChanged;

            ThemePicker.SelectedIndex = (int)Shell.Current.GetCurrentTheme();
            ThemePicker.SelectionChanged += ThemePicker_SelectionChanged;

            DocumentationTextblock.RequestedTheme = Shell.Current.GetCurrentTheme();
            DocumentationTextblock.SetRenderer<SampleAppMarkdownRenderer>();

            ProcessSampleEditorTime();
            XamlCodeEditor.UpdateRequested += XamlCodeEditor_UpdateRequested;
        }

        public void OpenClosePane()
        {
            if (CanChangePaneState)
            {
                if (SidePaneState == PaneState.Closed)
                {
                    SidePaneState = PaneState.Normal;
                }
                else
                {
                    SidePaneState = PaneState.Closed;
                }
            }
        }

        public void ExpandCollapsePane()
        {
            if (CanChangePaneState)
            {
                if (SidePaneState == PaneState.Full)
                {
                    SidePaneState = PaneState.Normal;
                }
                else
                {
                    SidePaneState = PaneState.Full;
                }
            }
        }

        public void RegisterNewCommand(string name, RoutedEventHandler action)
        {
            Commands.Add(new SampleCommand(name, () =>
            {
                try
                {
                    action.Invoke(this, new RoutedEventArgs());
                }
                catch (Exception ex)
                {
                    ExceptionNotification.Show(ex.Message, 3000);
                }
            }));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is Sample sample)
            {
                SampleSuite = sample.Parent;
                CurrentSample = SampleSuite.Samples.FirstOrDefault(s => s.Name == sample.Name);
            }
            else if (e.Parameter is SampleSet sampleset)
            {
                SampleSuite = sampleset;
                CurrentSample = SampleSuite.Samples.First();
            }
        }

        public async Task RefreshXamlRenderAsync()
        {
            if (CurrentSample != null)
            {
                var code = string.Empty;
                if (InfoAreaPivot.SelectedItem == PropertiesPivotItem)
                {
                    code = CurrentSample.XamlTemplate.XamlWithBindings;
                }
                else
                {
                    code = CurrentSample.XamlTemplate.XamlWithValues;
                }

                if (!string.IsNullOrWhiteSpace(code))
                {
                    await UpdateXamlRenderAsync(code);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (SamplePage is ISampleNavigation nav)
            {
                nav.NavigatingAway();
            }

            XamlCodeEditor = null;

            // Not great, but need to collect up after WebView. (Does this work?)
            GC.Collect();
        }

        private void SamplePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentSample != null && CurrentSample.HasXAMLCode)
            {
                this._lastRenderedProperties = true;
            }
        }

        private async void InfoAreaPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (InfoAreaPivot.SelectedItem != null)
            {
                if (DataContext is Sample sample)
                {
                    TrackingManager.TrackEvent("PropertyGrid", (InfoAreaPivot.SelectedItem as FrameworkElement)?.Name, sample.Name);
                }
            }

            if (CurrentSample == null)
            {
                return;
            }

            if (InfoAreaPivot.SelectedItem == PropertiesPivotItem)
            {
                // If we switch to the Properties Panel, we want to use a binded version of the Xaml Code.
                if (CurrentSample.HasXAMLCode)
                {
                    _lastRenderedProperties = true;

                    var t = UpdateXamlRenderAsync(CurrentSample.XamlTemplate.XamlWithBindings);
                }

                return;
            }

            if (CurrentSample.HasXAMLCode && InfoAreaPivot.SelectedItem == XamlPivotItem && _lastRenderedProperties)
            {
                // Use this flag so we don't re-render the XAML tab if we're switching from tabs other than the properties one.
                _lastRenderedProperties = false;

                // If we switch to the Live Preview, then we want to use the Value based Text
                XamlCodeEditor.Text = CurrentSample.XamlTemplate.XamlWithValues;

                var t = UpdateXamlRenderAsync(CurrentSample.XamlTemplate.XamlWithValues);
                await XamlCodeEditor.ResetPosition();

                XamlCodeEditor.Focus(FocusState.Programmatic);
                return;
            }

            if (CurrentSample.HasXAMLCode && InfoAreaPivot.SelectedItem == XamlReadOnlyPivotItem)
            {
                // Update Read-Only XAML tab on non-desktop devices to show changes to Properties
                XamlReadOnlyCodeRenderer.SetCode(CurrentSample.XamlTemplate.XamlWithValues, "xaml");
            }

            if (CurrentSample.HasCSharpCode && InfoAreaPivot.SelectedItem == CSharpPivotItem)
            {
                var code = await CurrentSample.GetCSharpSourceAsync();
                CSharpCodeRenderer.SetCode(code, "c#");

                return;
            }

            if (CurrentSample.HasJavaScriptCode && InfoAreaPivot.SelectedItem == JavaScriptPivotItem)
            {
                var code = await CurrentSample.GetJavaScriptSourceAsync();
                JavaScriptCodeRenderer.SetCode(code, "js");

                return;
            }
        }

        private async void XamlCodeEditor_UpdateRequested(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                var t = UpdateXamlRenderAsync(XamlCodeEditor.Text);
            });
        }

        private async void DocumentationTextblock_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            TrackingManager.TrackEvent("Link", e.Link);
            if (Uri.TryCreate(e.Link, UriKind.Absolute, out Uri result))
            {
                await Launcher.LaunchUriAsync(new Uri(e.Link));
            }
        }

        private async void DocumentationTextblock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            var deferral = e.GetDeferral();
            BitmapImage image = null;

            // Determine if the link is not absolute, meaning it is relative.
            if (!Uri.TryCreate(e.Url, UriKind.Absolute, out Uri url))
            {
                url = new Uri(documentationPath + e.Url);
            }

            if (url.Scheme == "ms-appx")
            {
                image = new BitmapImage(url);
            }
            else
            {
                var imageStream = await SampleSuite.GetImageStream(url);

                if (imageStream != null)
                {
                    image = new BitmapImage();
                    await image.SetSourceAsync(imageStream);
                }
            }

            // Handle only if no exceptions occur.
            if (image != null)
            {
                e.Image = image;
                e.Handled = true;
            }

            deferral.Complete();
        }

        private async void GitHub_OnClick(object sender, RoutedEventArgs e)
        {
            var url = SampleSuite.CodeUrl;
            TrackingManager.TrackEvent("Link", url);
            try
            {
                await Launcher.LaunchUriAsync(new Uri(url));
            }
            catch { }
        }

        private async Task UpdateXamlRenderAsync(string text)
        {
            // Hide any Previous Errors
            XamlCodeEditor.ClearErrors();

            // Try and Render Xaml to a UIElement
            UIElement element = null;
            try
            {
                element = _xamlRenderer.Render(text);
            }
            catch (Exception ex)
            {
                ExceptionNotification.Show(ex.Message, 3000);
            }

            if (element != null)
            {
                // Add element to main panel
                if (SamplePage == null)
                {
                    return;
                }

                var root = SamplePage.FindDescendantByName("XamlRoot");

                if (root is Panel)
                {
                    // If we've defined a 'XamlRoot' element to host us as a panel, use that.
                    (root as Panel).Children.Clear();
                    (root as Panel).Children.Add(element);
                }
                else
                {
                    // Otherwise, just replace the entire page's content
                    SamplePage.Content = element;
                }

                // Tell the page we've finished with an update to the XAML contents, after the control has rendered.
                await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    (SamplePage as IXamlRenderListener)?.OnXamlRendered(element as FrameworkElement);
                });
            }
            else if (_xamlRenderer.Errors.Count > 0)
            {
                var error = _xamlRenderer.Errors.First();

                XamlCodeEditor.ReportError(error);
            }
        }

        private Visibility GreaterThanZero(int value)
        {
            return value > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility GreaterThanOne(int value)
        {
            return value > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        private Visibility Not(bool value)
        {
            return value ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UpdateProperty([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ProcessSampleEditorTime()
        {
            if (CurrentSample != null &&
                CurrentSample.HasXAMLCode &&
                _xamlCodeRendererSupported)
            {
                if (XamlCodeEditor.TimeSampleEditedFirst != DateTime.MinValue &&
                    XamlCodeEditor.TimeSampleEditedLast != DateTime.MinValue)
                {
                    int secondsEdditingSample = (int)Math.Floor((XamlCodeEditor.TimeSampleEditedLast - XamlCodeEditor.TimeSampleEditedFirst).TotalSeconds);
                    TrackingManager.TrackEvent("xamleditor", "edited", CurrentSample.Name, secondsEdditingSample);
                }
                else
                {
                    TrackingManager.TrackEvent("xamleditor", "not_edited", CurrentSample.Name);
                }
            }

            XamlCodeEditor.ResetTimer();
        }

        private void WindowStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            switch (e.NewState.Name)
            {
                case nameof(NarrowState):
                    if (CanChangePaneState)
                    {
                        SidePaneState = PaneState.Closed;
                    }

                    // Update Read-Only XAML tab when switching back to show changes to TwoWay Bound Properties
                    if (CurrentSample?.HasXAMLCode == true && InfoAreaPivot.SelectedItem == XamlReadOnlyPivotItem)
                    {
                        XamlReadOnlyCodeRenderer.SetCode(CurrentSample.XamlTemplate.XamlWithValues, "xaml");
                    }

                    break;

                case nameof(WideState):
                    if (CanChangePaneState)
                    {
                        SidePaneState = PaneState.Normal;
                    }

                    break;
            }
        }

        private void PaneStates_CurrentStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.OldState?.Name == nameof(Full) && WindowStates.CurrentState?.Name == nameof(NarrowState))
            {
                // Restart the State, full state changed things.
                VisualStateManager.GoToState(this, NarrowState.Name, false);
            }
        }

        private void Current_ThemeChanged(object sender, ThemeChangedArgs e)
        {
            if (e.CustomSet)
            {
                DocumentationTextblock.RequestedTheme = e.Theme;
            }
        }

        private void ThemePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Shell.Current.SetCurrentTheme((ElementTheme)ThemePicker.SelectedIndex);
            }
            catch (Exception ex)
            {
                ExceptionNotification.Show(ex.Message);
            }
        }

        private async void SamplePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.FirstOrDefault() is Sample sample)
            {
                _onlyDocumentation = true;

                // TODO: Have standard blank page for samples without extra needs.
                if (sample.Type != null)
                {
                    try
                    {
                        var pageInstance = Activator.CreateInstance(sample.Type);
                        SampleContent.Content = pageInstance;
                    }
                    catch
                    {
                        ExceptionNotification.Show("Sample Page failed to load.");
                    }
                }
                else
                {
                    // If we didn't specify a specific page, just create a placeholder to hold content.
                    SampleContent.Content = new Page();
                }

                if (SamplePage != null)
                {
                    SamplePage.Loaded += SamplePage_Loaded;
                }

                DataContext = sample;

                await SampleLoader.PushRecentSample(SampleSuite);

                var propertyDesc = sample?.XamlTemplate?.Properties;

                InfoAreaPivot.Items.Clear();

                if (propertyDesc != null)
                {
                    _xamlRenderer.DataContext = propertyDesc.Expando;
                }

                if (propertyDesc != null && propertyDesc.Options.Count > 0)
                {
                    InfoAreaPivot.Items.Add(PropertiesPivotItem);
                }

                if (sample.HasXAMLCode && sample.XamlTemplate != null)
                {
                    if (AnalyticsInfo.VersionInfo.GetDeviceFormFactor() != DeviceFormFactor.Desktop || sample.DisableXamlEditorRendering)
                    {
                        // Only makes sense (and works) for now to show Live Xaml on Desktop, so fallback to old system here otherwise.
                        XamlReadOnlyCodeRenderer.SetCode(sample.XamlTemplate.XamlWithValues, "xaml");

                        InfoAreaPivot.Items.Add(XamlReadOnlyPivotItem);
                    }
                    else
                    {
                        XamlCodeEditor.Text = sample.XamlTemplate.XamlWithValues;

                        InfoAreaPivot.Items.Add(XamlPivotItem);

                        _xamlCodeRendererSupported = true;

                        await UpdateXamlRenderAsync(sample.XamlTemplate.XamlWithBindings);
                    }

                    InfoAreaPivot.SelectedIndex = 0;
                    _onlyDocumentation = false;
                }

                if (sample.HasCSharpCode)
                {
                    var code = await sample.GetCSharpSourceAsync();

                    CSharpCodeRenderer.SetCode(code, "c#");
                    InfoAreaPivot.Items.Add(CSharpPivotItem);
                    _onlyDocumentation = false;
                }

                if (sample.HasJavaScriptCode)
                {
                    var code = await sample.GetJavaScriptSourceAsync();

                    JavaScriptCodeRenderer.SetCode(code, "js");
                    InfoAreaPivot.Items.Add(JavaScriptPivotItem);
                    _onlyDocumentation = false;
                }

                if (SampleSuite.HasDocumentation)
                {
                    var (contents, path) = await SampleSuite.GetDocumentationAsync();
                    documentationPath = path;
                    if (!string.IsNullOrWhiteSpace(contents))
                    {
                        DocumentationTextblock.Text = contents;
                        InfoAreaPivot.Items.Add(DocumentationPivotItem);
                    }
                }

                // Hide the Github button if there isn't a CodeUrl.
                if (string.IsNullOrEmpty(SampleSuite.CodeUrl))
                {
                    GithubButton.Visibility = Visibility.Collapsed;
                }

                if (InfoAreaPivot.Items.Count == 0)
                {
                    SidePaneState = PaneState.None;
                    _hasDocumentation = false;
                }
                else
                {
                    SidePaneState = _onlyDocumentation ? PaneState.Full : PaneState.Normal;
                }

                Shell.Current.SetTitles($"{SampleSuite.Category} > {SampleSuite.Name} > {sample.Name}");
            }
            else
            {
                ExceptionNotification.Show("Sample does not exist");
            }

            if (!CanChangePaneState)
            {
                SampleTitleBar.Children.Remove(NarrowInfoButton);
                PaneStates.States.Clear();
                WindowStates.States.Clear();
            }
        }
    }
}