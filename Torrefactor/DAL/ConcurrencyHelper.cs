using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using Torrefactor.Controllers;

namespace Torrefactor.DAL
{
    public static class ConcurrencyHelper
    {
        private static int _maxRetries = 2;

        public static Task WithConcurrency<T>(
            Func<T, bool> operation,
            Func<Task<T>> getter,
            Func<T, Task> saver,
            T initialDocument = null
        )
            where T : class
        {
            return retry(operation, getter, saver, initialDocument);
        }

        private static async Task<bool> retry<T>(
            Func<T, bool> operation,
            Func<Task<T>> getter,
            Func<T, Task> saver,
            T initialDocument = null
        )
            where T : class
        {
            var currentRetry = 0;
            do
            {
                T target = initialDocument ?? await getter();
                initialDocument = null;

                if (!operation(target))
                    return false;

                try
                {
                    await saver(target);
                    return true;
                }
                catch (ConcurrencyException)
                {
                    if (currentRetry >= _maxRetries)
                        throw;
                    currentRetry++;
                }
            } while (true);
        }
    }
}