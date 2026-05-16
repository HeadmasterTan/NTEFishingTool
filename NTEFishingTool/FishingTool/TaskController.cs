using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace NTEFishingTool.FishingTool
{
    internal class TaskController
    {
        // 存储任务控制开关：Key为任务ID，Value为用于控制暂停/继续的信号源
        private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _taskSwitches = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

        // 取消令牌源，用于彻底停止所有任务
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public void StartTask(string taskId, Action taskFunc)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            tcs.SetResult(true);
            _taskSwitches[taskId] = tcs;

            Task.Run(async () => await DoWorkAsync(taskId, _cts.Token, taskFunc));
        }

        private async Task DoWorkAsync(string taskId, CancellationToken token, Action taskFunc)
        {
            Console.WriteLine($"任务 [{taskId}] 已启动");

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // 检查是否需要暂停
                    if (_taskSwitches.TryGetValue(taskId, out var tcs))
                    {
                        // 如果tcs.Task未完成，代码会在此处异步挂起，不阻塞线程池线程
                        await tcs.Task;
                    }

                    taskFunc();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"任务 [{taskId}] 收到取消信号");
            }
            finally
            {
                Console.WriteLine($"任务 [{taskId}] 已停止");
            }
        }

        public void PauseTask(params string[] taskIds)
        {
            foreach (string id in taskIds)
            {
                if (_taskSwitches.TryGetValue(id, out var tcs))
                {
                    // 如果当前是通行状态，则替换为一个全新未完成的tcs，后续执行到await tcs.Task 时就会暂停
                    if (tcs.Task.IsCompleted)
                    {
                        _taskSwitches[id] = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                        Console.WriteLine($"任务 [{id}] 已被暂停");
                    }
                }
            }
        }

        public void ResumeTasks(params string[] taskIds)
        {
            foreach(string id in taskIds)
            {
                if (_taskSwitches.TryGetValue(id, out var tcs))
                {
                    // 通过设置Result激活通过信号，让await tcs.Task恢复执行
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.TrySetResult(true);
                        Console.WriteLine($"任务 [{id}] 已恢复运行");
                    }
                }
            }
        }

        public void StopAllTasks()
        {
            _cts.Cancel();
            // 唤醒所有可能处于暂停状态的任务，让他们有机会检查CancellationToken并退出
            foreach (var kvp in _taskSwitches)
            {
                kvp.Value.TrySetResult(true);
            }
        }
    }
}
