using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Ironwill.Commands.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: CommandClass(typeof(Ironwill.Commands.SnapOverrule.SprinklerSnapOverruleCmd))]

namespace Ironwill.Commands.SnapOverrule
{
	internal class SprinklerSnapOverrule : OsnapOverruleBase
	{
		List<ObjectSnapModes> snapModes = new List<ObjectSnapModes>() { ObjectSnapModes.ModeIns, ObjectSnapModes.ModeNode };

		public SprinklerSnapOverrule()
		{
			name = "Sprinkler Head Layer - Insertion Point/Node Points Only";

			SetCustomFilter();
		}

		public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes mode, IntPtr gsm, Point3d pick, Point3d last, Matrix3d view, Point3dCollection snaps, IntegerCollection geomIds)
		{
			if (snapModes.Contains(mode))
			{
				base.GetObjectSnapPoints(entity, mode, gsm, pick, last, view, snaps, geomIds);
			}
		}

		public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes mode, IntPtr gsm, Point3d pick, Point3d last, Matrix3d view, Point3dCollection snaps, IntegerCollection geomIds, Matrix3d insertion)
		{
			if (snapModes.Contains(mode))
			{
				base.GetObjectSnapPoints(entity, mode, gsm, pick, last, view, snaps, geomIds, insertion);
			}
		}

		public override bool IsContentSnappable(Entity entity)
		{
			return false;
		}

		public override bool IsApplicable(RXObject overruledSubject)
		{
			BlockReference blockReference = overruledSubject as BlockReference;

			if (blockReference != null && blockReference.Layer == Layer.SystemHead)
			{
				return true;
			}

			return false;
		}
	}

	internal class SprinklerSnapOverruleCmd
	{
		private static SprinklerSnapOverrule sprinklerSnapOverrule = new SprinklerSnapOverrule();

		public static void Initialize()
		{
			sprinklerSnapOverrule.SetEnabled(true);
		}

		[CommandDescription("Disables most snaps on anything placed on the sprinkler heads layer.", "Leaves only the 'Insertion' and 'Node' snaps active on sprinkler head blocks when active.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleHeadSnapOverrule", CommandFlags.NoBlockEditor)]
		public static void Main()
		{
			sprinklerSnapOverrule.ToggleEnabled();
		}
	}
}
