// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Diagnostics;

using UnityEngine;

namespace CCGKit
{
    /// <summary>
    /// This class wraps Unity's Debug.Log with its own methods marked with the Conditional attribute
    /// so that logging only happens when explicitly enabled via a conditional compilation symbol.
    /// </summary>
    public sealed class Logger
    {
        public const string LOGGER_SYMBOL = "ENABLE_LOG";

        [Conditional(LOGGER_SYMBOL)]
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        [Conditional(LOGGER_SYMBOL)]
        public static void Log(string message, Object context)
        {
            UnityEngine.Debug.Log(message, context);
        }

        [Conditional(LOGGER_SYMBOL)]
        public static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        [Conditional(LOGGER_SYMBOL)]
        public static void LogWarning(string message, Object context)
        {
            UnityEngine.Debug.LogWarning(message, context);
        }

        [Conditional(LOGGER_SYMBOL)]
        public static void LogError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        [Conditional(LOGGER_SYMBOL)]
        public static void LogError(string message, Object context)
        {
            UnityEngine.Debug.LogError(message, context);
        }
    }
}