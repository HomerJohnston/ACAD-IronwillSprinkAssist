using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	internal class SprinkAssistCommand
	{
		protected CommandSettingsContainer settings;

		public SprinkAssistCommand()
		{
			string className = GetType().Name;

			Session.LogDebug("new SprinkAssistCommand for {0}", className);

			settings = new CommandSettingsContainer(className);
		}
	}
}
