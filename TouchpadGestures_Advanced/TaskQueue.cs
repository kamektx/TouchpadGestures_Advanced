using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;

namespace TouchpadGestures_Advanced
{
    class TaskQueueSingleThread :IDisposable
    {
        private Queue<Action> TaskQueue;
        private bool Disposed = false;
        private Thread MyThread;
        private readonly ManualResetEvent ATaskAdded = new ManualResetEvent(false);
        private readonly object LockObject = new object();


        ~TaskQueueSingleThread()
        {
            this.Dispose(false);
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool isDisposing)
        {
            if (!this.Disposed)
            {
                if (MyThread != null)
                {
                    MyThread.Interrupt();
                    MyThread.Join();
                }
                if (isDisposing)
                {
                    ATaskAdded.Dispose();
                }
                this.Disposed = true;
            }
        }
        private void Loop()
        {
            try
            {
                Action task;
                while (true)
                {
                    this.ATaskAdded.WaitOne();
                    this.ATaskAdded.Reset();

                    while (true)
                    {
                        lock (LockObject)
                        {
                            if (!TaskQueue.Any())
                            {
                                break;
                            }
                            task = TaskQueue.Dequeue();
                        }
                        task();
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                //おしまい
            }
        }

        public void AddTask(Action task)
        {
            lock (LockObject)
            {
                TaskQueue.Enqueue(task);
            }
            this.ATaskAdded.Set();
        }
        public TaskQueueSingleThread()
        {
            TaskQueue = new Queue<Action>();   
            MyThread = new Thread(Loop);
            MyThread.IsBackground = true;
            MyThread.Start();
        }
    }
}
