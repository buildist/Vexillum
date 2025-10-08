using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vexillum.Entities;
using System.Threading;
using System.Collections.Concurrent;

namespace Vexillum.util
{
    public class FrameTaskQueue
    {
        private ConcurrentDictionary<int, List<FrameTask>> frameActions = new ConcurrentDictionary<int, List<FrameTask>>();
        private Level level;
        private int frame;
        private TaskQueue tasks;
        public FrameTaskQueue(TaskQueue tasks, Level l)
        {
            this.tasks = tasks;
            level = l;
        }
        public void setFrame(int f)
        {
            if (f > frame)
            {
                for (int i = frame; i < f; i++)
                {
                    frame = i;
                    Process();
                }
            }
            frame = f;
        }
        public void Process()
        {
            lock (frameActions)
            {
                if (frameActions.ContainsKey(frame))
                {
                    List<FrameTask> actions;
                    frameActions.TryRemove(frame, out actions);
                    if (actions != null)
                    {
                        foreach (FrameTask t in actions)
                        {
                            t.action();
                            try
                            {

                            }
                            catch (Exception ex)
                            {
                                Util.Debug("WARNING: FrameTask " + frame + " caused an exception:");
                                Util.Debug(ex.ToString());
                                Util.Debug(ex.StackTrace);
                            }
                        }
                    }
                }
                frame++;
                Monitor.Pulse(frameActions);
            }
        }
        public void WaitForFrame(int frame)
        {
            lock (frameActions)
            {
                if (frame <= this.frame)
                    return;
                List<FrameTask> actions = frameActions[frame];
                if (!frameActions.ContainsKey(frame))
                    frameActions[frame] = new List<FrameTask>();
                Monitor.Wait(frameActions);
            }
        }
        public void Add(TaskDelegate task, int frame, Entity entity)
        {
            lock (frameActions)
            {
                if (frame <= this.frame)
                {
                    tasks.AddTask(delegate()
                    {
                        task();
                        if (entity != null)
                        {
                            for (int f = frame; f < this.frame; f++)
                            {
                                level.DoPhysicsForEntity(entity);
                            }
                        }
                    });
                }
                else
                {
                    if (!frameActions.ContainsKey(frame))
                        frameActions[frame] = new List<FrameTask>();
                    frameActions[frame].Add(new FrameTask(entity, task));
                }
            }
        }
    }
}
