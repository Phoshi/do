using System.Collections.Generic;
using System.Data;
using System.Linq;
using Tasks;

namespace Do.Extensions
{
    public static class TaskCollectionExtensions
    {
        public static IEnumerable<Task.T> Update(this IEnumerable<Task.T> tasks, Task.T task)
            => tasks.Select(t => t.filepath == task.filepath ? task : t);

    }
}