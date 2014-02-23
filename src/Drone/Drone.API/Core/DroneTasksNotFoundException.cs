using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Drone.API.Core
{
	[Serializable]
	public class DroneTasksNotFoundException : Exception
	{
		public DroneTasksNotFoundException()
		{
		}

		public DroneTasksNotFoundException(string message) : base(message)
		{
		}

		public DroneTasksNotFoundException(string message, Exception inner) : base(message, inner)
		{
		}

		protected DroneTasksNotFoundException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

		public static DroneTasksNotFoundException Get(IEnumerable<string> taskNames)
		{
			var ex = new DroneTasksNotFoundException("unable to find tasks in module by name");

			ex.Data["task-names"] = string.Join(", ", taskNames);

			return ex;
		}
	}
}