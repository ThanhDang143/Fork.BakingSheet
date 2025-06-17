using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThanhDV.Utilities.Editor
{
    public class LogSuppressor : ILogHandler
    {
        public ILogHandler PreviousHandler { get; }
        public IReadOnlyList<string> SuppressedMessages { get; }

        private readonly List<string> m_SuppressedMessagesInternal;

        public LogSuppressor(ILogHandler previousHandler, List<string> suppressedMessages)
        {
            PreviousHandler = previousHandler;
            m_SuppressedMessagesInternal = suppressedMessages;
            SuppressedMessages = m_SuppressedMessagesInternal.AsReadOnly();
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            string message = string.Format(format, args);

            if (m_SuppressedMessagesInternal.Any(suppressed => message.Contains(suppressed)))
            {
                return;
            }

            PreviousHandler?.LogFormat(logType, context, format, args);
        }

        public void LogException(System.Exception exception, Object context)
        {
            PreviousHandler?.LogException(exception, context);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            if (!Application.isEditor) return;

            // Add rules here!!!
            var rulesForThisTool = new List<string>
            {
                "has no meta file, but it's in an immutable folder. The asset will be ignored."
            };

            var existingRules = new HashSet<string>();
            ILogHandler current = Debug.unityLogger.logHandler;
            while (current != null)
            {
                if (current is LogSuppressor handler)
                {
                    foreach (var rule in handler.SuppressedMessages)
                    {
                        existingRules.Add(rule);
                    }

                    current = handler.PreviousHandler;
                }
                else
                {
                    break;
                }
            }

            var newRulesToAdd = rulesForThisTool.Where(rule => !existingRules.Contains(rule)).ToList();

            if (newRulesToAdd.Count == 0) return;

            var newHandler = new LogSuppressor(Debug.unityLogger.logHandler, newRulesToAdd);
            Debug.unityLogger.logHandler = newHandler;
        }
    }
}
