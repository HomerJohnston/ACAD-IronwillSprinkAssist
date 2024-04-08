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
using Ironwill.Commands.Help;

[assembly: CommandClass(typeof(Ironwill.Commands.GenerateCalcBackground.GenerateCalcBackgroundCmd))]

namespace Ironwill.Commands.GenerateCalcBackground
{
	internal class GenerateCalcBackgroundCmd : SprinkAssistCommand
	{
		CommandSetting<bool> includeXREF;

		public GenerateCalcBackgroundCmd() 
		{
			includeXREF = settings.RegisterNew("IncludeXref", true);
		}

		[CommandDescription("Toggles whether or not to include xref data in generated calc background.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "ToggleCalcBackgroundXRef", CommandFlags.NoBlockEditor | CommandFlags.Modal | CommandFlags.NoUndoMarker)]
		public void ToggleIncludeXRef()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				bool current = includeXREF.Get(transaction);

				includeXREF.Set(transaction, !current);

				Session.Log($"Include XRef in Calc Background: {!current}");

				transaction.Commit();
			}
		}

		[CommandMethod(SprinkAssist.CommandMethodPrefix, "TestTest", CommandFlags.NoBlockEditor | CommandFlags.Modal | CommandFlags.NoUndoMarker)]
		public void TestTest()
		{
			Document document = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;

			using (Transaction ttt = document.TransactionManager.StartTransaction())
			{
				using (Transaction transaction = document.TransactionManager.StartTransaction())
				{
					LayerHelper.SetCurrentLayer(transaction, Layer.Default);

					//document.Editor.Command("-laydel", "N", Layer.HeadCoverage.Get(), "", "Y");
					//document.Editor.Command("-laydel", "N", Layer.HeadCoverage_Fill.Get(), "", "Y");
					//document.Editor.Command("-laydel", "N", Layer.Wipeout.Get(), "", "Y");

					Database database = document.Database;

					LayerDelete(transaction, database, Layer.HeadCoverage);
					LayerDelete(transaction, database, Layer.HeadCoverage_Fill);
					LayerDelete(transaction, database, Layer.Wipeout);

					ObjectIdCollection xrefCollection = new ObjectIdCollection();
					
					using (XrefGraph xrefGraph = database.GetHostDwgXrefGraph(false))
					{
						int numNodes = xrefGraph.NumNodes;

						for (int i = 0; i < numNodes; i++)
						{
							XrefGraphNode xrefGraphNode = xrefGraph.GetXrefNode(i);

							if (xrefGraphNode.Database.Filename.Equals(database.Filename))
							{
								continue;
							}

							if (xrefGraphNode.XrefStatus != XrefStatus.Resolved)
							{
								continue;
							}

							xrefCollection.Add(xrefGraphNode.BlockTableRecordId);
						}
					}

					if (xrefCollection.Count > 0)
					{
						database.BindXrefs(xrefCollection, true);
					}                    

					transaction.Commit();
				}

				ttt.Abort();
			}
		}

		private string LayerDelete(Transaction transaction, Database database, string layerName)
		{
			if (layerName == "0")
			{
				return "Layer '0' cannot be deleted.";
			}

			LayerTable layerTable = transaction.GetObject(database.LayerTableId, OpenMode.ForRead) as LayerTable;

			if (!layerTable.Has(layerName))
			{
				return "Layer '" + layerName + "' not found.";
			}

			try
			{
				var layerId = layerTable[layerName];
				
				if (database.Clayer == layerId)
				{
					return "Current layer cannot be deleted.";
				}

				LayerTableRecord layer = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForWrite);
				layer.IsLocked = false;

				BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);

				foreach (var btrId in blockTable)
				{
					var block = (BlockTableRecord)transaction.GetObject(btrId, OpenMode.ForRead);
					foreach (var entId in block)
					{
						var ent = (Entity)transaction.GetObject(entId, OpenMode.ForRead);
						if (ent.Layer == layerName)
						{
							ent.UpgradeOpen();
							ent.Erase();
						}
					}
				}

				layer.Erase();
					
