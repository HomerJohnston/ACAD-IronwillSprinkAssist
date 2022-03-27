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

using Autodesk.AutoCAD.BoundaryRepresentation;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

[assembly: CommandClass(typeof(Ironwill.GenerateCalcBackground))]

namespace Ironwill
{
	public class GenerateCalcBackground
	{
		[CommandMethod("SpkAssist_GenerateCalcBackground")]
		public void GenerateCalcBackgroundCmd()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				List<Curve> boundaryCurves = new List<Curve>();
				List<Entity> candidateObjects = new List<Entity>();

				GetBoundaryCurvesAndCandidateObjects(ref boundaryCurves, ref candidateObjects);

				List<Region> allBoundaryRegions = new List<Region>();

				foreach (Curve curve in boundaryCurves)
				{
					if (!curve.Closed)
					{
						Session.WriteMessage("Skipping open boundary shape (polylines must be closed)");
						continue;
					}

					DBObjectCollection curves = new DBObjectCollection();
					
					curves.Add(curve);

					using (DBObjectCollection regions = Region.CreateFromCurves(curves))
					{
						if (regions == null || regions.Count == 0)
						{
							Session.WriteMessage("Error: Failed to create regions");
							continue;
						}
						if (regions.Count > 1)
						{
							Session.WriteMessage("Error: Multiple regions created for one curve");
							continue;
						}

						allBoundaryRegions.Add(regions.Cast<Region>().First());
					}
				}

				if (allBoundaryRegions.Count == 0)
				{
					Session.WriteMessage("No boundaries found. Did you create any polylines on " + Layer.Area_CalcBackground.Get() + " layer?");
					return;
				}

				LayerTable layerTable = transaction.GetObject(Session.GetDatabase().LayerTableId, OpenMode.ForRead) as LayerTable;

				Session.GetDatabase().Clayer = layerTable[Layer.Default.Get()];

				Session.Command("-laydel", "N", Layer.HeadCoverage.Get(), "", "Y");
				Session.Command("-laydel", "N", Layer.Wipeout.Get(), "", "Y");

				ObjectIdCollection objectIds = new ObjectIdCollection();

				List<Brep> breps = new List<Brep>();
				
				foreach (Region region in allBoundaryRegions)
				{
					breps.Add(new Brep(region));
				}

				foreach (Entity entity in candidateObjects)
				{
					List<Point3d> points = new List<Point3d>();

					if (entity is Line)
					{
						Line line = (Line)entity;
						points.Add(line.StartPoint);
						points.Add(line.EndPoint);
					}
					else if (entity is Polyline)
					{
						Polyline polyline = (Polyline)entity;

						int numVerts = polyline.NumberOfVertices;

						for (int i = 0; i < numVerts; i++)
						{
							points.Add(polyline.GetPoint3dAt(i));
						}
					}
					else if (entity is BlockReference)
					{
						BlockReference block = (BlockReference)entity;
						points.Add(block.Position);
					}

					if (points.Count == 0)
					{
						continue;
					}

					foreach (Brep brep in breps)
					{
						if (brep == null)
						{
							continue;
						}

						PointContainment result;

						int numIn = 0;

						foreach (Point3d point in points)
						{
							using (BrepEntity brepEntity = brep.GetPointContainment(point, out result))
							{
								if ((brepEntity is Autodesk.AutoCAD.BoundaryRepresentation.Face))
								{
									numIn++;
								}
								else
								{
									break;
								}
							}
						}

						if (numIn == points.Count)
						{
							objectIds.Add(entity.ObjectId);
							break;
						}
					}
				}

				foreach (Brep brep in breps)
				{
					brep.Dispose();
				}

				//Session.GetEditor().SetImpliedSelection(foundObjects.ToArray());

				using (Database tempDb = new Database(true, false))
				{
					Session.GetDatabase().Wblock(tempDb, objectIds, Point3d.Origin, DuplicateRecordCloning.Ignore);

					string docName = Session.GetDocument().Name;

					string path = Path.GetDirectoryName(docName);

					string fileName = Path.GetFileNameWithoutExtension(docName);

					string fileExtension = Path.GetExtension(docName);

					string calcSuffix = "_CalcBG";

					string fullPath = String.Empty;

					for (int i = 1; i < 999; i++)
					{
						string candidateFile = string.Format("{0}{1}{2}", fileName, calcSuffix, (i > 1) ? "_" + i.ToString() : "");

						string candidateFilePath = Path.Combine(path, candidateFile);

						candidateFilePath = Path.ChangeExtension(candidateFilePath, fileExtension);

						bool? locked = IsFileLocked(candidateFilePath);

						if (locked.HasValue && !locked.Value)
						{
							fullPath = candidateFilePath;
							break;
						}
					}

					if (fullPath == String.Empty)
					{
						Session.WriteMessage("Error: could not save calc background file");
						return;
					}

					Session.WriteMessage("Saving: " + fullPath);
					tempDb.SaveAs(fullPath, false, DwgVersion.Newest, tempDb.SecurityParameters);
				}
			}
		}

		bool? IsFileLocked(string path)
		{
			try
			{
				using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
				{
					return false;
				}
			}
			catch (IOException ioe)
			{
				int errorNum = Marshal.GetHRForException(ioe) & ((1 << 16) - 1);
				return errorNum == 32 || errorNum == 33;
			}
			catch (System.Exception e)
			{
				MessageBox.Show(e.Message, "IsFileLocked Checking");
				return null;
			}
		}

		void GetBoundaryCurvesAndCandidateObjects(ref List<Curve> foundCurves, ref List<Entity> candidateObjects)
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTableRecord modelSpaceBTR = transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Session.GetDatabase()), OpenMode.ForRead) as BlockTableRecord;

				if (modelSpaceBTR == null)
				{
					return;
				}

				foreach (ObjectId objectId in modelSpaceBTR)
				{
					Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;

					if (entity.Layer == Layer.Area_CalcBackground.Get())
					{
						Curve curve = entity as Curve;
						if (curve != null)
						{
							foundCurves.Add(curve);
						}
					}
					else
					{
						List<string> suitableLayers = new List<string>
						{
							Layer.Calculation.Get(),
							Layer.SystemDevice.Get(),
							Layer.SystemFitting.Get(),
							Layer.SystemHead.Get(),
							Layer.SystemPipe_Armover.Get(),
							Layer.SystemPipe_AuxDrain.Get(),
							Layer.SystemPipe_Branchline.Get(),
							Layer.SystemPipe_Main.Get()
						};

						if (suitableLayers.Contains(entity.Layer))
						{
							candidateObjects.Add(entity);
						}
					}
				}

				transaction.Commit();
			}
		}
	}
}
