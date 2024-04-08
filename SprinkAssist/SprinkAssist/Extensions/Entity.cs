using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.GraphicsInterface;

namespace Ironwill
{
	public static class EntityExtensions
	{
		public static void Draw(this Entity entity, WorldDraw draw)
		{
			draw.Geometry.Draw(entity);
		}
	}
}
