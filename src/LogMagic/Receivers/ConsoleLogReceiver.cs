﻿using System;
using LogMagic.Formatters;

namespace LogMagic.Receivers
{
   /// <summary>
   /// Outputs messages to system console and ideal for server logging. It doesn't do anything fancy 
   /// unlike <see cref="PoshConsoleLogReceiver"/>
   /// </summary>
   public class ConsoleLogReceiver : ILogReceiver
   {
      private readonly ILogChunkFormatter _formatter;

      /// <summary>
      /// Creates class instance
      /// </summary>
      public ConsoleLogReceiver() : this(null)
      {
         
      }

      /// <summary>
      /// Creates class instance
      /// </summary>
      /// <param name="formatter">Optional formatter</param>
      public ConsoleLogReceiver(ILogChunkFormatter formatter)
      {
         _formatter = formatter ?? new StandardFormatter();
      }

      /// <summary>
      /// Prints chunk to system console using the formatter
      /// </summary>
      /// <param name="chunk"></param>
      public void Send(LogChunk chunk)
      {
         Console.Write(_formatter.Format(chunk));
      }

      /// <summary>
      /// There is nothing to dispose in the console
      /// </summary>
      public void Dispose()
      {
      }
   }
}
