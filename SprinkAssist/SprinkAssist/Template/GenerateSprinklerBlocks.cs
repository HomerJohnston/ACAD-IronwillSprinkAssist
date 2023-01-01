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
	internal class SprinklerDefinition
	{
		public string baseBlockName = string.Empty;
		public List<string> componentNames = new List<string>();

		public SprinklerDefinition(string inBaseBlockName, params string[] inComponentNames)
		{
			baseBlockName = inBaseBlockName;

			foreach (string component in inComponentNames)
			{
				if (component != string.Empty)
				{
					componentNames.Add(component);
				}
			}
		}
	}

	internal class GenerateSprinklerBlocks : SprinkAssistCommand
	{
		[CommandMethod("GenTest")]
		public void RuntimeArgumentHandle()
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string dllFile = Uri.UnescapeDataString(uri.Path);

			string resourcesFolder = Path.Combine(Path.GetDirectoryName(dllFile), "..", "Resources");

			using (Transaction transaction = Session.StartTransaction())
			{
				List<SprinklerDefinition> sprinklerDefinitions = new List<SprinklerDefinition>();

				Database database = Session.GetDatabase();

				List<string> headBases = new List<string>();
				List<string> headLabels = new List<string>();
				List<string> headTemps = new List<string>();
				List<string> headDecorators = new List<string>();

				using (GenericParser parser = new GenericParser())
				{
					string Test = Path.Combine(resourcesFolder, "Test.csv");
					parser.SetDataSource(Test);
					parser.ColumnDelimiter = ',';
					parser.SkipStartingDataRows = 1;
					
					while (parser.Read())
					{
						if (parser[0] != string.Empty)
							headBases.Add(parser[0]);

						if (parser[1] != string.Empty)
							headLabels.Add(parser[1]);

						if (parser[2] != string.Empty)
							headTemps.Add(parser[2]);

						if (parser[3] != string.Empty)
							headDecorators.Add(parser[3]);
					}

					// Build up every single head combination. I have to generate every head name and the combination of components that compose it.

					const string nil = "NONE";

					// SprinklerBase_Head_NN
					foreach (string headBase in headBases)
					{
						string baseName = "S";
						Stack<string> components = new Stack<string>();

						string baseHeadName = baseName;

						if (headBase != nil)
						{
							baseHeadName += headBase.Substring(headBase.LastIndexOf('_'));
							components.Push(headBase);
						}

						foreach (string headLabel in headLabels)
						{
							string labeledHeadName = baseHeadName;

							if (headLabel != nil)
							{
								labeledHeadName += headLabel.Substring(headLabel.LastIndexOf('_'));
								components.Push(headLabel);
							}

							foreach (string tempLabel in headTemps)
							{
								string tempLabelledHeadName = labeledHeadName;

								if (tempLabel != nil)
								{
									tempLabelledHeadName += tempLabel.Substring(tempLabel.LastIndexOf('_'));
									components.Push(tempLabel);
								}

								foreach (string decorator in headDecorators)
								{
									string decoratedTempdLabelledHeadName = tempLabelledHeadName;

									if (decorator != nil)
									{
										decoratedTempdLabelledHeadName += decorator.Substring(decorator.LastIndexOf('_'));
										components.Push(decorator);
									}


									List<string> nameArray = new List<string>(components);
									nameArray.Prepend("S");

									string finalHeadName = string.Join("_", components.ToArray());

									Session.LogDebug("Generating definition: " + finalHeadName + " --- " + string.Join(", ", components.ToArray()));
									sprinklerDefinitions.Add(new SprinklerDefinition(decoratedTempdLabelledHeadName, components.ToArray()));


									if (decorator != nil)
									{
										components.Pop();
									}
								}

								if (tempLabel != nil)
								{
									components.Pop();
								}
							}

							if (headLabel != nil)
							{
								components.Pop();
							}
						}
					}
				}

				List<string> replacedBlocks = new List<string>();

				Session.Log("======================= Starting replacement ===========================");

				foreach (SprinklerDefinition sprinklerDefinition in sprinklerDefinitions)
				{
					const string templateSprinklerBlock = "S_Head_TEMPLATE";

					ObjectId newBlockId = RedefineBlock(transaction, templateSprinklerBlock, sprinklerDefinition.baseBlockName);

					if (!newBlockId.IsValid)
					{
						Session.Log("Error while replacing block!");
						continue;
					}

					replacedBlocks.Add(sprinklerDefinition.baseBlockName);

					BlockTableRecord btr = transaction.GetObject(newBlockId, OpenMode.ForWrite, false, true) as BlockTableRecord;

					foreach (ObjectId entityId in btr)
					{
						BlockReference ent = transaction.GetObject(entityId, OpenMode.ForWrite, false, true) as BlockReference;

						if (ent != null)
						{
							if (!sprinklerDefinition.componentNames.Contains(ent.Name))
							{
								Session.Log("Erasing entity " + ent.Name + " from block " + sprinklerDefinition.baseBlockName);
								DBObject obj = entityId.GetObject(OpenMode.ForWrite);

								obj.Erase();
							}
						}
					}
				}

				Session.Log("======================= Starting foundDeadBlocks ===========================");

				List<ObjectId> foundDeadBlocks = new List<ObjectId>();

				foreach (string replacedBlock in replacedBlocks)
				{
					BlockTable dwgBlockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForWrite, false, true) as BlockTable;

					BlockTableRecord modelSpace = transaction.GetObject(dwgBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false, true) as BlockTableRecord;

					foreach (ObjectId modelSpaceObjectId in modelSpace)
					{
						if (modelSpaceObjectId.ObjectClass.DxfName == "INSERT")
						{
							BlockReference blockReference = transaction.GetObject(modelSpaceObjectId, OpenMode.ForWrite, false, true) as BlockReference;

							if (blockReference != null && BlockOps.GetDynamicBlockName(blockReference) == replacedBlock)
							{
								foundDeadBlocks.Add(modelSpaceObjectId);
							}
						}
					}
				}

				Session.Log("======================= Starting replacement of dead blocks ===========================");

				foreach (ObjectId objectId in foundDeadBlocks)
				{
					BlockReference blockReference = transaction.GetObject(objectId, OpenMode.ForWrite, false, true) as BlockReference;
					string name = BlockOps.GetDynamicBlockName(blockReference);
					BlockOps.RecreateBlock(transaction, name, objectId);
				}

				Session.Log("======================= Spawning new blocks array ================================");

				double posY = 0;
				double posX = 0;
				int count = 0;
				foreach (SprinklerDefinition sprinklerDefinition in sprinklerDefinitions)
				{
					if (count % 24 == 0)
					{
						posY = 0;
						posX += 1000;
					}

					BlockReference newBlock = BlockOps.InsertBlock(sprinklerDefinition.baseBlockName);

					newBlock.Position = new Point3d(posX, posY, 0);
					newBlock.ScaleFactors = new Scale3d(100, 100, 100);
					
					posY -= 1000;
					count++;
				}

				transaction.Commit();
			}
		}

		public ObjectId RedefineBlock(Transaction transaction, string sourceBlock, string destinationBlock)
		{
			Database database = Session.GetDatabase();

			BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

			BlockTableRecord sourceBlockBTR = transaction.GetObject(blockTable[sourceBlock], OpenMode.ForRead) as BlockTableRecord;

			if (sourceBlockBTR == null)
			{
				Session.Log("Error! Source block " + sourceBlock + " was not found.");
				return ObjectId.Null;
			}

			ObjectId copyId = ObjectId.Null;

			using (Database cloneDatabase = database.Wblock(sourceBlockBTR.ObjectId))
			{
				copyId = database.Insert(destinationBlock, cloneDatabase, true);

				if (copyId.IsValid)
				{
					BlockTable dwgBlockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForWrite, false, true) as BlockTable;

					BlockTableRecord modelSpace = transaction.GetObject(dwgBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false, true) as BlockTableRecord;

					List<ObjectId> foundDeadBlocks = new List<ObjectId>();

					foreach (ObjectId modelSpaceObjectId in modelSpace)
					{
						if (modelSpaceObjectId.ObjectClass.DxfName == "INSERT")
						{
							BlockReference blockReference = transaction.GetObject(modelSpaceObjectId, OpenMode.ForWrite, false, true) as BlockReference;

							if (blockReference != null && BlockOps.GetDynamicBlockName(blockReference) == destinationBlock)
							{
								foundDeadBlocks.Add(modelSpaceObjectId);
							}
						}
					}

					foreach (ObjectId objectId in foundDeadBlocks)
					{
						BlockOps.RecreateBlock(transaction, destinationBlock, objectId, false);
					}
				}
			}

			if (copyId == null)
			{
				Session.Log("Error! Failed to replace block");
			}

			return copyId;
		}

		[CommandMethod("CopyXC")]
		public void testCopyBlockReference()
		{
			Database database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

				BlockTableRecord sourceBlockBTR = transaction.GetObject(blockTable["S_Head_TEMPLATE"], OpenMode.ForRead) as BlockTableRecord;

				using (Database cloneDatabase = database.Wblock(sourceBlockBTR.ObjectId))
				{
					ObjectId copyId = database.Insert("wtf", cloneDatabase, true);

					if (copyId.IsValid)
					{
						BlockTable dwgBlockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForWrite, false, true) as BlockTable;

						BlockTableRecord modelSpace = transaction.GetObject(dwgBlockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite, false, true) as BlockTableRecord;

						List<ObjectId> foundDeadBlocks = new List<ObjectId>();

						foreach (ObjectId modelSpaceObjectId in modelSpace)
						{
							if (modelSpaceObjectId.ObjectClass.DxfName == "INSERT")
							{
								BlockReference blockReference = transaction.GetObject(modelSpaceObjectId, OpenMode.ForWrite, false, true) as BlockReference;

								if (blockReference != null && BlockOps.GetDynamicBlockName(blockReference) == "wtf")
								{
									Session.Log(BlockOps.GetDynamicBlockName(blockReference) + ", " + blockReference.Name + ", " + blockReference.BlockName);

									foundDeadBlocks.Add(modelSpaceObjectId);
								}
							}
						}

						foreach (ObjectId objectId in foundDeadBlocks)
						{
							BlockOps.RecreateBlock(transaction, "wtf", objectId);
						}
					}
				}

				Session.Log("Committing transaction");
				transaction.Commit();
			}
		}
	}
}
