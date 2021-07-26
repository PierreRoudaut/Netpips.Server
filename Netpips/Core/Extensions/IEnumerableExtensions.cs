
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Netpips.Core.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            var index = new Random().Next(0, enumerable.Count());
            return enumerable.ElementAt(index);
        }

        public static IEnumerable<T> RandomSubsequence<T>(this IEnumerable<T> enumerable, int subSequenceCount)
        {
            var skip = enumerable.Count() - subSequenceCount;
            if (skip < 0) throw new IndexOutOfRangeException();

            var rand = new Random();
            return enumerable.OrderBy(item => rand.Next()).Skip(skip);
        }

        public static async Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks, TimeSpan timeout)
        {
            var timeoutTask = Task.Delay(timeout).ContinueWith(_ => default(TResult));
            var completedTasks =
                (await Task.WhenAll(tasks.Select(task => Task.WhenAny(task, timeoutTask)))).
                Where(task => task != timeoutTask);
            return await Task.WhenAll(completedTasks);
        }
    }
}
