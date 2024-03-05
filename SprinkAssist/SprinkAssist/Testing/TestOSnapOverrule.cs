using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Windows.Documents;

[assembly: CommandClass(typeof(Ironwill.Commands.DisableObjectSnaps.Commands))]

namespace Ironwill.Commands
{
	internal class DisableObjectSnaps : SprinkAssistCommand
	{
		public class IFEOsnapOverruleBase : OsnapOverrule
		{
			private bool active = false;

			protected string name = string.Empty;

			public void SetEnabled(bool newState)
			{
				if (newState)
				{
					if (name != string.Empty)
					{
						Session.Log($"Enabling snap overrule: {name}");
					}

					AddOverrule(RXObject.GetClass(typeof(Entity)), this, false);
					Overruling = true;
					active = true;
				}
				else
				{
					if (name != string.Empty)
					{
						Session.Log($"Disabling snap overrule: {name}");
					}

					RemoveOverrule(RXObject.GetClass(typeof(Entity)), this);
					active = false;
				}
			}

			public void ToggleEnabled()
			{
				if (active)
				{
					SetEnabled(false);
				}
				else
				{
					SetEnabled(true);
				}
			}
		}

		internal class SprinklerSnapOverrule : IFEOsnapOverruleBase
		{
			List<ObjectSnapModes> snapModes = new List<ObjectSnapModes>() { ObjectSnapModes.ModeIns, ObjectSnapModes.ModeNode };

			public SprinklerSnapOverrule()
			{
				name = "Sprinkler Head Layer - Insertion Point Only";
			
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

		internal class XrefSnapOverrule : IFEOsnapOverruleBase
		{
			public XrefSnapOverrule()
			{
				name = "XRef Layer - No Snaps";

				SetCustomFilter();
			}

			public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes mode, IntPtr gsm, Point3d pick, Point3d last, Matrix3d view, Point3dCollection snaps, IntegerCollection geomIds)
			{
				//base.GetObjectSnapPoints(entity, mode, gsm, pick, last, view, snaps, geomIds);
			}

			public override void GetObjectSnapPoints(Entity entity, ObjectSnapModes mode, IntPtr gsm, Point3d pick, Point3d last, Matrix3d view, Point3dCollection snaps, IntegerCollection geomIds, Matrix3d insertion)
			{
				//base.GetObjectSnapPoints(entity, mode, gsm, pick, last, view, snaps, geomIds, insertion);
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

		internal class Commands
		{
			private static SprinklerSnapOverrule sprinklerSnapOverrule = new SprinklerSnapOverrule();

			private static XrefSnapOverrule xrefSnapOverrule = new XrefSnapOverrule();

			public static void EnableHeadSnapping()
			{
				sprinklerSnapOverrule.SetEnabled(true);
			}

			[CommandMethod("SpkAssist", "ToggleHeadSnapOverrule", CommandFlags.NoBlockEditor)]
			public static void ToggleHeadSnapping()
			{
				sprinklerSnapOverrule.ToggleEnabled();
			}

			[CommandMethod("SpkAssist", "ToggleXrefSnapDisable", CommandFlags.NoBlockEditor)]
			public static void ToggleXrefSnapping()
			{
				xrefSnapOverrule.ToggleEnabled();
			}
		}
	}
}
