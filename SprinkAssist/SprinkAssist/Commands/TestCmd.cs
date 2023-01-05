using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;

using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

[assembly: CommandClass(typeof(Ironwill.Commands.TestCmd))]
[assembly: CommandClass(typeof(Ironwill.Commands.TestClone))]
[assembly: CommandClass(typeof(ContextMenuApplication.Commands))]

namespace Ironwill.Commands
{
	public class TestClone
	{
		[CommandMethod("TestClone")]
		public static void TestCloneCommand()
		{
			Database currentDB = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				ObjectId blockTableId = currentDB.BlockTableId;
				BlockTable blockTable = transaction.GetObject(blockTableId, OpenMode.ForWrite) as BlockTable;

				foreach (ObjectId btrId in blockTable)
				{
					BlockTableRecord block = transaction.GetObject(btrId, OpenMode.ForRead) as BlockTableRecord;

					//.//BlockTableRecord clonedBlock = block.Clone();
				}


				//ObjectId blockRefId = blockReference.ObjectId;

				
				
				transaction.Commit();
			}
		}
	}

	public class TestCmd
	{
		[CommandMethod("EntArray")]
		public static void RunMyCommand()
		{
			try
			{
				using (var arrayJig = new EntityArrayJig())
				{
					arrayJig.DrawArray();
				}
			}
			catch
			{
			}
		}
	}

	public class EntityArrayJig : DrawJig, IDisposable
	{
		private Document _dwg;
		private Editor _ed;
		private Database _db;

		private ObjectId _baseEntity = ObjectId.Null;

		private Entity _baseGhost = null;
		private List<Tuple<Entity, Matrix3d>> _ghostEntities = new List<Tuple<Entity, Matrix3d>>();

		private Point3d _basePoint;
		private Point3d _currPoint;
		private double _distance = 0.0;
		private int _colorIndex = 2;

		public void Dispose()
		{
			if (_baseGhost != null) _baseGhost.Dispose();
			ClearGhostEntities();
		}

		public void DrawArray()
		{
			_dwg = Application.DocumentManager.MdiActiveDocument;
			_ed = _dwg.Editor;
			_db = _dwg.Database;
			if (!SelectBaseEntity(out _baseEntity, out _basePoint, out _distance)) return;
			_baseGhost = CreateBaseGhost();
			_currPoint = _basePoint;

			var res = _ed.Drag(this);
			if (res.Status == PromptStatus.OK && _ghostEntities.Count > 0)
			{
				using (var tran = _db.TransactionManager.StartTransaction())
				{
					var space = (BlockTableRecord)tran.GetObject(
						_db.CurrentSpaceId, OpenMode.ForWrite);
					var baseEnt = (Entity)tran.GetObject(_baseEntity, OpenMode.ForRead);
					for (int i = 0; i < _ghostEntities.Count; i++)
					{
						CreateEntityCopy(baseEnt, space, i, tran);
					}

					tran.Commit();
				}
			}
		}

		protected override SamplerStatus Sampler(JigPrompts prompts)
		{
			var opt = new JigPromptPointOptions("\nSelect array's end:");
			opt.BasePoint = _basePoint;
			opt.UseBasePoint = true;
			opt.Cursor = CursorType.RubberBand;
			var res = prompts.AcquirePoint(opt);
			if (res.Status == PromptStatus.OK)
			{
				if (res.Value == _currPoint)
				{
					return SamplerStatus.NoChange;
				}
				else
				{
					_currPoint = res.Value;
					return SamplerStatus.OK;
				}
			}
			else
			{
				return SamplerStatus.Cancel;
			}
		}

		protected override bool WorldDraw(WorldDraw draw)
		{
			RecreateGhosts();

			foreach (var item in _ghostEntities)
			{
				draw.Geometry.Draw(item.Item1);
			}

			return true;
		}

		#region private methods

		private bool SelectBaseEntity(out ObjectId baseEnt, out Point3d basePoint, out double distance)
		{
			baseEnt = ObjectId.Null;
			basePoint = Point3d.Origin;
			distance = 0.0;

			var res = _ed.GetEntity(
				"\nSelect an entity:");
			if (res.Status == PromptStatus.OK)
			{
				baseEnt = res.ObjectId;
				basePoint = res.PickedPoint;

				var pOpt = new PromptPointOptions(
					$"\nBase point: {basePoint}. Select different base point:");
				pOpt.AllowNone = true;
				pOpt.AppendKeywordsToMessage = true;
				pOpt.Keywords.Add("Continue");
				pOpt.Keywords.Add("Cancel");
				pOpt.Keywords.Default = "Continue";

				bool ptOk = false;
				var pRes = _ed.GetPoint(pOpt);
				if (pRes.Status == PromptStatus.OK)
				{
					basePoint = pRes.Value;
					ptOk = true;
				}
				else if (pRes.Status == PromptStatus.Keyword)
				{
					if (pRes.StringResult == "Continue") ptOk = true;
				}

				if (ptOk)
				{
					var dOpt = new PromptDistanceOptions(
						"\nPick/Enter the distance between entities:");
					dOpt.AllowNegative = false;
					dOpt.AllowZero = false;
					dOpt.UseBasePoint = true;
					dOpt.BasePoint = basePoint;
					dOpt.UseDashedLine = true;

					var dRes = _ed.GetDistance(dOpt);
					if (dRes.Status == PromptStatus.OK)
					{
						distance = dRes.Value;
						return true;
					}
				}
			}

			return false;
		}

		private Entity CreateBaseGhost()
		{
			Entity ghost = null;
			using (var tran = _db.TransactionManager.StartTransaction())
			{
				var ent = (Entity)tran.GetObject(_baseEntity, OpenMode.ForRead);
				ghost = ent.Clone() as Entity;
				ghost.ColorIndex = _colorIndex;
				tran.Commit();
			}
			return ghost;
		}

		private void ClearGhostEntities()
		{
			foreach (var item in _ghostEntities)
			{
				item.Item1.Dispose();
			}
			_ghostEntities.Clear();
		}

		private void RecreateGhosts()
		{
			ClearGhostEntities();
			using (var line = new Line(_basePoint, _currPoint))
			{
				var l = _distance;
				while (l < line.Length)
				{
					var pt = line.GetPointAtDist(l);
					var mt = Matrix3d.Displacement(_basePoint.GetVectorTo(pt));
					var ghost = _baseGhost.Clone() as Entity;
					ghost.ColorIndex = _colorIndex;
					ghost.TransformBy(mt);
					_ghostEntities.Add(new Tuple<Entity, Matrix3d>(ghost, mt));
					l += _distance;
				}
			}
		}

		private void CreateEntityCopy(Entity sourceEntity, BlockTableRecord space, int index, Transaction tran)
		{
			var newEnt = sourceEntity.Clone() as Entity;
			if (newEnt != null)
			{
				var mt = _ghostEntities[index].Item2;
				newEnt.TransformBy(mt);
				space.AppendEntity(newEnt);
				tran.AddNewlyCreatedDBObject(newEnt, true);
			}
		}

		#endregion
	}
}










