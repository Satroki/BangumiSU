using BangumiSU.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using System;
using Windows.System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Linq.Expressions;

namespace BangumiSU.SharedCode
{
    public static class Extensions
    {
        public static bool IsEmpty(this string str) => string.IsNullOrEmpty(str);

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

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering, params object[] values)
        {
            var type = typeof(T);
            var property = type.GetProperty(ordering);
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable), "OrderBy", new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
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

        public static async Task LaunchAsFile(this string path)
        {
            var folder = AppCache.VideoFolder;
            var file = (await folder.TryGetItemAsync(path.Substring(folder.Path.Length))) as StorageFile;
            if (file != null)
                await Launcher.LaunchFileAsync(file);
        }

        public static async Task LaunchAsFolder(this string path)
        {
            var folder = AppCache.VideoFolder;
            var f = (await folder.TryGetItemAsync(path.Substring(folder.Path.Length))) as StorageFolder;
            if (f != null)
                await Launcher.LaunchFolderAsync(f);
        }
    }
}
