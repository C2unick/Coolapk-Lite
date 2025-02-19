﻿using CoolapkLite.Models;
using CoolapkLite.Models.Update;
using MetroLog;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using IObjectSerializer = Microsoft.Toolkit.Helpers.IObjectSerializer;

namespace CoolapkLite.Helpers
{
    internal static partial class SettingsHelper
    {
        public const string Uid = nameof(Uid);
        public const string Token = nameof(Token);
        public const string TileUrl = nameof(TileUrl);
        public const string UserName = nameof(UserName);
        public const string CustomUA = nameof(CustomUA);
        public const string Bookmark = nameof(Bookmark);
        public const string IsUseAPI2 = nameof(IsUseAPI2);
        public const string CustomAPI = nameof(CustomAPI);
        public const string IsFirstRun = nameof(IsFirstRun);
        public const string IsCustomUA = nameof(IsCustomUA);
        public const string APIVersion = nameof(APIVersion);
        public const string IsNoPicsMode = nameof(IsNoPicsMode);
        public const string TokenVersion = nameof(TokenVersion);
        public const string IsUseLiteHome = nameof(IsUseLiteHome);
        public const string IsUseCompositor = nameof(IsUseCompositor);
        public const string CurrentLanguage = nameof(CurrentLanguage);
        public const string IsUseMultiWindow = nameof(IsUseMultiWindow);
        public const string SelectedAppTheme = nameof(SelectedAppTheme);
        public const string IsUseOldEmojiMode = nameof(IsUseOldEmojiMode);
        public const string ShowOtherException = nameof(ShowOtherException);
        public const string SemaphoreSlimCount = nameof(SemaphoreSlimCount);
        public const string IsDisplayOriginPicture = nameof(IsDisplayOriginPicture);
        public const string CheckUpdateWhenLuanching = nameof(CheckUpdateWhenLuanching);

        public static Type Get<Type>(string key) => LocalObject.Read<Type>(key);
        public static void Set<Type>(string key, Type value) => LocalObject.Save(key, value);
        public static Task<Type> GetFile<Type>(string key) => LocalObject.ReadFileAsync<Type>($"Settings/{key}");
        public static Task SetFile<Type>(string key, Type value) => LocalObject.CreateFileAsync($"Settings/{key}", value);

        public static void SetDefaultSettings()
        {
            if (!LocalObject.KeyExists(Uid))
            {
                LocalObject.Save(Uid, string.Empty);
            }
            if (!LocalObject.KeyExists(Token))
            {
                LocalObject.Save(Token, string.Empty);
            }
            if (!LocalObject.KeyExists(TileUrl))
            {
                LocalObject.Save(TileUrl, "https://api.coolapk.com/v6/page/dataList?url=V9_HOME_TAB_FOLLOW&type=circle");
            }
            if (!LocalObject.KeyExists(UserName))
            {
                LocalObject.Save(UserName, string.Empty);
            }
            if (!LocalObject.KeyExists(CustomUA))
            {
                LocalObject.Save(CustomUA, UserAgent.Parse(NetworkHelper.Client.DefaultRequestHeaders.UserAgent.ToString()));
            }
            if (!LocalObject.KeyExists(IsUseAPI2))
            {
                LocalObject.Save(IsUseAPI2, true);
            }
            if (!LocalObject.KeyExists(CustomAPI))
            {
                LocalObject.Save(CustomAPI, new APIVersion("9.2.2", "1905301"));
            }
            if (!LocalObject.KeyExists(IsFirstRun))
            {
                LocalObject.Save(IsFirstRun, true);
            }
            if (!LocalObject.KeyExists(IsCustomUA))
            {
                LocalObject.Save(IsCustomUA, false);
            }
            if (!LocalObject.KeyExists(APIVersion))
            {
                LocalObject.Save(APIVersion, Common.APIVersions.V13);
            }
            if (!LocalObject.KeyExists(IsNoPicsMode))
            {
                LocalObject.Save(IsNoPicsMode, false);
            }
            if (!LocalObject.KeyExists(TokenVersion))
            {
                LocalObject.Save(TokenVersion, Common.TokenVersions.TokenV2);
            }
            if (!LocalObject.KeyExists(IsUseLiteHome))
            {
                LocalObject.Save(IsUseLiteHome, false);
            }
            if (!LocalObject.KeyExists(IsUseCompositor))
            {
                LocalObject.Save(IsUseCompositor, true);
            }
            if (!LocalObject.KeyExists(CurrentLanguage))
            {
                LocalObject.Save(CurrentLanguage, LanguageHelper.AutoLanguageCode);
            }
            if (!LocalObject.KeyExists(IsUseMultiWindow))
            {
                LocalObject.Save(IsUseMultiWindow, true);
            }
            if (!LocalObject.KeyExists(SelectedAppTheme))
            {
                LocalObject.Save(SelectedAppTheme, ElementTheme.Default);
            }
            if (!LocalObject.KeyExists(IsUseOldEmojiMode))
            {
                LocalObject.Save(IsUseOldEmojiMode, false);
            }
            if (!LocalObject.KeyExists(ShowOtherException))
            {
                LocalObject.Save(ShowOtherException, true);
            }
            if (!LocalObject.KeyExists(SemaphoreSlimCount))
            {
                LocalObject.Save(SemaphoreSlimCount, Environment.ProcessorCount);
            }
            if (!LocalObject.KeyExists(IsDisplayOriginPicture))
            {
                LocalObject.Save(IsDisplayOriginPicture, false);
            }
            if (!LocalObject.KeyExists(CheckUpdateWhenLuanching))
            {
                LocalObject.Save(CheckUpdateWhenLuanching, true);
            }
            SetDefaultFileSettings();
        }

