﻿using System;
using System.Collections.Generic;
using System.Linq;
using Drone.Lib.Helpers;
using NLog;
using System.IO;
using System.Diagnostics;

namespace Drone.Lib.Core
{
	public static class DroneLogManager
	{
		public static readonly string LogName = "drone";

		public static readonly string TaskLogBaseName = "drone.task";

		public static readonly string ErrorLogFilename = "drone.errors.txt";

		public static Logger GetTaskLog(string taskName)
		{
			if (string.IsNullOrWhiteSpace(taskName))
				throw new ArgumentException("taskName is empty or null", "taskName");

			var logName = string.Format("{0}.{1}", TaskLogBaseName, taskName);
			return LogManager.GetLogger(logName);
		}

		public static Logger GetLog()
		{
			return LogManager.GetLogger(LogName);
		}

		public static LogLevel GetLogLevelFromString(string str)
		{
			if (string.IsNullOrWhiteSpace(str))
				throw InvalidLogLevelStringException.Get(str);

			var level = null as LogLevel;

			switch (str.ToLower())
			{
				case "off":
					level = LogLevel.Off;
					break;

				case "fatal":
					level = LogLevel.Fatal;
					break;

				case "error":
				case "err":
					level = LogLevel.Error;
					break;

				case "warn":
					level = LogLevel.Warn;
					break;

				case "info":
					level = LogLevel.Info;
					break;

				case "debug":
					level = LogLevel.Debug;
					break;

				case "trace":
					level = LogLevel.Trace;
					break;
			}

			if (level == null)
				throw InvalidLogLevelStringException.Get(str);

			return level;
		}

		public static void DeleteErrorLogFile()
		{
			try
			{
				if(File.Exists(ErrorLogFilename))
					File.Delete(ErrorLogFilename);
			}
			catch (Exception)
			{
				
			}
		}

		public static void ExceptionAndData(this Logger log, Exception ex)
		{
			if (log == null)
				throw new ArgumentNullException("log");

			if (ex == null)
				throw new ArgumentNullException("ex");

			var exceptions = ex.ToList();

			var counter = 1;

			foreach (var cex in exceptions)
			{
				log.Info(string.Empty);

				log.Error("({0}/{1}) {2}", counter, exceptions.Count, cex.Message);

				foreach (var key in cex.Data.Keys)
				{
					log.Error("{0}: {1}", key, ex.Data[key]);
				}

				counter += 1;
			}

			if (log.IsDebugEnabled)
			{
				log.Info(string.Empty);
				log.Error(ex);
			}
		}

		public static void LogTimeElapsed(this Logger log, string fmt, Stopwatch sw)
		{
			if (log == null)
				throw new ArgumentNullException("log");

			if (sw == null)
				throw new ArgumentNullException("sw");

			log.Info(fmt, HumanTime.Format(sw.Elapsed));
		}
	}
}