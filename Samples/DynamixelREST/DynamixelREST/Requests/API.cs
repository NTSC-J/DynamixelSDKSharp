﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamixelREST.Requests
{
	[Serializable]
	class API : IRequest
	{
		public object Perform()
		{
			return AutoRouting.Routes;
		}
	}
}