        public static async void SetDefaultFileSettings()
        {
            StorageFolder folder = LocalObject.Folder;
            StorageFolder settings = await folder.CreateFolderAsync("Settings", CreationCollisionOption.OpenIfExists);
            if (await settings.TryGetItemAsync(Bookmark) == null)
            {
                await SetFile(Bookmark, Models.Bookmark.GetDefaultBookmarks());
            }
        }
    }

    internal static partial class SettingsHelper
    {
        public static event TypedEventHandler<string, bool> LoginChanged;
        public static readonly ILogManager LogManager = LogManagerFactory.CreateLogManager();
        public static readonly ApplicationDataStorageHelper LocalObject = ApplicationDataStorageHelper.GetCurrent(new SystemTextJsonObjectSerializer());

        static SettingsHelper() => SetDefaultSettings();

        public static void InvokeLoginChanged(string sender, bool args) => LoginChanged?.Invoke(sender, args);

        public static async Task<bool> Login()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.CoolapkUri))
                {
                    switch (item.Name)
                    {
                        case "uid":
                            uid = item.Value;
                            break;
                        case "username":
                            userName = item.Value;
                            break;
                        case "token":
                            token = item.Value;
                            break;
                        default:
                            break;
                    }
                }
                if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userName) || !await RequestHelper.CheckLogin())
                {
                    Logout();
                    return false;
                }
                else
                {
                    Set(Uid, uid);
                    Set(Token, token);
                    Set(UserName, userName);
                    InvokeLoginChanged(uid, true);
                    return true;
                }
            }
        }

        public static async Task<bool> Login(string Uid, string UserName, string Token)
        {
            if (!string.IsNullOrEmpty(Uid) && !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Token))
            {
                using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
                {
                    HttpCookieManager cookieManager = filter.CookieManager;
                    HttpCookie uid = new HttpCookie("uid", ".coolapk.com", "/");
                    HttpCookie username = new HttpCookie("username", ".coolapk.com", "/");
                    HttpCookie token = new HttpCookie("token", ".coolapk.com", "/");
                    uid.Value = Uid;
                    username.Value = UserName;
                    token.Value = Token;
                    cookieManager.SetCookie(uid);
                    cookieManager.SetCookie(username);
                    cookieManager.SetCookie(token);
                }
                if (await RequestHelper.CheckLogin())
                {
                    Set(SettingsHelper.Uid, Uid);
                    Set(SettingsHelper.Token, Token);
                    Set(SettingsHelper.UserName, UserName);
                    InvokeLoginChanged(Uid, true);
                    return true;
                }
                else
                {
                    Logout();
                    return false;
                }
            }
            return false;
        }

        public static async Task<bool> CheckLoginAsync()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                string uid = string.Empty, token = string.Empty, userName = string.Empty;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.CoolapkUri))
                {
                    switch (item.Name)
                    {
                        case "uid":
                            uid = item.Value;
                            break;
                        case "username":
                            userName = item.Value;
                            break;
                        case "token":
                            token = item.Value;
                            break;
                        default:
                            break;
                    }
                }
                return !string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userName) && await RequestHelper.CheckLogin();
            }
        }

        public static void Logout()
        {
            using (HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter())
            {
                HttpCookieManager cookieManager = filter.CookieManager;
                foreach (HttpCookie item in cookieManager.GetCookies(UriHelper.Base2Uri))
                {
                    cookieManager.DeleteCookie(item);
                }
            }
            Set(Uid, string.Empty);
            Set(Token, string.Empty);
            Set(UserName, string.Empty);
            InvokeLoginChanged(string.Empty, false);
        }
    }

    public class SystemTextJsonObjectSerializer : IObjectSerializer
    {
        // Specify your serialization settings
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings() { DefaultValueHandling = DefaultValueHandling.Ignore };

        string IObjectSerializer.Serialize<T>(T value) => JsonConvert.SerializeObject(value, typeof(T), Formatting.Indented, settings);

        public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, settings);
    }
}
