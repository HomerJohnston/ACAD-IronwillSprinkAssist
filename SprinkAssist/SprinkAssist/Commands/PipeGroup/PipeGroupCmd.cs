using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: CommandClass(typeof(Ironwill.Commands.PipeGroup.PipeGroupCmd))]

namespace Ironwill.Commands.PipeGroup
{
	internal class PipeGroupCmd
	{
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "PipeGroup", CommandFlags.NoBlockEditor)]
		public static void Main()
		{
		}
	}
}
