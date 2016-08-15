using BangumiSU.Models;
using System;
using System.Threading.Tasks;
using PropertyChanged;

namespace BangumiSU.ViewModels
{
    public abstract class ViewModelBase : ModelBase
    {
        public bool OnLoading { get; set; } = false;

        [DependsOn(nameof(OnLoading))]
        public bool IsLoaded => !OnLoading;

        public string Message { get; set; }

        public async Task<T> LoadingTask<T>(Task<T> task)
        {
            try
            {
                OnLoading = true;
                return await task;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                return default(T);
            }
            finally
            {
                OnLoading = false;
            }
        }

        public async Task LoadingTask(Task task)
        {
            try
            {
                OnLoading = true;
                await task;
                return;
            }
            catch (Exception ex)
            {
                Message = ex.Message;
                return;
            }
            finally
            {
                OnLoading = false;
            }
        }
    }
}
