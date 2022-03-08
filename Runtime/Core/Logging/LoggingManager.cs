//-----------------------------------------------------------------------
// <copyright file="LoggingManager.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class LoggingManager : Manager<LoggingManager>
    {
        private const string LogEventName = "log_event";
        private const string LogTypeName = "log_type";
        private const string ConditionName = "condition";
        private const string HashCodeName = "hash_code";
        private const string CallstackName = "callstack";

        #pragma warning disable 0649
        [SerializeField] private bool forwardLoggingAsAnalyticsEvents = true;
        [SerializeField] private bool dontForwardInfoLevelLogging = true;

        [Tooltip("Don't forward any log events that contain these strings.")]
        [SerializeField] private List<string> forwardingExceptions;
        #pragma warning restore 0649

        private readonly List<ILoggingProvider> loggingProviders = new List<ILoggingProvider>();
        private readonly HashSet<int> sentLogs = new HashSet<int>();

        private readonly Dictionary<string, object> eventArgsCache = new Dictionary<string, object>()
        {
            { LogTypeName, string.Empty },
            { ConditionName, string.Empty },
            { HashCodeName, 0 },
        };

        private readonly Dictionary<LogType, string> logTypeCache = new Dictionary<LogType, string>
        {
            { LogType.Assert, "Assert" },
            { LogType.Error, "Error" },
            { LogType.Exception, "Exception" },
            { LogType.Log, "Log" },
            { LogType.Warning, "Warning" },
        };

        public override void Initialize()
        {
            if (Application.isEditor == false)
            {
                Application.logMessageReceived += this.Application_logMessageReceived;
            }

            this.SetInstance(this);
        }

        public void AddLoggingProvider(ILoggingProvider loggingProvider)
        {
            this.loggingProviders.Add(loggingProvider);
        }

        private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
        {
            try
            {
                int stackTraceHashCode = stackTrace.GetHashCode();

                // Forward all Logging as an Analytic Event (if we haven't seen it before this session)
                if (this.forwardLoggingAsAnalyticsEvents && this.sentLogs.Contains(stackTraceHashCode) == false)
                {
                    // Making sure we don't send regular logs up if that flag is set
                    if (this.dontForwardInfoLevelLogging == false || type != LogType.Log)
                    {
                        this.sentLogs.Add(stackTraceHashCode);

                        if (this.IsForwardingException(condition) == false)
                        {
                            this.eventArgsCache[LogTypeName] = this.logTypeCache[type];
                            this.eventArgsCache[ConditionName] = condition;
                            this.eventArgsCache[HashCodeName] = stackTraceHashCode;

                            // NOTE [bgish]: Currently can't do this, because it puts us over event size limits
                            this.eventArgsCache[CallstackName] = string.Empty; // stackTrace

                            Lost.Analytics.AnalyticsEvent.Custom(LogEventName, this.eventArgsCache);
                        }
                    }
                }

                // Sending the log to all the providers
                for (int i = 0; i < this.loggingProviders.Count; i++)
                {
                    try
                    {
                        this.loggingProviders[i].Log(condition, stackTrace, type);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private bool IsForwardingException(string condition)
        {
            if (this.forwardingExceptions?.Count > 0)
            {
                for (int i = 0; i < this.forwardingExceptions.Count; i++)
                {
                    if (this.forwardingExceptions[i]?.IsNullOrWhitespace() == false &&
                        condition.Contains(this.forwardingExceptions[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

#endif
