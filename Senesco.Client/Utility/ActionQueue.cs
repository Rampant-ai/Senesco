using System;
using System.Collections.Generic;
using System.Threading;
using log4net;

namespace Senesco.Client.Utility
{
   /// <summary>
   /// Class that provides an asynchronous "Action Queue" where each action
   /// added to the queue is processed in sequence on a separate processing
   /// thread.  This is preferred over having each action run in its own
   /// thread, which would quickly lead to a huge mess of threads.
   /// </summary>
   public class ActionQueue
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(ActionQueue));

      private Queue<QueuedAction> m_queue = new Queue<QueuedAction>();
      private Thread m_thread = null;
      private AutoResetEvent m_gate = new AutoResetEvent(false);

      public delegate Status QueuedAction();

      public Status Add(QueuedAction action)
      {
         try
         {
            lock (m_queue)
            {
               m_queue.Enqueue(action);
            }
            // Start or restart the processing thread.
            if (m_thread == null || m_thread.ThreadState == ThreadState.Stopped || m_thread.ThreadState == ThreadState.Aborted)
            {
               m_thread = new Thread(ProcessActions);
               m_thread.Name = "ActionQueue Thread";
               m_thread.Start();
            }

            // Notify the processing thread that there's an action to process.
            m_gate.Set();

            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Exception queueing action: {0}", e.Message);
            return Status.Failure;
         }
      }

      private void ProcessActions()
      {
         while (true)
         {
            QueuedAction action = null;
            lock (m_queue)
            {
               if (m_queue.Count > 0)
                  action = m_queue.Dequeue();
            }

            // If there are no actions, sleep until notified.
            if (action == null)
            {
               m_gate.WaitOne();
            }
            else // Otherwise process the QueuedAction pulled from the queue.
            {
               try
               {
                  Status result = action.Invoke();
                  s_log.DebugFormat("Method {0} returned {1}", action.Method, result.ToString());
               }
               catch (Exception e)
               {
                  s_log.ErrorFormat("Exception during action {0}: {1}", action.Method, e.Message);
               }
            }
         }
      }
   }
}
