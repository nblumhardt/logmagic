﻿using LogMagic.Enrichers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LogMagic
{
   /// <summary>
   /// Utility class to server log clients
   /// </summary>
   class LogClient : ILog
   {
      private readonly string _name;

      public LogClient(Type type) :
         this(type.FullName)
      {
         
      }

      public LogClient(string name)
      {
         _name = name ?? throw new ArgumentNullException(nameof(name));
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      internal void Serve(LogSeverity severity, EventType eventType,
         Dictionary<string, object> properties,
         string format,  params object[] parameters)
      {
         LogEvent e = EventFactory.CreateEvent(_name, severity, eventType, format, parameters);

         if(properties != null && properties.Count > 0)
         {
            foreach(var prop in properties)
            {
               e.AddProperty(prop.Key, prop.Value);
            }
         }

         SubmitNow(e);
      }

      private void SubmitNow(LogEvent e)
      {
         foreach (ILogWriter writer in new List<ILogWriter>(L.Config.Writers))
         {
            try
            {
               writer.Write(new[] { e });
            }
            catch(Exception ex)
            {
               //there is nowhere else to log the error as we are the logger!
               Console.WriteLine("could not write: " + ex);

#if NETFULL
               Trace.TraceError("fatal submit error: " + ex);
#endif
            }
         }
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void D(string format, params object[] parameters)
      {
         Serve(LogSeverity.Debug, EventType.Trace, null, format, parameters);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void E(string format, params object[] parameters)
      {
         Serve(LogSeverity.Error, EventType.Trace, null, format, parameters);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void I(string format, params object[] parameters)
      {
         Serve(LogSeverity.Info, EventType.Trace, null, format, parameters);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void W(string format, params object[] parameters)
      {
         Serve(LogSeverity.Warning, EventType.Trace, null, format, parameters);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void Dependency(string type, string name, string command, long duration, Exception error)
      {
         var properties = new Dictionary<string, object>
         {
            { KnownProperty.Duration, duration },
            { KnownProperty.DependencyName, name },
            { KnownProperty.DependencyType, type },
            { KnownProperty.DependencyCommand, command }
         };

         var parameters = new List<object> { _name, command, TimeSpan.FromTicks(duration) };
         if (error != null) parameters.Add(error);

         Serve(LogSeverity.Info, EventType.Dependency, properties,
            "dependency {0}.{1} took {2}",
            parameters.ToArray());
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void Event(string name, Dictionary<string, object> properties)
      {
         if (properties == null) properties = new Dictionary<string, object>();
         properties[KnownProperty.EventName] = name;

         Serve(LogSeverity.Info, EventType.ApplicationEvent, properties,
            "application event {0} occurred",
            name);
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void Request(string name, long duration, Exception error)
      {
         var properties = new Dictionary<string, object>
         {
            { KnownProperty.Duration, duration },
            { KnownProperty.RequestName, name }
         };

         var parameters = new List<object> { name, TimeSpan.FromTicks(duration) };
         if (error != null) parameters.Add(error);

         Serve(LogSeverity.Info, EventType.HandledRequest, properties,
            "request {0} took {1}",
            parameters.ToArray());
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      public void Metric(string name, double value, Dictionary<string, object> properties)
      {
         if (properties == null) properties = new Dictionary<string, object>();
         properties[KnownProperty.MetricName] = name;
         properties[KnownProperty.MetricValue] = value;

         Serve(LogSeverity.Info, EventType.Metric, properties,
            "metric {0} == {1}",
            name, value);
      }
   }
}
