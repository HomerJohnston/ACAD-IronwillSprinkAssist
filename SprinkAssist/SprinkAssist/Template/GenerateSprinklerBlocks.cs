using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GenericParsing;
using Autodesk.AutoCAD.Runtime;
using Ironwill.Commands;
using System.Reflection;
using System.IO;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

[assembly: CommandClass(typeof(Ironwill.Generation.GenerateSprinklerBlocks))]

namespace Ironwill.Generation
{
	internal class GenerateSprinklerBlocks : SprinkAssistCommand
	{
		[CommandMethod("GenTest")]
		public void RuntimeArgumentHandle()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string dllFile = Uri.UnescapeDataString(uri.Path);

			string resourcesFolder = Path.Combine(Path.GetDirectoryName(dllFile), "..", "Resources");

			Session.Log(resourcesFolder);
			
			using (GenericParser parser = new GenericParser())
			{
				string Test = Path.Combine(resourcesFolder, "Test.csv");
				parser.SetDataSource(Test);
				parser.ColumnDelimiter = ',';
				parser.SkipStartingDataRows = 1;

				List<string> headBases = new List<string>();
				List<string> headTypes = new List<string>();
				List<string> headTemps = new List<string>();

				List<string> sidewallBases = new List<string>();
				List<string> sidewallTypes = new List<string>();
				List<string> sidewallTemps = new List<string>();

				List<string> atticBases = new List<string>();

				while (parser.Read())
				{
					if (parser[0] != string.Empty)
						headBases.Add(parser[0]);
					
					if (parser[1] != string.Empty)
						headTypes.Add(parser[1]);

					if (parser[2] != string.Empty)
						headTemps.Add(parser[2]);

					if (parser[3] != string.Empty)
						sidewallBases.Add(parser[3]);

					if (parser[4] != string.Empty)
						sidewallTypes.Add(parser[4]);

					if (parser[5] != string.Empty)
						sidewallTemps.Add(parser[5]);

					if (parser[6] != string.Empty)
						atticBases.Add(parser[6]);
				}

				List<string> sprinklerBlocks = new List<string>();

				foreach (string headBase in headBases)
				{
					string blockName = headBase.Replace("SprinklerBase", "S"); ;

					sprinklerBlocks.Add(blockName);

					foreach (string headType in headTypes)
					{
						string subtypeBlockName = blockName + "_" + headType.Substring(headType.LastIndexOf('_'));

						sprinklerBlocks.Add(subtypeBlockName);

						foreach (string headTemp in headTemps)
						{
							sprinklerBlocks.Add(subtypeBlockName + "_" + headTemp.Substring(headTemp.LastIndexOf('_')));
						}
					}
				}

				foreach (string blockName in sprinklerBlocks)
				{
					Session.Log(blockName);
				}

				BuildBlock("", "");
			}
		}

		void BuildBlock(string blockName, params string[] decorators)
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = transaction.GetObject(Session.GetDatabase().BlockTableId, OpenMode.ForRead) as BlockTable;

				BlockTableRecord blockTableRecord = null;

				if (blockTable.Has(blockName))
				{
					blockTableRecord = transaction.GetObject(blockTable[blockName], OpenMode.ForWrite) as BlockTableRecord;
				}
				else 
				{
					blockTableRecord = new BlockTableRecord();
					blockTableRecord.Name = blockName;
				}





				foreach (ObjectId blockId in blockTable)
				{
					//BlockTableRecord blockTableRecord = (BlockTableRecord)transaction.GetObject(blockId, OpenMode.ForRead);

					//blockTableRecord
					if (blockTableRecord.IsLayout)
					{
						continue;
					}

					//Session.Log(blockTableRecord.Name);
				}


				/*
				try
				{
					SymbolUtilityServices.ValidateSymbolName(blockName, false);
				}
				catch
				{
					if (blockTable.Has(blockName))
					{
					}
					else
					{

					}
				}
				*/
			}
		}

		[CommandMethod("CopyXC")]
		public void testCopyBlockReference()
		{
			Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
			Database db = doc.Database;
			Editor ed = doc.Editor;
			using (Transaction tr = db.TransactionManager.StartTransaction())
			{
				BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
				PromptEntityOptions peo = new PromptEntityOptions("\nSelect main block instance to copy: ");
				peo.SetRejectMessage("\nMust be a type of the BlockReference!");
				peo.AddAllowedClass(typeof(BlockReference), true);
				PromptEntityResult per = ed.GetEntity(peo);

				if (per.Status != PromptStatus.OK) return;

				BlockReference bref = (BlockReference)tr.GetObject(per.ObjectId, OpenMode.ForWrite);
				BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bref.BlockTableRecord, OpenMode.ForRead);
				ed.WriteMessage("\nBlock Selected with name: {0}", btr.Name);
				PromptStringOptions pso = new PromptStringOptions("\nEnter new block name: ");
				pso.AllowSpaces = true;
				PromptResult sres = ed.GetString(pso);
				if (sres.Status != PromptStatus.OK) return;
				string newname = sres.StringResult;
				if (bt.Has(newname))
				{
					ed.WriteMessage("\nBlock with name: {0} already exist, try again", newname);
					return;
				}

				ObjectIdCollection ids = new ObjectIdCollection();
				foreach (ObjectId id in btr)
				{
					ids.Add(id);
				}

				BlockTableRecord newbtr = new BlockTableRecord();
				bt.UpgradeOpen();

				newbtr.Name = newname;
				ObjectId newBtrId = bt.Add(newbtr);
				tr.AddNewlyCreatedDBObject(newbtr, true);
				//----------------------------------------------------------------//


				IdMapping idMap = new IdMapping();
				db.DeepCloneObjects(ids, newBtrId, idMap, true);

				foreach (ObjectId id in newbtr)
				{
					Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
					if (ent == null) continue;
					ent.UpgradeOpen();
					ent.ColorIndex = 1;
				}

				//Change BlockReference
				bref.BlockTableRecord = newbtr.Id;

				tr.Commit();

			}
		}
		/*
		class Components
		{
		}

		public static BlockStruct SprinklerSymbol_Head_Circle = new BlockStruct(name: "SprinklerSymbol_Head_Circle");

		public struct BlockStruct
		{
			public string Name;
			public double Rotation;

			public BlockStruct(string name, double rotation = 0)
			{
				Name = name;
				Rotation = rotation;
			}
		}*/

	}
}
