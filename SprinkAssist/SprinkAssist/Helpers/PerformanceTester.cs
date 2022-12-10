using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace Ironwill
{
	internal class PerformanceTester
	{
		static Dictionary<string, Stopwatch> timers = new Dictionary<string, Stopwatch>();

		[System.Diagnostics.Conditional("DEBUG")]
		public static void StartMeasurement(params string[] ids)
		{
			string id = String.Join(".", ids);

			Stopwatch stopwatch;

			if (timers.TryGetValue(id, out stopwatch))
			{
				Session.LogDebug("Warning: timer " + id + " already running!");
				return;
			}

			stopwatch = Stopwatch.StartNew();
			timers.Add(id, stopwatch);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void StopMeasurement(params string[] ids)
		{
			string id = String.Join(".", ids);

			Stopwatch stopwatch;

			if (timers.TryGetValue(id, out stopwatch))
			{
				Session.LogDebug(id + " elapsed ms: " + stopwatch.ElapsedMilliseconds);

				ShutdownTimer(ids);

				return;
			}

			Session.LogDebug("No timer named " + id + " exists!");
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void ShutdownTimer(params string[] ids)
		{
			string id = String.Join(".", ids);

			Stopwatch stopwatch;

			if (timers.TryGetValue(id, out stopwatch))
			{
				stopwatch.Stop();
			}

			timers.Remove(id);
		}
	}
}
