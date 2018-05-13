﻿using DynamixelSDKSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace Dispatcher
{
	[Serializable]
	class Scheduler
	{
		//Singleton
		public static readonly Scheduler X = new Scheduler();

		[Serializable]
		public class SettingsType
		{
			public int Sleep { get; set; } = 100;
		}

		[Serializable]
		public class Schedule
		{
			public double Period { get; set; } = 0.0;
			public string Action { get; set; }
			public bool OnStart { get; set; } = false;

			public DateTime LastPerformed = DateTime.Now;

			public void Perform()
			{
				var httpRequest = (HttpWebRequest)WebRequest.Create(String.Format("http://localhost:{0}/{1}", Program.Port, this.Action));
				httpRequest.Method = "GET";
				var httpResponse = (HttpWebResponse)httpRequest.GetResponse();

				this.LastPerformed = DateTime.Now;
			}
		}

		public const string Filename = "Schedule.json";
		Thread Thread;
		bool Joining = false;

		public List<Schedule> Schedules { get; private set; } = new List<Schedule>();
		public SettingsType Settings { get; private set; } = new SettingsType();
		public bool Enabled { get; set; } = true;

		public void Start()
		{
			try
			{
				//load the schedule
				this.Load(Scheduler.Filename);

				//start the thread
				this.Thread = new Thread(this.ThreadedFunction);
				this.Thread.IsBackground = true;
				this.Thread.Name = "Scheduler";
				this.Thread.Start();
			}
			catch (Exception e)
			{
				Logger.Log(Logger.Level.Error, e);
			}
		}

		void Load(string filename)
		{
			//load list of constraints
			using (StreamReader file = new StreamReader(filename))
			{
				var json = file.ReadToEnd();
				JsonConvert.PopulateObject(json, this, ProductDatabase.JsonSerializerSettings);
			}
		}


		void ThreadedFunction()
		{
			//perform any startup actions
			foreach(var schedule in this.Schedules)
			{
				if(schedule.OnStart)
				{
					schedule.Perform();
				}
			}

			while(!this.Joining)
			{
				if(this.Enabled)
				{
					var now = DateTime.Now;

					//perform all schedules
					foreach (var schedule in this.Schedules)
					{
						if ((now - schedule.LastPerformed).TotalSeconds > schedule.Period)
						{
							schedule.Perform();
						}
					}
				}
				

				//sleep
				Thread.Sleep(this.Settings.Sleep);
			}
		}
	}
}