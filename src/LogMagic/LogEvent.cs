﻿using System;
using System.Collections.Generic;
using LogMagic.Tokenisation;
using LogMagic.Enrichers;

namespace LogMagic
{
   /// <summary>
   /// Represents a log event
   /// </summary>
   public class LogEvent
   {
      /// <summary>
      /// Constructs a new instance of a log event
      /// </summary>
      public LogEvent(LogSeverity severity, string sourceName, DateTime eventTime)
      {
         Severity = severity;
         SourceName = sourceName;
         EventTime = eventTime;
      }

      /// <summary>
      /// Name of the source, usually class name or type
      /// </summary>
      public string SourceName;

      /// <summary>
      /// Log severity
      /// </summary>
      public LogSeverity Severity;

      /// <summary>
      /// Type of log event, default is trace
      /// </summary>
      public EventType EventType;

      /// <summary>
      /// Time in UTC when log event has occurred
      /// </summary>
      public DateTime EventTime;

      /// <summary>
      /// Formatted log message
      /// </summary>
      public string FormattedMessage;

      /// <summary>
      /// Tokenised log message
      /// </summary>
      public FormattedString Message;

      /// <summary>
      /// Extra properties
      /// </summary>
      public Dictionary<string, object> Properties;

      /// <summary>
      /// Tries to find the error and cast to <see cref="System.Exception"/> class, otherwise returns null
      /// </summary>
      public Exception ErrorException
      {
         get
         {
            object exceptionObject = GetProperty(KnownProperty.Error);
            return exceptionObject as Exception;
         }
      }

      /// <summary>
      /// Adds a new property to this event
      /// </summary>
      public void AddProperty(string name, object value)
      {
         if (name == null || value == null) return;

         if(Properties == null) Properties = new Dictionary<string, object>();

         Properties[name] = value;
      }

      /// <summary>
      /// Tries to get the log property by name
      /// </summary>
      public object GetProperty(string name)
      {
         if (Properties == null) return null;

         object r;
         if (!Properties.TryGetValue(name, out r)) return null;
         return r;
      }

      /// <summary>
      /// Calling this method get the property by name and removes from the properties dictionary
      /// </summary>
      /// <param name="name">Property name</param>
      /// <param name="defaultValue">Defalt value to use if property is not found</param>
      /// <returns>Property value</returns>
      public T UseProperty<T>(string name, T defaultValue = default(T))
      {
         if (Properties == null) return defaultValue;

         object r;
         if (Properties.TryGetValue(name, out r))
         {
            Properties.Remove(name);
         }
         else
         {
            r = defaultValue;
         }

         try
         {
            return (T)r;
         }
         catch(InvalidCastException)
         {
            //as a last resort try to cast to string
            if(typeof(T) == typeof(string))
            {
               return (T)(object)(r.ToString());
            }

            return defaultValue;
         }
      }
   }
}
