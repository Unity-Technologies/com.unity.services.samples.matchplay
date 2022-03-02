using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Matchplay.Tests.Helpers
{
    public static class AsyncTestHelpers
    {
        // Runs a Task like a coroutine; logs and re-throws exceptions
        // Ex:
        //  [UnityTest]
        //  public IEnumerator AsyncTestHelpers_ExecuteTask_ExampleTest_A()
        //  {
        //      async Task TestAsync()
        //      {
        //          Debug.Log("starting test");
        //          await Task.Delay(5000);
        //          Debug.Log("after 5000ms delay");
        //      }
        //     yield return AsyncTestHelpers.ExecuteTask(TestAsync());
        //  }
        public static IEnumerator ExecuteTask(Task task)
        {
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    Debug.LogException(task.Exception);

                    // TODO - Note that exceptions on IEnumerators / coroutines are generally not handled
                    throw task.Exception;
                }
            }
        }

        // Runs a Func<Task> like a coroutine; logs and re-throws exceptions
        // Ex:
        //  [UnityTest]
        //  public IEnumerator AsyncTestHelpers_ExecuteTask_ExampleTest_B()
        //  {
        //      yield return AsyncTestHelpers.ExecuteTask(Task.Run(async () =>
        //      {
        //          Debug.Log("starting test");
        //          await Task.Delay(5000);
        //         Debug.Log("after 5000ms delay");
        //     }));
        //  }
        public static IEnumerator ExecuteTask(Func<Task> taskFunc)
        {
            var task = taskFunc.Invoke();
            yield return ExecuteTask(task);
        }

        public static IEnumerator RunActionAsUnityCoroutine(Action<Action> action)
        {
            var hasFinished = false;
            Exception exceptionCaught = null;
            try
            {
                action(() =>
                {
                    hasFinished = true;
                });
            }
            catch (Exception e)
            {
                exceptionCaught = e;
                hasFinished = true;
            }

            while (!hasFinished)
            {
                yield return null;
            }

            if (exceptionCaught != null)
            {
                throw exceptionCaught;
            }
        }

        public static IEnumerator RunAsyncTest(Action<Action> setup, Action<Action> test, Action validate,
            Action<Action> teardown)
        {
            if (setup != null)
            {
                yield return RunActionAsUnityCoroutine(setup);
            }

            yield return RunActionAsUnityCoroutine(test);
            validate();

            if (teardown != null)
            {
                yield return RunActionAsUnityCoroutine(teardown);
            }
        }
    }
}