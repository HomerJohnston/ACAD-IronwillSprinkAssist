using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: CommandClass(typeof(Ironwill.Commands.SnapOverrule.XrefSnapOverruleCmd))]

namespace Ironwill.Commands.SnapOverrule
{
	internal class XrefSnapOverrule : OsnapOverruleBase
	{
		public XrefSnapOverrule()
		{
			name = "XRef Layer - No Snaps";

			SetCustomFilter();
		}

		public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes mode, IntPtr gsm, Point3d pick, Point3d last, Matrix3d view, Point3dCollection snaps, IntegerCollection geomIds)
		{
			// None! Allow no snapping
		}

		public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes mode, IntPtr gsm, Point3d pick, Point3d last, Matrix3d view, Point3dCollection snaps, IntegerCollection geomIds, Matrix3d insertion)
		{
			// None! Allow no snapping
		}

		public override bool IsContentSnappable(Entity entity)
		{
			return false;
		}

		public override bool IsApplicable(RXObject overruledSubject)
		{
			Entity entity = overruledSubject as Entity;

			if (entity == null)
			{
				return false;
			}

			return entity.Layer == Layer.XREF;
		}
	}

	internal class XrefSnapOverruleCmd
	{
		private static XrefSnapOverrule xrefSnapOverrule = new XrefSnapOverrule();

		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleXrefSnapDisable", CommandFlags.NoBlockEditor)]
		public static void ToggleXrefSnapping()
		{
			xrefSnapOverrule.ToggleEnabled();
		}
	}
}