namespace ContextMenuApplication
{
	public class Commands : IExtensionApplication
	{
		[CommandMethod("ContextMenuExtTest")]
		static public void ContextMenuExtTest()
		{
			ContextMenuExtension contectMenu = new ContextMenuExtension();
			
			MenuItem item0 = new MenuItem("Line context menu");
			
			contectMenu.MenuItems.Add(item0);

			MenuItem Item1 = new MenuItem("Test1");

			Item1.Click += new EventHandler(Test1_Click);

			item0.MenuItems.Add(Item1);

			MenuItem Item2 = new MenuItem("Test2");

			Item2.Click += new EventHandler(Test2_Click);

			item0.MenuItems.Add(Item2);

			//for custom entity, replace the "Line" with .NET
			//(managed) wrapper of custom entity

			Application.AddObjectContextMenuExtension(Line.GetClass(typeof(Line)), contectMenu);
		}

		static void Test1_Click(object sender, EventArgs e)
		{
			Application.ShowAlertDialog("Test1 clicked\n");
		}

		static async void Test2_Click(object sender, EventArgs e)
		{
			// Application.ShowAlertDialog("Test2 clicked\n");
			
			var dm = Application.DocumentManager;
			var doc = dm.MdiActiveDocument;
			var ed = doc.Editor;

			// Get the selected objects
			var psr = ed.GetSelection();

			if (psr.Status != PromptStatus.OK)
				return;

			try
			{
				// Ask AutoCAD to execute our command in the right context
				await dm.ExecuteInCommandContextAsync(
				  async (obj) =>
				  {
					  if (psr.Value.Count > 0);
					  
					  await Ironwill.Session.AsyncCommand(".-bedit", "S_Head01");
					  //await System.Threading.Tasks.Task.Delay(4000);
					  await Ironwill.Session.AsyncCommand("bsave");
					  await Ironwill.Session.AsyncCommand("bclose");
					  
					  //await ed.CommandAsync(".-bedit", "S_Head_TEMPLATE");
					  //await ed.CommandAsync(".-bclose");
					  
				  },
				  null
				);
			}

			catch (System.Exception ex)
			{
				ed.WriteMessage("\nException: {0}\n", ex.Message);
			}
		}






		public void Initialize()
		{
			Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

			ed.WriteMessage("Hello WOrld!!!");

			Ironwill.Session.Log("Initialize!!!");
			ScaleMenu.Attach();
		}

		public void Terminate()
		{
			Ironwill.Session.Log("TERMINATE!!");
			ScaleMenu.Detach();
		}
	}

	public class ScaleMenu
	{
		private static ContextMenuExtension cme;

		public static void Attach()
		{
			if (cme == null)
			{
				cme = new ContextMenuExtension();
				MenuItem mi = new MenuItem("Scale by 5");
				mi.Click += new EventHandler(OnScale);
				cme.MenuItems.Add(mi);
			}

			RXClass rxc = Entity.GetClass(typeof(Entity));
			Application.AddObjectContextMenuExtension(Polyline.GetClass(typeof(Polyline)), cme);
		}

		public static void Detach()
		{
			RXClass rxc = Entity.GetClass(typeof(Entity));
			Application.RemoveObjectContextMenuExtension(rxc, cme);
		}

		private static async void OnScale(Object o, EventArgs e)
		{
			var dm = Application.DocumentManager;
			var doc = dm.MdiActiveDocument;
			var ed = doc.Editor;

			// Get the selected objects
			var psr = ed.GetSelection();

			if (psr.Status != PromptStatus.OK)
				return;

			try
			{
				// Ask AutoCAD to execute our command in the right context
				await dm.ExecuteInCommandContextAsync(
				  async (obj) =>
				  {
				  // Scale the selected objects by 5 relative to 0,0,0
					  await ed.CommandAsync("._scale", psr.Value, "", Point3d.Origin, 5);
				  },
				  null
				);
			}

			catch (System.Exception ex)
			{
				ed.WriteMessage("\nException: {0}\n", ex.Message);
			}
		}
	}
}