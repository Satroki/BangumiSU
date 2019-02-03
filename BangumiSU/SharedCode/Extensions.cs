using BangumiSU.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace BangumiSU.SharedCode
{
    public static class Extensions
    {
        public static bool IsEmpty(this string str) => string.IsNullOrEmpty(str);

        public static bool ContainsIgnoreCase(this string str, string value) => str?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;

        public static bool IsEmpty<T>(this IEnumerable<T> list) => list?.Any() != true;

        public static bool Replace<T>(this IList<T> list, T oldItem, T newItem)
        {
            var index = list.IndexOf(oldItem);
            if (index >= 0)
            {
                list[index] = newItem;
                return true;
            }
            return false;
        }

        public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> pre)
        {
            var index = 0;
            if (source == null)
                return -1;
            foreach (var item in source)
            {
                if (pre(item))
                    return index;
                else
                    index++;
            }
            return -1;
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return new ObservableCollection<T>();
            else
                return new ObservableCollection<T>(source);
        }

        public static Bangumi InitTrackings(this Bangumi b)
        {
            if (!b.Trackings.IsEmpty())
                foreach (var item in b.Trackings)
                    item.Bangumi = b;
            return b;
        }

        public static List<Bangumi> InitTrackings(this List<Bangumi> list)
        {
            list?.ForEach(b => b.InitTrackings());
            return list;
        }

        public static Tracking InitBangumi(this Tracking t)
        {
            t.Bangumi?.Trackings?.Add(t);
            return t;
        }

        public static List<Tracking> InitBangumi(this List<Tracking> list)
        {
            list?.ForEach(t => t.InitBangumi());
            return list;
        }

        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var json = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T Copy<T>(this T source)
        {
            var json = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static async Task Message(this FrameworkElement fr, string msg, string title)
        {
            await new MessageDialog(msg, title).ShowAsync();
        }

        public static async Task Message(this FrameworkElement fr, string msg)
        {
            await new MessageDialog(msg).ShowAsync();
        }

        public static async Task LaunchAsUri(this string uri)
        {
            await Launcher.LaunchUriAsync(new Uri(uri));
        }

        public static async Task LaunchToWeb(this string uri)
        {
            if (AppCache.AppSettings.UseInternalBrowser)
                NavigationHelper.NavigateToWeb(uri);
            else
                await uri.LaunchAsUri();
        }

        public static async Task<StorageFile> AsFile(this string path)
        {
            var folder = AppCache.VideoFolder;
            return (await folder.TryGetItemAsync(path.Substring(folder.Path.Length).Trim('\\'))) as StorageFile;
        }

        public static async Task<StorageFolder> AsFolder(this string path)
        {
            var folder = AppCache.VideoFolder;
            return (await folder.TryGetItemAsync(path.Substring(folder.Path.Length).Trim('\\'))) as StorageFolder;
        }

        public static async Task LaunchAsFile(this string path)
        {
            var file = await path.AsFile();
            if (file != null)
                await Launcher.LaunchFileAsync(file);
        }

        public static async Task LaunchAsFolder(this string path)
        {
            var f = await path.AsFolder();
            if (f != null)
                await Launcher.LaunchFolderAsync(f);
        }

        public static string GetExt(this StorageFile file)
        {
            var index = file.Name.LastIndexOf('.');
            return file.Name.Substring(index);
        }

        public static async Task<string> GetStringWithRedirect(this HttpClient hc, string uri)
        {
            var res = await hc.GetAsync(uri);
            while (res.StatusCode != System.Net.HttpStatusCode.OK)
            {
                if (res.StatusCode == System.Net.HttpStatusCode.Redirect)
                    res = await hc.GetAsync(res.Headers.Location);
                else
                    return null;
            }
            return await res.Content.ReadAsStringAsync();
        }

        public static string[] StringSplit(this string str)
        {
            var chars = new[] { '[', ']' };
            return str.Split(chars, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string[] StringMatchSplit(this string str)
        {
            var ps = AppCache.AppSettings.LocalFilePattern;
            foreach (var p in ps)
            {
                var m = Regex.Match(str, p);
                if (m.Success && m.Groups.Count > 3)
                {
                    var result = new[]
                    {
                        m.Groups[1].Value,
                        m.Groups[2].Value,
                        m.Groups[3].Value,
                    };
                    if (string.IsNullOrEmpty(result[0]))
                        result[0] = "Other";
                    return result;
                }
            }
            return null;
        }
    }
}
