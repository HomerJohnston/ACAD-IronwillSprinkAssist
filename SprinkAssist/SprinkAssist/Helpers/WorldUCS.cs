using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.AutoCAD.Geometry;

namespace Ironwill
{
	public class WorldUCS : IDisposable
	{
		Matrix3d ucs;

		public WorldUCS()
		{
			ucs = Session.GetEditor().CurrentUserCoordinateSystem;
			Session.GetEditor().CurrentUserCoordinateSystem = Matrix3d.Identity;
		}

		public void Dispose()
		{
			Session.GetEditor().CurrentUserCoordinateSystem = ucs;
		}
	}
}