				return "Layer '" + layerName + "' have been deleted.";
			}
			catch (System.Exception e)
			{
				return "Error: " + e.Message;
			}
		}

		private List<Entity> TestXref(Transaction transaction)
		{
			List<Entity> entities = new List<Entity>();

			Database database = Session.GetDatabase();
			database.ResolveXrefs(true, false);

			XrefGraph xrefGraph = database.GetHostDwgXrefGraph(false);

			GraphNode root = xrefGraph.RootNode;

			List<Database> xrefs = new List<Database>();

			for (int i = 0; i < xrefGraph.NumNodes; i++) 
			{
				XrefGraphNode xrefGraphNode = xrefGraph.GetXrefNode(i);

				Database xrefDatabase = null;

				if (xrefGraphNode != null)
				{
					xrefDatabase = new Database(false, true);

					xrefDatabase.ReadDwgFile(xrefGraphNode.Database.Filename, System.IO.FileShare.Read, true, null);

					xrefs.Add(xrefDatabase);
				}

				xrefGraphNode.Dispose();

				if (xrefDatabase != null)
				{
					using (Transaction transaction2 = Session.StartTransaction())
					{
						BlockTable blockTable = transaction2.GetObject(xrefDatabase.BlockTableId, OpenMode.ForRead) as BlockTable;

						BlockTableRecord blockTableRecord = transaction2.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

						foreach (ObjectId id in blockTableRecord)
						{
							Entity entity = transaction2.GetObject(id, OpenMode.ForRead) as Entity;

							LayerTable layerTable = transaction2.GetObject(xrefDatabase.LayerTableId, OpenMode.ForRead) as LayerTable;

							if (entity != null)
							{
								var entityType = entity.GetType();

								if (entity is Line)
								{
									entities.Add(entity);
								}
							}
						}

						transaction2.Commit();
					}
				}
			}

			return entities;
		}

		[CommandDescription("Exports the current sprinkler system data into a separate DWG file.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "GenerateCalcBackground", CommandFlags.NoBlockEditor | CommandFlags.Modal | CommandFlags.NoUndoMarker)]
		public void Main()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				LayerHelper.SetCurrentLayer(transaction, Layer.Default);

				EraseIrrelevantSprinklerLayers();

				if (includeXREF.Get(transaction))
				{
					// Unlock the layer
					LayerHelper.SetLocked(transaction, false, Layer.XREF);

					// Bind all XREFs
					Document document = Session.GetDocument();
					Database database = Session.GetDatabase();

					ObjectIdCollection xrefCollection = new ObjectIdCollection();

					using (XrefGraph xrefGraph = database.GetHostDwgXrefGraph(false))
					{
						int numNodes = xrefGraph.NumNodes;

						for (int i = 0; i < numNodes; i++)
						{
							XrefGraphNode xrefGraphNode = xrefGraph.GetXrefNode(i);

							if (xrefGraphNode.Database.Filename.Equals(database.Filename))
							{
								continue;
							}

							if (xrefGraphNode.XrefStatus != XrefStatus.Resolved)
							{
								continue;
							}

							xrefCollection.Add(xrefGraphNode.BlockTableRecordId);
						}
					}
					
					if (xrefCollection.Count > 0)
					{
						database.BindXrefs(xrefCollection, true);
					}

					Session.Log("Bound " + xrefCollection.Count.ToString() + " xrefs");

					// Explode xref block
					BlockTableRecord modelSpaceBTR = transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(Session.GetDatabase()), OpenMode.ForWrite) as BlockTableRecord;

					if (modelSpaceBTR == null)
					{
						return;
					}

					foreach (ObjectId objectId in modelSpaceBTR)
					{
						Entity entity = transaction.GetObject(objectId, OpenMode.ForRead) as Entity;

						if (entity is BlockReference && entity.Layer == Layer.XREF)
						{
							DBObjectCollection entityContents = new DBObjectCollection();
							entity.Explode(entityContents);

							if (entityContents.Count > 0)
							{
								entity.UpgradeOpen();
								entity.Erase();

								foreach (DBObject explodedObj in entityContents)
								{
									Entity explodedEnt = explodedObj as Entity;

									if (explodedObj == null)
									{
										continue;
									}

									modelSpaceBTR.AppendEntity(explodedEnt);
									transaction.AddNewlyCreatedDBObject(explodedEnt, true);
								}
							}
						}
					}
				}

				List<Entity> objectsInBounds = FindObjectsInBounds(transaction);

                Dictionary<string, int> layerCounts = new Dictionary<string, int>();

				if (objectsInBounds.Count == 0)
				{
					Session.Log($"No candidate objects found. Did you enclose any drawing objects within polylines on {Layer.Area_CalcBackground.Get()} layer?");
					return;
				}

				foreach (Entity entity in objectsInBounds)
				{
					if (!layerCounts.ContainsKey(entity.Layer))
					{
						layerCounts.Add(entity.Layer, 0);
					}

					layerCounts[entity.Layer]++;
				}

				foreach (var kvp in layerCounts)
				{
					Session.Log($"{kvp.Key}: {kvp.Value}");
				}

				ObjectIdCollection objectIDsToExport = FilterObjectsToExport(objectsInBounds, transaction);

				Export(objectIDsToExport);

				transaction.Commit();
            }

			// There is some sort of bug in .net, deleting layers and then aborting the transaction causes some sort of corruption which crashes autocad. Instead of aborting the above transaction, we commit the transaction and then undo it normally.
			Session.Log("");
			Session.Command("undo", "1");
        }

		private void EraseIrrelevantSprinklerLayers()
		{
			using (Transaction t = Session.StartTransaction())
			{
				Session.Command("-laydel", "N", Layer.HeadCoverage.Get(), "", "Y");
				Session.Command("-laydel", "N", Layer.HeadCoverage_Fill.Get(), "", "Y");
				Session.Command("-laydel", "N", Layer.Wipeout.Get(), "", "Y");

				t.Commit();
			}
		}

		private List<Entity> FindObjectsInBounds(Transaction transaction)
		{
			List<Entity> objectsInBounds = new List<Entity>();

			ModelSpaceHelper.GetObjectsWithinBoundaryCurvesOfLayerByExtents(Session.GetDocument(), transaction, ref objectsInBounds, Layer.Area_CalcBackground, OpenMode.ForRead);

			Session.Log($"Found {objectsInBounds.Count} objects");

			return objectsInBounds;
		}

		private ObjectIdCollection FilterObjectsToExport(List<Entity> candidateEntities, Transaction transaction)
		{
			ObjectIdCollection objectIDsToExport = new ObjectIdCollection();

			List<string> sprinklerWhitelist = new List<string>
			{
				Layer.Calculation.Get(),
				Layer.SystemDevice.Get(),
				Layer.SystemFitting.Get(),
				Layer.SystemHead.Get(),
				Layer.SystemPipe_Armover.Get(),
				Layer.SystemPipe_AuxDrain.Get(),
				Layer.SystemPipe_Branchline.Get(),
				Layer.SystemPipe_Main.Get(),
			};

			List<string> backgroundWhitelist = new List<string>() // TODO make this UI driven
			{
				"WALL",
				"PARTITION",
				"BEAM",
				"JOIST",
				"COL",
				"RCP",
				"ROOF",
				"DOOR",
				"GLAZ",
				"CEILING",
				"CLG",
				"CLNG",
			};

			int sprinklerCount = 0;
			int backgroundCount = 0;

			foreach (Entity entity in candidateEntities)
			{
				// Skip hatch objects, unless they might be representing columns
				if (entity is Hatch && !entity.Layer.ToUpper().Contains("COL"))
				{
					continue;
				}

				if (entity.Layer.StartsWith(Layer.Pfix))
				{
					if (sprinklerWhitelist.Any(s => s == entity.Layer))
					{
						objectIDsToExport.Add(entity.Id);
						sprinklerCount++;
						
					}
				}
				else
				{
					if (backgroundWhitelist.Any(s => entity.Layer.ToUpper().Contains(s)))
					{
						objectIDsToExport.Add(entity.Id);
						backgroundCount++;
					}
				}
			}

			Session.Log($"Filtered to {objectIDsToExport.Count} objects; {sprinklerCount} sprinkler entities, {backgroundCount} background entities");

			return objectIDsToExport;
		}

		private void Export(ObjectIdCollection objectIDsToExport)
		{
			using (Database tempDb = new Database(true, false))
			{
				Session.GetDatabase().Wblock(tempDb, objectIDsToExport, Point3d.Origin, DuplicateRecordCloning.Ignore);

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
					Session.Log("Error: could not save calc background file");
					return;
				}

				Session.Log("Saving: " + fullPath);

				tempDb.SaveAs(fullPath, false, DwgVersion.Newest, tempDb.SecurityParameters);
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
	}
}
