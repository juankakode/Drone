﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Drone.Lib.Helpers
{
	public static class ListExt
	{
		public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
		{
			if (list == null)
				throw new ArgumentNullException("list");

			if (items == null)
				throw new ArgumentNullException("items");

			foreach (var item in items)
			{
				list.Add(item);
			}
		}
	}
}
