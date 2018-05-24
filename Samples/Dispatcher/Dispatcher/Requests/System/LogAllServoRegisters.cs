﻿using DynamixelSDKSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;

namespace Dispatcher.Requests.System
{
	[Serializable]
	class LogAllServoRegisters : IRequest
	{
		public object Perform()
		{
			var servos = PortPool.X.Servos;

			foreach (var servo in servos.Values)
			{
				//Ideally we want to change this later
				servo.ReadAll();

				var dataRow = new DataLogger.Registers();
				dataRow.servo = servo.ID;

				var registers = servo.Registers;
				foreach(var register in registers)
				{
					dataRow.registerValues.Add(register.Key.ToString(), register.Value.Value);
				}

				DataLogger.Database.X.Log(dataRow);
			}
			
			return new { };
		}
	}
}