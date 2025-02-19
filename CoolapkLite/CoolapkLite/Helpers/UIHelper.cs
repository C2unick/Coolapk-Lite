﻿using CoolapkLite.Models.Images;
using CoolapkLite.Pages;
using CoolapkLite.Pages.BrowserPages;
using CoolapkLite.Pages.FeedPages;
using CoolapkLite.Pages.SettingsPages;
using CoolapkLite.ViewModels.BrowserPages;
using CoolapkLite.ViewModels.FeedPages;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkLite.Helpers
{
    internal static partial class UIHelper
    {
        public const int Duration = 3000;
        public static bool IsShowingProgressBar, IsShowingMessage;
        public static bool HasTitleBar => !CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
        public static bool HasStatusBar => ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");

        private static CoreDispatcher shellDispatcher;
        public static CoreDispatcher ShellDispatcher
        {
            get => shellDispatcher;
            set
            {
                if (shellDispatcher == null)
                {
                    shellDispatcher = value;
                }
            }
        }

        private static readonly List<string> MessageList = new List<string>();
    }

    internal static partial class UIHelper
    {
        public static IHaveTitleBar AppTitle;

        public static async void ShowProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.HideProgressBar());
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = null;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.ShowProgressBar());
            }
        }

        public static async void ShowProgressBar(double value)
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.HideProgressBar());
                StatusBar.GetForCurrentView().ProgressIndicator.ProgressValue = value * 0.01;
                await StatusBar.GetForCurrentView().ProgressIndicator.ShowAsync();
            }
            else
            {
                await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.ShowProgressBar(value));
            }
        }

        public static async void PausedProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.PausedProgressBar());
        }

        public static async void ErrorProgressBar()
        {
            IsShowingProgressBar = true;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.ErrorProgressBar());
        }

        public static async void HideProgressBar()
        {
            IsShowingProgressBar = false;
            if (HasStatusBar)
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
            }
            await AppTitle?.Dispatcher.AwaitableRunAsync(() => AppTitle?.HideProgressBar());
        }

        public static async void ShowMessage(string message)
        {
            MessageList.Add(message);
            if (!IsShowingMessage)
            {
                IsShowingMessage = true;
                while (MessageList.Count > 0)
                {
                    if (HasStatusBar)
                    {
                        StatusBar statusBar = StatusBar.GetForCurrentView();
                        if (!string.IsNullOrEmpty(MessageList[0]))
                        {
                            statusBar.ProgressIndicator.Text = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                            statusBar.ProgressIndicator.ProgressValue = IsShowingProgressBar ? null : (double?)0;
                            await statusBar.ProgressIndicator.ShowAsync();
                            await Task.Delay(3000);
                        }
                        MessageList.RemoveAt(0);
                        if (MessageList.Count == 0 && !IsShowingProgressBar) { await statusBar.ProgressIndicator.HideAsync(); }
                        statusBar.ProgressIndicator.Text = string.Empty;
                    }
                    else
                    {
                        await AppTitle?.Dispatcher.AwaitableRunAsync(async () =>
                        {
                            if (AppTitle != null)
                            {
                                if (!string.IsNullOrEmpty(MessageList[0]))
                                {
                                    string messages = $"[{MessageList.Count}] {MessageList[0].Replace("\n", " ")}";
                                    AppTitle.ShowMessage(messages);
                                    await Task.Delay(3000);
                                }
                                MessageList.RemoveAt(0);
                                if (MessageList.Count == 0)
                                {
                                    AppTitle.ShowMessage();
                                }
                            }
                        });
                    }
                }
                IsShowingMessage = false;
            }
        }

        public static void ShowInAppMessage(MessageType type, string message = null)
        {
            switch (type)
            {
                case MessageType.Message:
                    ShowMessage(message);
                    break;
                default:
                    ShowMessage(type.ConvertMessageTypeToMessage());
                    break;
            }
        }

        public static void ShowHttpExceptionMessage(HttpRequestException e)
        {
            if (e.Message.IndexOfAny(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' }) != -1)
            { ShowInAppMessage(MessageType.Message, $"服务器错误： {e.Message.Replace("Response status code does not indicate success: ", string.Empty)}"); }
            else if (e.Message == "An error occurred while sending the request.") { ShowInAppMessage(MessageType.Message, "无法连接网络。"); }
            else { ShowInAppMessage(MessageType.Message, $"请检查网络连接。 {e.Message}"); }
        }

        public static bool IsOriginSource(object source, object originalSource)
        {
            if (source == originalSource) { return true; }

            bool result = false;
            FrameworkElement DependencyObject = originalSource as FrameworkElement;
            if (DependencyObject.FindAscendant<ButtonBase>() == null && !(originalSource is ButtonBase) && !(originalSource is RichEditBox))
            {
                if (source is FrameworkElement FrameworkElement)
                {
                    result = source == DependencyObject.FindAscendant(FrameworkElement.Name);
                }
            }

            return DependencyObject.Tag == null && result;
        }

        public static string ConvertMessageTypeToMessage(this MessageType type)
        {
            switch (type)
            {
                case MessageType.NoMore:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMore");

                case MessageType.NoMoreShare:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreShare");

                case MessageType.NoMoreReply:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreReply");

                case MessageType.NoMoreHotReply:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreHotReply");

                case MessageType.NoMoreLikeUser:
                    return ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("NoMoreLikeUser");

                default: return string.Empty;
            }
        }

        public static void SetBadgeNumber(string badgeGlyphValue)
        {
            // Get the blank badge XML payload for a badge number
            XmlDocument badgeXml =
                BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            // Set the value of the badge in the XML to our number
            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", badgeGlyphValue);
            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(badgeXml);
            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            // And update the badge
            badgeUpdater.Update(badge);
        }

        public static string ExceptionToMessage(this Exception ex)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append('\n');
            if (!string.IsNullOrWhiteSpace(ex.Message)) { builder.AppendLine($"Message: {ex.Message}"); }
            builder.AppendLine($"HResult: {ex.HResult} (0x{Convert.ToString(ex.HResult, 16)})");
            if (!string.IsNullOrWhiteSpace(ex.StackTrace)) { builder.AppendLine(ex.StackTrace); }
            if (!string.IsNullOrWhiteSpace(ex.HelpLink)) { builder.Append($"HelperLink: {ex.HelpLink}"); }
            return builder.ToString();
        }

        public static TResult AwaitByTaskCompleteSource<TResult>(Func<Task<TResult>> function, CancellationToken cancellationToken = default)
        {
            TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
            Task<TResult> task = taskCompletionSource.Task;
            _ = Task.Run(async () =>
            {
                try
                {
                    TResult result = await function.Invoke().ConfigureAwait(false);
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception e)
                {
                    taskCompletionSource.SetException(e);
                }
            }, cancellationToken);
            TResult taskResult = task.Result;
            return taskResult;
        }

        public static bool IsTypePresent(string AssemblyName, string TypeName)
        {
            try
            {
                Assembly asmb = Assembly.Load(new AssemblyName(AssemblyName));
                Type supType = asmb.GetType($"{AssemblyName}.{TypeName}");
                if (supType != null)
                {
                    try { Activator.CreateInstance(supType); }
                    catch (MissingMethodException) { }
                }
                return supType != null;
            }
            catch
            {
                return false;
            }
        }
    }

    public enum NavigationThemeTransition
    {
        Default,
        Entrance,
        DrillIn,
        Suppress
    }

    internal static partial class UIHelper
    {
        public static void Navigate(Type pageType, object e = null, NavigationThemeTransition Type = NavigationThemeTransition.Default)
        {
            switch (Type)
            {
                case NavigationThemeTransition.DrillIn:
                    _ = AppTitle?.MainFrame.Navigate(pageType, e, new DrillInNavigationTransitionInfo());
                    break;
                case NavigationThemeTransition.Entrance:
                    _ = AppTitle?.MainFrame.Navigate(pageType, e, new EntranceNavigationTransitionInfo());
                    break;
                case NavigationThemeTransition.Suppress:
                    _ = AppTitle?.MainFrame.Navigate(pageType, e, new SuppressNavigationTransitionInfo());
                    break;
                case NavigationThemeTransition.Default:
                    _ = AppTitle?.MainFrame.Navigate(pageType, e);
                    break;
                default:
                    _ = AppTitle?.MainFrame.Navigate(pageType, e);
                    break;
            }
        }

        public static async Task ShowImageAsync(ImageModel image)
        {
            if (SettingsHelper.Get<bool>(SettingsHelper.IsUseMultiWindow) && WindowHelper.IsSupportedAppWindow)
            {
                (AppWindow window, Frame frame) = await WindowHelper.CreateWindow();
                window.TitleBar.ExtendsContentIntoTitleBar = true;
                ThemeHelper.Initialize();
                frame.Navigate(typeof(ShowImagePage), image);
                await window.TryShowAsync();
            }
            else
            {
                ((Page)AppTitle).Frame.Navigate(typeof(ShowImagePage), image);
            }
        }
    }

    internal static partial class UIHelper
    {
        public static async Task<bool> OpenLinkAsync(string link)
        {
            if (string.IsNullOrWhiteSpace(link)) { return false; }

            string origin = link;

            if (link.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Replace("http://", string.Empty).Replace("https://", string.Empty);
                if (link.StartsWith("image.coolapk.com"))
                {
                    _ = ShowImageAsync(new ImageModel(origin, ImageType.SmallImage));
                    return true;
                }
                else
                {
                    Regex coolapk = new Regex(@"\w*?.?coolapk.\w*/");
                    if (coolapk.IsMatch(link))
                    {
                        link = coolapk.Replace(link, string.Empty);
                    }
                    else
                    {
                        Navigate(typeof(BrowserPage), new BrowserViewModel(origin));
                        return true;
                    }
                }
            }
            else if (link.StartsWith("coolapk://", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Substring(10);
            }
            else if (link.StartsWith("coolmarket://", StringComparison.OrdinalIgnoreCase))
            {
                link = link.Substring(13);
            }

            if (link.FirstOrDefault() != '/')
            {
                link = $"/{link}";
            }

            if (link == "/contacts/fans")
            {
                Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), false, "我"));
            }
            else if (link == "/user/myFollowList")
            {
                Navigate(typeof(AdaptivePage), AdaptiveViewModel.GetUserListProvider(SettingsHelper.Get<string>(SettingsHelper.Uid), true, "我"));
            }
            else if (link.StartsWith("/page?", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(6);
                Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
            else if (link.StartsWith("/u/", StringComparison.OrdinalIgnoreCase))
            {
                string url = link.Substring(3, "?");
                string uid = int.TryParse(url, out _) ? url : (await NetworkHelper.GetUserInfoByNameAsync(url)).UID;
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.UserPageList, uid);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/feed/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(6, "?");
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
                else
                {
                    ShowMessage("暂不支持");
                }
            }
            else if (link.StartsWith("/picture/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new FeedDetailViewModel(id));
                }
            }
            else if (link.StartsWith("/question/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(10, "?");
                if (int.TryParse(id, out _))
                {
                    Navigate(typeof(FeedShellPage), new QuestionViewModel(id));
                }
            }
            else if (link.StartsWith("/t/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(3, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.TagPageList, tag);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/dyh/", StringComparison.OrdinalIgnoreCase))
            {
                string tag = link.Substring(5, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.DyhPageList, tag);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/product/", StringComparison.OrdinalIgnoreCase))
            {
                if (link.StartsWith("/product/categoryList", StringComparison.OrdinalIgnoreCase))
                {
                    Navigate(typeof(AdaptivePage), new AdaptiveViewModel(link));
                }
                else
                {
                    string tag = link.Substring(9, "?");
                    FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, tag);
                    if (provider != null)
                    {
                        Navigate(typeof(FeedListPage), provider);
                    }
                }
            }
            else if (link.StartsWith("/collection/", StringComparison.OrdinalIgnoreCase))
            {
                string id = link.Substring(12, "?");
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.CollectionPageList, id);
                if (provider != null)
                {
                    Navigate(typeof(FeedListPage), provider);
                }
            }
            else if (link.StartsWith("/mp/", StringComparison.OrdinalIgnoreCase))
            {
                Navigate(typeof(HTMLPage), new HTMLViewModel(origin));
            }
            else if (origin.StartsWith("http://") || link.StartsWith("https://"))
            {
                Navigate(typeof(BrowserPage), new BrowserViewModel(origin));
            }
            else
            {
                return origin.Contains("://") && await Launcher.LaunchUriAsync(origin.ValidateAndGetUri());
            }

            return true;
        }

        public static async Task<bool> OpenActivatedEventArgs(IActivatedEventArgs args)
        {
            switch (args.Kind)
            {
                case ActivationKind.Launch:
                    LaunchActivatedEventArgs LaunchActivatedEventArgs = (LaunchActivatedEventArgs)args;
                    if (!string.IsNullOrWhiteSpace(LaunchActivatedEventArgs.Arguments))
                    {
                        switch (LaunchActivatedEventArgs.Arguments)
                        {
                            case "settings":
                                Navigate(typeof(SettingsPage));
                                break;
                            case "flags":
                                Navigate(typeof(TestPage));
                                break;
                            default:
                                return await OpenLinkAsync(LaunchActivatedEventArgs.Arguments);
                        }
                    }
                    else if (ApiInformation.IsTypePresent("Windows.ApplicationModel.Activation.ILaunchActivatedEventArgs2")
                            && LaunchActivatedEventArgs.TileActivatedInfo != null)
                    {
                        if (LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.Any())
                        {
                            string TileArguments = LaunchActivatedEventArgs.TileActivatedInfo.RecentlyShownNotifications.FirstOrDefault().Arguments;
                            return !string.IsNullOrWhiteSpace(LaunchActivatedEventArgs.Arguments) && await OpenLinkAsync(TileArguments);
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                    break;
                case ActivationKind.Protocol:
                    IProtocolActivatedEventArgs ProtocolActivatedEventArgs = (IProtocolActivatedEventArgs)args;
                    switch (ProtocolActivatedEventArgs.Uri.Host)
                    {
                        case "www.coolapk.com":
                        case "coolapk.com":
                        case "www.coolmarket.com":
                        case "coolmarket.com":
                            return await OpenLinkAsync(ProtocolActivatedEventArgs.Uri.AbsolutePath);
                        case "http":
                        case "https":
                            return await OpenLinkAsync($"{ProtocolActivatedEventArgs.Uri.Host}:{ProtocolActivatedEventArgs.Uri.AbsolutePath}");
                        case "settings":
                            Navigate(typeof(SettingsPage));
                            break;
                        case "flags":
                            Navigate(typeof(TestPage));
                            break;
                        default:
                            return await OpenLinkAsync(ProtocolActivatedEventArgs.Uri.AbsoluteUri);
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        private static string Substring(this string str, int startIndex, string endString)
        {
            int end = str.IndexOf(endString);
            return end > startIndex ? str.Substring(startIndex, end - startIndex) : str.Substring(startIndex);
        }
    }

    public enum MessageType
    {
        Message,
        NoMore,
        NoMoreReply,
        NoMoreLikeUser,
        NoMoreShare,
        NoMoreHotReply,
    }
}
