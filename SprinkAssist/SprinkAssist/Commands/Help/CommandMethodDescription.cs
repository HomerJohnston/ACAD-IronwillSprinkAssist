using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ironwill.Commands.Help
{
	[System.AttributeUsage(System.AttributeTargets.Method)]
	internal class CommandMethodDescription : System.Attribute
	{
		public string Description;

		public CommandMethodDescription(string description = "NO DESCRIPTION ENTERED")
		{
			Description = description;
		}
	}
}
