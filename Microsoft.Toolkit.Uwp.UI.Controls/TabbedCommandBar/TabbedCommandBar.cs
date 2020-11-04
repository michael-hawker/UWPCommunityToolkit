// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Security.Authentication.OnlineId;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Animation;

namespace Microsoft.Toolkit.Uwp.UI.Controls
{
    /// <summary>
    /// A basic ribbon control that houses <see cref="TabbedCommandBarItem"/>s
    /// </summary>
    [ContentProperty(Name = nameof(MenuItems))]
    [TemplatePart(Name = "PART_RibbonContent", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_TabChangedStoryboard", Type = typeof(Storyboard))]
    public class TabbedCommandBar : NavigationView
    {
        private ContentControl _ribbonContent = null;
        private Storyboard _tabChangedStoryboard = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabbedCommandBar"/> class.
        /// </summary>
        public TabbedCommandBar()
            : base()
        {
            DefaultStyleKey = typeof(TabbedCommandBar);

            SelectionChanged += RibbonNavigationView_SelectionChanged;
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            if (_ribbonContent != null)
            {
                _ribbonContent.Content = null;
            }

            // Get RibbonContent first, since setting SelectedItem requires it
            _ribbonContent = GetTemplateChild("PART_RibbonContent") as ContentControl;

            SelectedItem = MenuItems.FirstOrDefault();

            _tabChangedStoryboard = GetTemplateChild(nameof(_tabChangedStoryboard)) as Storyboard;
        }

        private void RibbonNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is TabbedCommandBarItem item)
            {
                _ribbonContent.Content = item;
                _tabChangedStoryboard?.Begin();
            }
            else if (args.SelectedItem is NavigationViewItem navItem)
            {
                // This code is a hack and is only temporary, because I can't get binding to work.
                // RibbonContent might be null here, there should be a check
                _ribbonContent.Content = MenuItems[System.Math.Min(MenuItems.Count - 1, MenuItems.IndexOf(navItem))];
            }
        }
    }
}