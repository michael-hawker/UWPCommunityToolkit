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
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Uwp.SampleApp.Models;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Microsoft.Toolkit.Uwp.SampleApp.SamplePages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PivotStylesPage : Page, IXamlRenderListener
    {
        private Pivot _mainPivot;

        public PivotStylesPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Shell.Current.RegisterNewCommand("Add Tab", AddTabClick);
        }

        public void OnXamlRendered(FrameworkElement control)
        {
            _mainPivot = control.FindChildByName("DocumentTabs") as Pivot;
        }

        private void AddTabClick(object sender, RoutedEventArgs e)
        {
            if (_mainPivot != null)
            {
                var pi = new PivotItem()
                {
                    Header = "Test",
                    Content = "Some test content..."
                };

                _mainPivot.Items.Add(pi);
            }
        }
    }

    #pragma warning disable SA1402 // File may only contain a single class
    internal class CloseCommand : ICommand
    #pragma warning restore SA1402 // File may only contain a single class
    {
        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
            }

            remove
            {
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            var pi = parameter as PivotItem;

            var md = new MessageDialog(string.Format("Are you sure you want to close '{0}'?", pi?.Header), "Close Tab?");
            var yes = new UICommand("Yes");
            var no = new UICommand("No");

            md.Commands.Add(yes);
            md.Commands.Add(no);

            md.DefaultCommandIndex = 1;
            md.CancelCommandIndex = 1;

            var result = await md.ShowAsync();

            if (result == yes)
            {
                var pivot = pi.FindAscendant<Pivot>();

                pivot.Items.Remove(pi);
            }
        }
    }
}
