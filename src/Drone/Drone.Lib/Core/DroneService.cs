﻿using Drone.Lib.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Drone.Lib.Core
{
	public class DroneService
	{
		private readonly DroneConfigStore store;
		private readonly DroneCompiler compiler;
		private readonly DroneLoader loader;
		private readonly DroneTaskRunner taskRunner;
		private readonly Logger log;

		public DroneService()
		{
			this.log = DroneLogManager.GetLog();
			this.store = new DroneConfigStore();
			this.compiler = new DroneCompiler();
			this.loader = new DroneLoader();
			this.taskRunner = new DroneTaskRunner(new DroneTaskHandlerFactory());
		}

		private DroneModule CompileAndLoadModule(DroneConfig config, LogLevel logLevel)
		{
			this.compiler.Compile(config, logLevel);
			var module = this.loader.Load(config);
			return module;
		}

		public DroneConfig LoadConfig(string configFilename)
		{
			if(string.IsNullOrWhiteSpace(configFilename))
				throw new ArgumentException("configFilename is empty or null", "configFilename");

			using (var fs = File.OpenRead(configFilename))
			{
				return store.Load(fs, configFilename);
			}
		}

		public void SaveConfig(DroneConfig config)
		{
			if(config == null)
				throw new ArgumentNullException("config");

			using (var fs = File.Open(config.Filepath, FileMode.Create, FileAccess.Write))
			{
				store.Save(config, fs);
			}
		}

		public void AddFiles(DroneConfig config, IList<string> sourceFiles, IList<string> referenceFiles)
		{
			if(config == null)
				throw new ArgumentNullException("config");

			if(sourceFiles == null)
				throw new ArgumentNullException("sourceFiles");

			if(referenceFiles == null)
				throw new ArgumentNullException("referenceFiles");

			var sourcesToAdd = sourceFiles.Except(config.SourceFiles).ToList();

			var referencesToAdd = referenceFiles.Except(config.ReferenceFiles).ToList();

			if (sourcesToAdd.Count == 0 && referencesToAdd.Count == 0)
			{
				this.log.Warn("no files added. files already exists in config");
				return;
			}

			config.SourceFiles.AddRange(sourcesToAdd);
			config.ReferenceFiles.AddRange(referencesToAdd);

			foreach (var file in sourcesToAdd.Concat(referencesToAdd))
				this.log.Info("added '{0}'", file);
		}

		public void RemoveFiles(DroneConfig config, IList<string> sourceFiles, IList<string> referenceFiles)
		{
			if(config == null)
				throw new ArgumentNullException("config");

			if(sourceFiles == null)
				throw new ArgumentNullException("sourceFiles");

			if(referenceFiles == null)
				throw new ArgumentNullException("referenceFiles");

			var sourcesRemoved = sourceFiles
				.Where(x => config.SourceFiles.Remove(x))
				.ToList();

			var referencesRemoved = referenceFiles
				.Where(x => config.ReferenceFiles.Remove(x))
				.ToList();

			if (sourcesRemoved.Count == 0 && referencesRemoved.Count == 0)
			{
				this.log.Warn("nothing to remove. files dont exists in config");
				return;
			}

			foreach (var file in sourcesRemoved.Concat(referencesRemoved))
				this.log.Info("removed '{0}'", file);
		}

		public void RunTasks(DroneConfig config, DroneFlags flags, IEnumerable<string> taskNames)
		{
			using(DroneEnvironment.PushScope(config, flags))
			{
				var module = this.CompileAndLoadModule(config, LogLevel.Debug);
				this.taskRunner.Run(module, taskNames, config, flags);
			}
		}

		public IEnumerable<DroneTask> GetTasks(DroneConfig config, DroneFlags flags, string searchPattern)
		{
			using(DroneEnvironment.PushScope(config, flags))
			{
				var module = this.CompileAndLoadModule(config, LogLevel.Debug);
				
				foreach (var task in module.Tasks)
				{
					var taskCopy = new DroneTask(task.Name, task.Dependencies);
					yield return taskCopy;
				}
			}
		}

		public void CompileTasks(DroneConfig config, LogLevel logLevel)
		{
			this.compiler.Compile(config, logLevel);
		}
	}
}