﻿using Autodesk.AutoCAD.Geometry;
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


				using (GenericParser parser = new GenericParser())
				{
					string Test = Path.Combine(resourcesFolder, "Test.csv");
					parser.SetDataSource(Test);
					parser.ColumnDelimiter = ',';
					parser.SkipStartingDataRows = 1;

					List<string> headBases = new List<string>();
					List<string> headLabels = new List<string>();
					List<string> headTemps = new List<string>();
					List<string> headDecorators = new List<string>();

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
						string baseHeadName = headBase.Replace("SprinklerBase", "S"); // SprinklerBase_Head_NN --> S_Head_NN
						Stack<string> components = new Stack<string>();

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

									Session.LogDebug("Generating definition: " + decoratedTempdLabelledHeadName + " --- " + string.Join(", ", components.ToArray()));

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

				foreach (SprinklerDefinition sprinklerDefinition in sprinklerDefinitions)
				{
					const string templateSprinklerBlock = "S_Head_TEMPLATE";

					// Clone the master template block.
					Session.LogDebug("Building: " + sprinklerDefinition.baseBlockName);

					// Edit the block.
					ObjectId newBlockId = FuckFaceReplace(templateSprinklerBlock, sprinklerDefinition.baseBlockName);

					/*
					BlockTableRecord btr = transaction.GetObject(newBlockId, OpenMode.ForWrite, false, true) as BlockTableRecord;

					foreach (ObjectId entityId in btr)
					{
						BlockReference ent = transaction.GetObject(entityId, OpenMode.ForWrite, false, true) as BlockReference;

						if (ent != null)
						{
							if (!sprinklerDefinition.componentNames.Contains(ent.Name))
							{
								DBObject obj = entityId.GetObject(OpenMode.ForWrite);

								//obj.Erase();
							}
						}
					}
					*/
				}


				// Delete shit in the block I don't want to be in it.
			}
		}

		public ObjectId FuckFaceReplace(string sourceBlock, string destinationBlock)
		{
			Database database = Session.GetDatabase();

			using (Transaction transaction = Session.StartTransaction())
			{
				BlockTable blockTable = transaction.GetObject(database.BlockTableId, OpenMode.ForRead) as BlockTable;

				BlockTableRecord sourceBlockBTR = transaction.GetObject(blockTable[sourceBlock], OpenMode.ForRead) as BlockTableRecord;

				if (sourceBlockBTR == null)
				{
					Session.Log("Error! Source block " + sourceBlock + " was not found.");
					return ObjectId.Null;
				}

				using (Database cloneDatabase = database.Wblock(sourceBlockBTR.ObjectId))
				{
					ObjectId copyId = database.Insert(destinationBlock, cloneDatabase, true);

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
							BlockOps.RecreateBlock(transaction, destinationBlock, objectId);
						}

						transaction.Commit();
						return copyId;
					}
				}

				Session.Log("Error! Failed to replace block");
				return ObjectId.Null;
			}
		}


	}
}
