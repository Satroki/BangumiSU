﻿using BangumiSU.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Notifications;
using BangumiSU.Pages;

namespace BangumiSU.SharedCode
{
    public static class NavigationHelper
    {
        public static void Navigate(Type type, object para = null)
        {
            var frame = Window.Current.Content as Frame;
            frame?.Navigate(type, para);
        }

        public static void Navigate<T>(object para = null)
        {
            var frame = Window.Current.Content as Frame;
            frame?.Navigate(typeof(T), para);
        }

        public static void NavigateToWeb(string uri)
        {
            Navigate<WebPage>(uri);
        }
    }

    public static class FolderHelper
    {
        public static async Task<StorageFolder> GetFolder(string token)
        {
            try
            {
                var access = StorageApplicationPermissions.FutureAccessList.ContainsItem(token);
                if (access)
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                return null;
            }
            catch { return null; }
        }

        public static async Task<StorageFolder> PickFolder(string token)
        {
            var fp = new FolderPicker();
            fp.FileTypeFilter.Add("*");
            fp.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            var folder = await fp.PickSingleFolderAsync();
            if (folder != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(token, folder);
                return folder;
            }
            return null;
        }
    }

    public static class ImageHelper
    {
        public static async Task<ImageSource> GetImage(Bangumi b)
        {
            try
            {
                var img = await GetImageById(b.Id.ToString());
                img = img ?? await GetRemoteImage(b);
                b.Cover = img;
                return img;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<ImageSource> GetImageById(string id)
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
            var file = await folder.TryGetItemAsync($"{id}.jpg");

            if (file == null)
                return null;

            var img = (StorageFile)file;
            using (var stream = await img.OpenReadAsync())
            {
                var bmp = new BitmapImage();
                await bmp.SetSourceAsync(stream);
                return bmp;
            }
        }

        public static async Task<ImageSource> GetRemoteImage(Bangumi b)
        {
            if (b.ImageUri.IsEmpty())
                return null;

            var img = await AppCache.IClient.GetImage(b.ImageUri);
            var bmp = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(img.Data.AsBuffer());
                stream.Seek(0);
                await bmp.SetSourceAsync(stream);
            }
            await SaveImage(img.Data, $"{b.Id}.jpg");
            return bmp;
        }

        public static async Task SaveImage(byte[] data, string name)
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("Covers", CreationCollisionOption.OpenIfExists);
            var file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            using (var s = await file.OpenStreamForWriteAsync())
            {
                await s.WriteAsync(data, 0, data.Length);
                await s.FlushAsync();
            }
        }

        public static async Task DeleteImages()
        {
            var folder = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync("Covers") as StorageFolder;
            if (folder != null)
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
        }
    }

    public static class ToastHelper
    {
        public static void Toast(string msg)
        {
            var tpl = ToastTemplateType.ToastText02;
            var xml = ToastNotificationManager.GetTemplateContent(tpl);
            var txt = xml.GetElementsByTagName("text");
            txt[0].AppendChild(xml.CreateTextNode(msg));

            var no = new ToastNotification(xml);
            ToastNotificationManager.CreateToastNotifier().Show(no);
        }
        /*
         <toast>
    <visual>
        <binding template="ToastText02">
            <text id="1">headlineText</text>
            <text id="2">bodyText</text>
        </binding>  
    </visual>
</toast>*/
    }
}
