using System;
using System.Collections;
using System.Threading;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;
using Vexillum.net;
using Vexillum.Entities;
using System.Diagnostics;

namespace Vexillum.util
{
    /*public class ConditionalTask : Task
    {
        public long lastRunTime;
        public ConditionalTask(ConditionalTaskDelegate d)
        {
            task = delegate()
            {
                if (d())
                {
                    
                }
            };
        }
    }*/
    public delegate void TaskDelegate();
    public delegate bool ConditionalTaskDelegate();
    public class FrameTask
    {
        public TaskDelegate action;
        public Entity entity;
        public FrameTask(Entity e, TaskDelegate a)
        {
            entity = e;
            action = a;
        }
    }
    public class TaskQueue
    {
        private int idx;
        private ArrayList tasks = new ArrayList();
        public TaskQueue()
        {
        }
        public void AddTask(TaskDelegate task)
        {
            lock (tasks.SyncRoot)
            {
                tasks.Add(task);
            }
        }
        public void AddConditionalTask(ConditionalTaskDelegate task)
        {
            TaskDelegate d = null;
            d = delegate()
            {
                if (task())
                {
                    tasks.Add(d);
                    //idx++;
                }
            };
            AddTask(d);
        }
        public void Process(int time)
        {
            lock (tasks.SyncRoot)
            {
                int count = tasks.Count;
                for(idx = 0; idx < count; idx++)
                {
                    TaskDelegate task = (TaskDelegate) tasks[0];
                    tasks.RemoveAt(0);
                    try
                    {
                        task();
                    }
                    catch (Exception ex)
                    {
                        Util.Debug("WARNING: Task caused an exception:");
                        Util.Debug(ex.ToString());
                        Util.Debug(ex.StackTrace);
                    }
                }
            }
            Thread.MemoryBarrier();
        }
    }
}
