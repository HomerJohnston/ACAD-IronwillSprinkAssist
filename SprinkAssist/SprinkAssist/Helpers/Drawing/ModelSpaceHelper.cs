using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.BoundaryRepresentation;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill
{
	public class ModelSpaceHelper
	{
		public static void GetObjectsWithinBoundaryCurvesOfLayerByExtents(Document document, Transaction transaction, ref List<Entity> foundEntities, string boundaryLayer, OpenMode openMode)
		{
			Session.LogDebug("GetObjectsWithinBoundary... " + boundaryLayer);
			bool log = boundaryLayer == "TEMPLATE_BASE";

			// Build up list of boundary entities
			BlockTableRecord modelSpaceBTR = transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(document.Database), OpenMode.ForWrite) as BlockTableRecord;

			if (modelSpaceBTR == null)
			{
				return;
			}

			// Get BOUNDARY CURVES
			List<Curve> boundaryCurves = new List<Curve>();

			foreach (ObjectId objectId in modelSpaceBTR)
			{
				Curve curve = transaction.GetObject(objectId, OpenMode.ForRead) as Curve;

				if (curve != null && curve.Layer == boundaryLayer)
				{
					Session.LogDebug("Adding boundary curve: " + curve.GeometricExtents.ToString());
					boundaryCurves.Add(curve);
				}
			}

			if (log)
				Session.LogDebug("Found " + boundaryCurves.Count + " boundary curves");

			// Construct REGIONS
			List<Region> allBoundaryRegions = new List<Region>();

			foreach (Curve curve in boundaryCurves)
			{
				if (!curve.Closed)
				{
					Session.Log("Skipping open boundary shape (polylines must be closed)");
					continue;
				}
				
				DBObjectCollection curves = new DBObjectCollection();
				curves.Add(curve);

				using (DBObjectCollection regions = Region.CreateFromCurves(curves))
				{
					if (regions == null || regions.Count == 0)
					{
						Session.Log("Failed to create regions from a boundary shape, ignoring");
						continue;
					}

					if (regions.Count > 1)
					{
						Session.Log("Multiple regions created for one curve (invalid geometry?), ignoring");
						continue;
					}

					allBoundaryRegions.Add(regions.Cast<Region>().First());
				}
			}

			// Construct BREPS
			List<Brep> allBoundaryBreps = new List<Brep>();
			foreach (Region region in allBoundaryRegions)
			{
				allBoundaryBreps.Add(new Brep(region));
			}

			if (allBoundaryBreps.Count == 0)
			{
				Session.Log("Warning: could not find any boundary objects on layer " + boundaryLayer + ", aborting!");
				return;
			}

			foreach (ObjectId objectId in modelSpaceBTR)
			{
				Entity entity = transaction.GetObject(objectId, openMode) as Entity;

				if (entity == null)
				{
					continue;
				}

				Extents3d extents = entity.GeometricExtents;

				Point3d min = extents.MinPoint;
				Point3d max = extents.MaxPoint;

				bool bFullyContained = false;

				List<Point3d> testPoints = new List<Point3d>()
				{
					new Point3d(min.X, min.Y, 0),
					new Point3d(min.X, max.Y, 0),
					new Point3d(max.X, max.Y, 0),
					new Point3d(max.X, min.Y, 0),
				};
				
				//if (log)
				//	Session.LogDebug("Checking " + entity.GetType().ToString());

				foreach (Brep brep in allBoundaryBreps)
				{
					PointContainment result;

					bool brepContainmentResult = true;

					foreach (Point3d point in testPoints)
					{
						//if (log)
						//	Session.LogDebug("   " + point.ToString());

						using (BrepEntity brepEntity = brep.GetPointContainment(point, out result))
						{
							if (!(brepEntity is Autodesk.AutoCAD.BoundaryRepresentation.Face))
							{
								//if (log)
								//	Session.LogDebug("   Failed containment testing");
								brepContainmentResult = false;
								break;
							}
						}
					}

					if (brepContainmentResult)
					{
						bFullyContained = true;
						break;
					}
				}

				if (bFullyContained)
				{
					foundEntities.Add(entity);
				}
			}
		}
	
		[Flags]
		public enum ERecurseFlags
		{
			None = 0,
			RecurseXrefs = 1,
			RecurseBlocks = 2,
		}

		/** Iterates all entities of a database and runs an action on them */
		public static void IterateAllEntities(Transaction transaction, Database database, ERecurseFlags recurseFlags, Action<Entity, Matrix3d> processEntityAction)
		{
			BlockTableRecord modelSpaceBTR = transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(database), OpenMode.ForRead) as BlockTableRecord;

			Session.LogDebug("Iterating model space BTR " + modelSpaceBTR.Id);

			// Gather all entities
			IterateAllEntities(transaction, modelSpaceBTR, recurseFlags, processEntityAction, Matrix3d.Identity);
		}

		/** Iterates all entities of a block table record */
		public static void IterateAllEntities(Transaction transaction, BlockTableRecord blockTableRecord, ERecurseFlags recurseFlags, Action<Entity, Matrix3d> processEntityAction, Matrix3d parentTransform)
		{
			foreach (ObjectId objectId in blockTableRecord)
			{
				Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;

				BlockReference blockReference = entity as BlockReference;
					
				if (blockReference != null && recurseFlags.HasFlag(ERecurseFlags.RecurseBlocks))
				{
					Matrix3d blockTransform = blockReference.BlockTransform;
					Matrix3d blockToWorld = parentTransform * blockTransform;

					BlockTableRecord blockBtr = transaction.GetObject(blockReference.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;

					if (blockBtr.IsFromExternalReference)
					{
						Session.Log("Found an XREF: {0}", blockBtr.Name);
					}

					IterateAllEntities(transaction, blockBtr, recurseFlags, processEntityAction, blockToWorld);
				}
				else
				{
					processEntityAction(entity, parentTransform);
				}
			}
		}



		/*
		public static void IterateEntitiesInXrefs(Transaction transaction, Database database, ERecurseFlags recurseFlags, Action<Entity, Matrix3d> processEntityAction)
		{
			Session.LogDebug("Iterating xrefs in database: " + database.CurrentSpaceId);
			database.ResolveXrefs(true, false);

			XrefGraph xrefGraph = database.GetHostDwgXrefGraph(false);

			GraphNode rootGraphNode = xrefGraph.RootNode;

			List<Database> xrefs = new List<Database>();

			for (int i = 0; i < rootGraphNode.NumOut; ++i)
			{
				XrefGraphNode xrefGraphNode = rootGraphNode.Out(i) as XrefGraphNode;

				if (xrefGraphNode == null)
				{
					continue;
				}

				BlockTableRecord xrefBlockTableRecord = transaction.GetObject(xrefGraphNode.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
				
				Session.LogDebug("Iterating xref: " + xrefGraphNode.Name);
				IterateAllEntities(transaction, xrefBlockTableRecord, recurseFlags, processEntityAction);

					
			}
		}
		*/
	}
}
