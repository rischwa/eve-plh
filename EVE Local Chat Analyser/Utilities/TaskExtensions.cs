using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace EveLocalChatAnalyser.Utilities
{
    public static class TaskExtensions
    {
        public static Task<T> ContinueInDispatcher<T, K>(this Task<K> task, Task<T> taskOut)
        {
            return task.ContinueWith(
                                     x =>
                                     {
                                         Application.Current.Dispatcher.Invoke(new Action( taskOut.RunSynchronously));
                                         return taskOut.Result;
                                     });
        }

        public static Task ContinueInDispatcher(this Task task, Action<Task> taskOut, CancellationToken token = default(CancellationToken))
        {
            return task.ContinueWith(
                                     x =>
                                     {
                                         Application.Current.Dispatcher.Invoke(new Action(()=>taskOut(x)));
                                     }, token);
        }

        public static Task ContinueInDispatcher(this Task task, Action taskOut, CancellationToken token = default(CancellationToken))
        {
            return task.ContinueInDispatcher(x => taskOut(), token);
        }
    }
}
