﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using LogMagic.Configuration;

namespace LogMagic
{
   /// <summary>
   /// Global public logging configuration and initialisation class
   /// </summary>
   public static class L
   {

      /// <summary>
      /// Gets logging library configuration
      /// </summary>
      public static ILogConfiguration Config { get; } = new LogConfiguration();
      /// <summary>
      /// Get logger for the specified type
      /// <typeparam name="T">Class type</typeparam>
      /// </summary>
      public static ILog G<T>()
      {
         return new LogClient(typeof(T));
      }

      /// <summary>
      /// Get logger for the specified type
      /// </summary>
      public static ILog G(Type t)
      {
         return new LogClient(t);
      }

      /// <summary>
      /// Gets logger by specified name. Use when you can't use more specific methods.
      /// </summary>
      public static ILog G(string name)
      {
         return new LogClient(name);
      }

#if NETFULL
    /// <summary>
    /// Get logger for the current class
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.NoInlining)]
      public static ILog G()
      {
         return new LogClient(GetClassFullName());
      }
#endif

#if NETFULL
    /// <summary>
    /// Gets the fully qualified name of the class invoking the LogManager, including the 
    /// namespace but not the assembly.    
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
      private static string GetClassFullName()
      {
         string className;
         Type declaringType;
         int framesToSkip = 2;

         do
         {
            StackFrame frame = new StackFrame(framesToSkip, false);
            MethodBase method = frame.GetMethod();
            declaringType = method.DeclaringType;
            if (declaringType == null)
            {
               className = method.Name;
               break;
            }

            framesToSkip++;
            className = declaringType.FullName;
         } while (declaringType.Module.Name.Equals("mscorlib.dll", StringComparison.OrdinalIgnoreCase));

         return className;
      }
#endif

      /// <summary>
      /// Adds a context property
      /// </summary>
      /// <param name="name">Property name</param>
      /// <param name="value">Property value</param>
      /// <returns></returns>
      public static IDisposable CP(string name, string value)
      {
         return LogContext.PushProperty(name, value);
      }

   }
}
