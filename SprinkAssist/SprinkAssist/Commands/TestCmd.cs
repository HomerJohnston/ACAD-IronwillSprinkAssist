using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.Geometry;

using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(Ironwill.TestCmd))]

namespace Ironwill
{
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