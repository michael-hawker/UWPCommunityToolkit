using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TabTestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public bool IsFullScreen {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFullScreen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register(nameof(IsFullScreen), typeof(bool), typeof(MainPage), new PropertyMetadata(false, IsFullScreenPropertyChanged));

        public MainPage()
        {
            this.InitializeComponent();

            // Hide default title bar.
            // https://docs.microsoft.com/en-us/windows/uwp/design/shell/title-bar
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            // Register for changes
            coreTitleBar.LayoutMetricsChanged += this.CoreTitleBar_LayoutMetricsChanged;
            CoreTitleBar_LayoutMetricsChanged(coreTitleBar, null);

            coreTitleBar.IsVisibleChanged += this.CoreTitleBar_IsVisibleChanged;

            // Set XAML element as draggable region.
            Window.Current.SetTitleBar(AppTitleBar);

            // Listen for Fullscreen Changes from Shift+Win+Enter or our F11 shortcut
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += this.MainPage_VisibleBoundsChanged;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Adjust our content based on the Titlebar's visibility
            TabView.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
            AppTitleBar.Visibility = TabView.Visibility;
        }

        private void MainPage_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            // Update Fullscreen from other modes of adjusting view (keyboard shortcuts)
            IsFullScreen = ApplicationView.GetForCurrentView().IsFullScreenMode;
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            // Get the size of the caption controls area and back button 
            // (returned in logical pixels), and move your content around as necessary.
            LeftPaddingColumn.Width = new GridLength(sender.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(sender.SystemOverlayRightInset);

            // Update title bar control size as needed to account for system size changes.
            AppTitleBar.Height = sender.Height;
        }

        private void Button_FullScreen_Click(object sender, RoutedEventArgs e)
        {
            // Toggle FullScreen manually
            if (!IsFullScreen)
            {
                IsFullScreen = ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            }
            else
            {
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
                IsFullScreen = false;
            }
        }

        private static void IsFullScreenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            // When we change, make some layout changes so we can overlay TabView on top of Content.
            if (sender is MainPage mainpage)
            {
                if (args.NewValue as bool? == true)
                {
                    Grid.SetRowSpan(mainpage.ContentPresenter, 2);
                    Grid.SetRow(mainpage.ContentPresenter, 0);
                }
                else
                {
                    Grid.SetRowSpan(mainpage.ContentPresenter, 1);
                    Grid.SetRow(mainpage.ContentPresenter, 1);
                }
            }
        }
    }
}
