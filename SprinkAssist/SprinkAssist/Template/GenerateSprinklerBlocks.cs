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
		public void GenTest()
		{
			Run("NormalHeads.csv", "S_Head_TEMPLATE", 0.0);
			Run("SidewallHeads.csv", "S_Sidewall_TEMPLATE", 50000.0);
			Run("AtticHeads.csv", "S_Attic_TEMPLATE", 100000.0);
		}

		public void Run(string fileName, string templateBlock, double blockSpawnOffset)
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			UriBuilder uri = new UriBuilder(codeBase);
			string dllFile = Uri.UnescapeDataString(uri.Path);

			string resourcesFolder = Path.Combine(Path.GetDirectoryName(dllFile), "..", "Resources");

			using (Transaction transaction = Session.StartTransaction())
			{
				List<SprinklerDefinition> sprinklerDefinitions = new List<SprinklerDefinition>();

				Database database = Session.GetDatabase();

				List<List<string>> readComponents = new List<List<string>>();

				int sprinklerColumns = 1;
				int totalSprinklers = 1;

				using (GenericParser parser = new GenericParser())
				{
					string Test = Path.Combine(resourcesFolder, fileName);
					parser.SetDataSource(Test);
					parser.ColumnDelimiter = ',';
					parser.SkipStartingDataRows = 1;

					while (parser.Read())
					{
						int columnCount = parser.ColumnCount;

						for (int i = 0; i < columnCount; i++)
						{
							while (readComponents.Count <= i)
							{
								readComponents.Add(new List<string>());
							}

							List<string> column = readComponents[i];

							string data = parser[i];

							if (data != String.Empty)
							{
								Session.Log("Added " + data + " to column " + i);
								column.Add(data);
							}
						}
					}

					for (int i = 0; i < readComponents.Count; i++)
					{
						if (i == 0)
						{
							sprinklerColumns = readComponents[i].Count;
						}

						totalSprinklers *= readComponents[i].Count;
					}

					Stack<string> strings = new Stack<string>();

					RecurseTest(ref readComponents, 0, strings, ref sprinklerDefinitions);
				}

				List<string> replacedBlocks = new List<string>();

				Session.Log("======================= Starting replacement ===========================");

				foreach (SprinklerDefinition sprinklerDefinition in sprinklerDefinitions)
				{
					string templateSprinklerBlock = templateBlock;

					ObjectId newBlockId = RedefineBlock(transaction, templateSprinklerBlock, sprinklerDefinition.baseBlockName);

					if (!newBlockId.IsValid)
					{
						Session.Log("Error while replacing block!");
						continue;
					}

					replacedBlocks.Add(sprinklerDefinition.baseBlockName);

					BlockTableRecord btr = transaction.GetObject(newBlockId, OpenMode.ForWrite, false, true) as BlockTableRecord;
					
					Session.Log("START");

					

					BlockReference br = transaction.GetObject(newBlockId, OpenMode.ForRead, false, true) as BlockReference;
					
					if (br != null)
					{
						br.RecordGraphicsModified(true);
						DynamicBlockReferencePropertyCollection test = br.DynamicBlockReferencePropertyCollection;
					}

					foreach (ObjectId entityId in btr)
					{
						Entity entity = transaction.GetObject(entityId, OpenMode.ForWrite, false, true) as Entity;

						Session.Log(entity.ToString() + " -- " + entity.Handle.ToString() + " - " + entityId.ToString());

						if (entity == null)
						{
							continue;
						}

						BlockReference blockReference = entity as BlockReference;

						if (blockReference != null)
						{
							if (!sprinklerDefinition.componentNames.Contains(blockReference.Name))
							{
								Session.Log("Erasing entity " + blockReference.Name + " from block " + sprinklerDefinition.baseBlockName);
								DBObject obj = entityId.GetObject(OpenMode.ForWrite);
								obj.Erase();
							}
						}
					}

					foreach (ObjectId entityId in btr)
					{
						Entity entity = transaction.GetObject(entityId, OpenMode.ForWrite, false, true) as Entity;

						Session.Log(entity.ToString() + " -- " + entity.Handle.ToString() + " - " + entityId.ToString());

						
					}

					Session.Log("END");
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
				double posX = blockSpawnOffset;
				int count = 0;
				foreach (SprinklerDefinition sprinklerDefinition in sprinklerDefinitions)
				{
					if (count % (totalSprinklers / sprinklerColumns) == 0)
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

		public void RecurseTest(ref List<List<string>> data, int currentColumn, Stack<string> namePieces, ref List<SprinklerDefinition> sprinklerDefinitions)
		{
			//Session.Log("----- Starting " + currentColumn);

			List<string> currentColumnData = data[currentColumn];

			int columnCount = data.Count;

			for (int row = 0; row < currentColumnData.Count; row++)
			{
				string rowString = currentColumnData[row];

				const string nil = "NONE";

				bool use = rowString != nil;

				if (use)
				{
					//Session.Log("Pushing: " + rowString + " [" + currentColumn + ", " + row + "]");
					
					namePieces.Push(rowString);

					string debug = string.Join(", ", namePieces.Reverse().ToArray());
					Session.Log(debug);
				}

				//for (int column = currentColumn + 1; column < columnCount; column++)
				if (currentColumn < columnCount - 1)
				{
					RecurseTest(ref data, currentColumn + 1, namePieces, ref sprinklerDefinitions);
				}
							
				if (currentColumn == columnCount - 1)
				{
					List<string> components = namePieces.Reverse().ToList();

					for (int i = 0; i < components.Count; i++)
					{
						string s = components[i];

						int last_ = s.LastIndexOf('_');

						if (last_ >= 0)
						{
							components[i] = s.Substring(last_ + 1);
						}

						if (components[i] == string.Empty)
						{
							components.RemoveAt(i--);
						}
					}

					components.Insert(0, "S");

					string blockName = string.Join("_", components);

					Session.Log("Finished [" + currentColumn + ", " + row + "]: " + blockName);

					SprinklerDefinition sprinklerDefinition = new SprinklerDefinition(blockName, namePieces.ToArray());
					sprinklerDefinitions.Add(sprinklerDefinition);
				}

				if (use)
				{
					namePieces.Pop();

					string debug = string.Join(", ", namePieces.Reverse().ToArray());
					Session.Log(debug);
				}
			}
		}

		public void RecurseColumn(ref List<List<string>> rawData, ref int currentColumn, int currentIndex, Stack<string> outComponents, string currentString)
		{
			if (currentColumn >= rawData.Count)
			{
				return;
			}

			string tempName = currentString;

			List<string> columnData = rawData[currentColumn];

			foreach (string row in columnData)
			{
				
				//outComponents.Push(part);
			
				//tempName = tempName + part.Substring(part.LastIndexOf('_'));
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

		[CommandMethod("FixTest")]
		public async void testResetBlock()
		{
			//using (Transaction transaction = Session.StartTransaction())
			//{
				/*BlockReference blockReference = BlockOps.PickSprinkler(transaction, "pick a sprinkler");

				if (blockReference == null)
				{
					return;
				}

				List<ObjectId> selected = new List<ObjectId>();

				selected.Add(blockReference.ObjectId);
				*/

				/*
				var psr = Session.GetEditor().GetSelection();

				if (psr.Status != PromptStatus.OK)
				{
					return;
				}
				*/

				Session.Log("Start");
				await Session.GetDocumentManager().ExecuteInCommandContextAsync(
					async (obj) =>
					{
						await Session.GetEditor().CommandAsync(new object[]
							{
								".-bedit", "S_Head_TEMPLATE"
							}
						);
					},
					null
				);
				Session.Log("End");
				//transaction.Commit();
				//Session.GetEditor().SetImpliedSelection(selected.ToArray());
				
				//Session.Command("-bedit");
				


				//Session.Command("-bclose");
			//}
		}
	}
}
