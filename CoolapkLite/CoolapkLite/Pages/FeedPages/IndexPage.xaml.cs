﻿using CoolapkLite.Helpers;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkLite.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IndexPage : Page
    {
        private ViewModels.IndexPage.ViewModel Provider;

        private Thickness StackPanelMargin => UIHelper.StackPanelMargin;

        public IndexPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            TitleBar.Title = ResourceLoader.GetForCurrentView("MainPage").GetString("Home");
            if (e.Parameter is ViewModels.IndexPage.ViewModel ViewModel)
            {
                Provider = ViewModel;
                await Refresh(-2);
                if (!string.IsNullOrEmpty(Provider.Title))
                {
                    TitleBar.Title = Provider.Title;
                }
            }
        }

        private async Task Refresh(int p = -1)
        {
            await Provider.Refresh(p);
        }

        private void TitleBar_RefreshEvent(object sender, RoutedEventArgs e) => _ = Refresh(-2);

        private async void ListView_RefreshRequested(object sender, System.EventArgs e) => await Refresh(-2);
    }
}