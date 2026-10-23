using Autodesk.AutoCAD.Runtime;
using Ironwill.Commands.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: CommandClass(typeof(Ironwill.Commands.ListXData.ListXDataCmd))]

namespace Ironwill.Commands.ListXData
{
	internal class ListXDataCmd
	{
		[CommandDescription("Lists all XData (AutoCAD internal settings data) being stored in this drawing.", "Mostly intended to be used to debug SprinkAssist issues only.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ListXData", CommandFlags.NoBlockEditor)]
		public static void Main()
		{
			Session.Log("NOT IMPLEMENTED YET!");
		}
	}
}
