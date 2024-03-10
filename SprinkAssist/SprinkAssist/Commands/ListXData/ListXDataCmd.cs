using Autodesk.AutoCAD.Runtime;
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
		[CommandMethod("SpkAssist", "ListXData", CommandFlags.NoBlockEditor)]
		public static void Main()
		{
			Session.Log("Hello world");
		}
	}
}
