using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Ironwill.Commands.Help
{
	[System.AttributeUsage(System.AttributeTargets.Method)]
	internal class CommandDescription : System.Attribute
	{
		public string Description;

		public string[] BulletPoints;
		/// <summary>
		/// Write a description for the command. Each parameter is one line.
		/// </summary>
		/// <param name="description"></param>
		public CommandDescription(string description, params string[] bulletPoints)
		{
			Description = description;
			BulletPoints = bulletPoints;
		}
	}
}
