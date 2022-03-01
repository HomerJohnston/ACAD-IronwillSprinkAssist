using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(Ironwill.AboutSprinkAssist))]

namespace Ironwill
{
	class AboutSprinkAssist
	{
		[CommandMethod("AboutSprinkAssist")]
		public void AboutSprinkAssistCmd()
		{
			Session.WriteMessage("Built on __DATE__");
		}
	}
}
