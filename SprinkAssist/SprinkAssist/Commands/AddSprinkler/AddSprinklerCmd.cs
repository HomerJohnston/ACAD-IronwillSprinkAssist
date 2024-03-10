using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Autodesk.AutoCAD;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;

using Polyline = Autodesk.AutoCAD.DatabaseServices.Polyline;

using AcApplication = Autodesk.AutoCAD.ApplicationServices.Application;
using Autodesk.AutoCAD.GraphicsInterface;
using System.Collections.ObjectModel;
using Autodesk.AutoCAD.Colors;
using Ironwill.Commands.Help;

[assembly: CommandClass(typeof(Ironwill.Commands.AddSprinkler.AddSprinklerCmd))]

namespace Ironwill.Commands.AddSprinkler
{
	internal class PickSprinklerCommandState : State
	{
		public PickSprinklerCommandState(AddSprinklerCmd c, Transaction t) : base(c, t)
		{
			RegisterTransition<AddSprinklerCmd>(typeof(PickTileIntersectionCommandState), (command, transaction) =>
			{
				return (command.templateSprinklerId.IsValid && !command.templateSprinklerId.IsEffectivelyErased && command.templateSprinklerId.IsWellBehaved && command.templateSprinkler != null);
			});
		}

		protected override void Enter()
		{
			Session.Log("Entering PickSprinklerCommandState");
		}

		protected override void Run()
		{
			Session.Log("Running PickSprinklerCommandState");
			var cmd = GetCommand<AddSprinklerCmd>();

			cmd.templateSprinkler = BlockOps.PickSprinkler(transaction, "Pick a template sprinkler block");

			if (cmd.templateSprinkler == null)
			{
				ForceExit();
				return;
			}

			cmd.templateSprinklerId = cmd.templateSprinkler.ObjectId;
		}

		protected override void Exit()
		{
			Session.Log("Exiting PickSprinklerCommandState");
		}
	}

	internal class PickTileIntersectionCommandState : State
	{
		public PickTileIntersectionCommandState(AddSprinklerCmd c, Transaction t) : base(c, t)
		{
			//RegisterTransition<Command>(
		}

		protected override void Enter()
		{
			Session.Log("Entering PickTileIntersectionCommandState");
		}

		protected override void Run()
		{
			Session.Log("Running PickSprinklerCommandState");
		}

		protected override void Exit()
		{
			Session.Log("Exiting PickSprinklerCommandState");
		}
/*
		bool ValidateAnchor(TileAnchor anchor)
		{
			if (!EnsureValidAnchorPosition())
			{
				return false;
			}

			if (!EnsureValidAnchorTileLengths())
			{
				return false;
			}

			if (!EnsureValidAnchorRotation())
			{
				return false;
			}

			return true;
		}

		bool EnsureValidAnchorPosition()
		{
			if (anchor.anchorPosSet)
			{
				return true;
			}

			SelectAnchorPosition(anchor);

			return anchor.anchorPosSet;
		}

		bool EnsureValidAnchorRotation()
		{
			if (anchor.tileVectorSet)
			{
				return true;
			}

			SelectAnchorRotation(anchor);

			return anchor.tileVectorSet;
		}

		bool EnsureValidAnchorTileLengths()
		{
			if (anchor.tileLength1Set)
			{
				return true;
			}

			SelectAnchorTileVectorLengths(anchor);

			return anchor.tileLength1Set && anchor.tileLength2Set;
		}

		void SelectAnchorPosition(TileAnchor inAnchor)
		{
			AnchorPositionJigger anchorJigger = new AnchorPositionJigger(inAnchor);

			PromptResult result = Session.GetEditor().Drag(anchorJigger);

			if (result.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.anchorPos = anchorJigger.anchor.anchorPos;

			Session.Log("");
		}

		void SelectAnchorRotation(TileAnchor inAnchor)
		{
			AnchorRotationJigger anchorJigger = new AnchorRotationJigger(inAnchor);

			PromptResult result = Session.GetEditor().Drag(anchorJigger);

			if (result.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileVector = anchorJigger.anchor.tileVector;

			Session.Log("");
		}

		void SelectAnchorTileVectorLengths(TileAnchor inAnchor)
		{
			AnchorTileVectorJigger anchorJigger1 = new AnchorTileVectorJigger(inAnchor);

			PromptResult result1 = Session.GetEditor().Drag(anchorJigger1);

			if (result1.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileVector = anchorJigger1.anchor.tileVector;
			anchor.tileLength1 = anchorJigger1.anchor.tileLength1;
			Session.Log("");

			AnchorTileLength2Jigger anchorJigger2 = new AnchorTileLength2Jigger(inAnchor);

			PromptResult result2 = Session.GetEditor().Drag(anchorJigger2);

			if (result2.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileLength2 = anchorJigger2.anchor.tileLength2;
			Session.Log("");
		}*/
	}

	internal class AddSprinklerKeywordManager<T> : KeywordActionManager<T> where T : AddSprinklerCmd
	{
		public AddSprinklerKeywordManager(T inCommand) : base(inCommand)
		{
		}
	}

	// --------------------------------------------------------------------------------------------
	// Actual command
	internal class AddSprinklerCmd : SprinkAssistCommand
	{
		// Settings -----------------------------------

		// State --------------------------------------
		TileAnchor anchor = new TileAnchor();

		public ObjectId templateSprinklerId = ObjectId.Null;
		public BlockReference templateSprinkler = null;

		public void ResetTemplateSprinkler()
		{
			templateSprinkler = null;
			templateSprinklerId = ObjectId.Null;
		}

		AddSprinklerKeywordManager<AddSprinklerCmd> keywordActionManager;

		AddSprinklerJigger currentJigger = null;

		public AddSprinklerCmd()
		{
			keywordActionManager = new AddSprinklerKeywordManager<AddSprinklerCmd>(this);

			keywordActionManager.Register("Head", (transaction, T) =>
			{
				T.ResetTemplateSprinkler();
				T.ValidateTemplateSprinkler(transaction);
			});
		}

		[CommandDescription("Places sprinkler heads.", "Intended to be used for T-Bar ceilings.", "Select a template source sprinklers and place a 'Tile Anchor' down onto the grids of a T-Bar ceiling to use.", "Displays simple circles showing min/max light hazard standard spray spacing.")]
		[CommandMethod(SprinkAssist.CommandMethodPrefix, "AddSprinkler", CommandFlags.NoBlockEditor | CommandFlags.NoPaperSpace)]
		public void Main()
		{
			PromptResult promptResult = null;
			
			while (promptResult == null || promptResult.Status == PromptStatus.OK || promptResult.Status == PromptStatus.Keyword)
			{
				using (Transaction transaction = Session.StartTransaction())
				{
					if (!ValidateTemplateSprinkler(transaction))
					{
						Session.Log("Did not select a sprinkler to place");
						transaction.Commit();
						return;
					}

					if (!ValidateAnchor(anchor))
					{
						Session.Log("Did not specify valid tile");
						transaction.Commit();
						return;
					}

					currentJigger = new AddSprinklerJigger(transaction, templateSprinkler, anchor);

					promptResult = Session.GetEditor().Drag(currentJigger);

					switch (promptResult.Status)
					{
						case PromptStatus.Keyword:
						{
							keywordActionManager.Consume(transaction, promptResult.StringResult);

							currentJigger.RemoveSprinkler();
							break;
						}
						case PromptStatus.OK:
						{
							break;
						}
						default:
						{
							currentJigger.RemoveSprinkler();
							break;
						}
					}

					transaction.Commit();
					currentJigger = null;
				}
			}
		}

		bool ValidateTemplateSprinkler(Transaction transaction)
		{
			if (templateSprinklerId.IsNull)
			{
				SelectTemplateSprinkler(transaction);
			}
			else if (templateSprinklerId.IsErased || templateSprinklerId.IsEffectivelyErased)
			{
				Session.Log("Template sprinkler was erased - pick new template sprinkler");
				SelectTemplateSprinkler(transaction);
			}
			else if (templateSprinklerId.IsValid && templateSprinklerId.IsWellBehaved)
			{
				templateSprinkler = transaction.GetObject(templateSprinklerId, OpenMode.ForRead) as BlockReference;
				return true;
			}

			return templateSprinkler != null;
		}

		void SelectTemplateSprinkler(Transaction transaction)
		{
			templateSprinkler = BlockOps.PickSprinkler(transaction, "Pick a template sprinkler block");

			if (templateSprinkler == null)
			{
				return;
			}

			templateSprinklerId = templateSprinkler.ObjectId;
		}

		bool ValidateAnchor(TileAnchor anchor)
		{
			if (!EnsureValidAnchorPosition())
			{
				return false;
			}

			if (!EnsureValidAnchorTileLengths())
			{
				return false;
			}

			if (!EnsureValidAnchorRotation())
			{
				return false;
			}

			return true;
		}

		bool EnsureValidAnchorPosition()
		{
			if (anchor.anchorPosSet)
			{
				return true;
			}

			SelectAnchorPosition(anchor);

			return anchor.anchorPosSet;
		}

		bool EnsureValidAnchorRotation()
		{
			if (anchor.tileVectorSet)
			{
				return true;
			}

			SelectAnchorRotation(anchor);

			return anchor.tileVectorSet;
		}

		bool EnsureValidAnchorTileLengths()
		{
			if (anchor.tileLength1Set)
			{
				return true;
			}

			SelectAnchorTileVectorLengths(anchor);

			return anchor.tileLength1Set && anchor.tileLength2Set;
		}

		void SelectAnchorPosition(TileAnchor inAnchor)
		{
			AnchorPositionJigger anchorJigger = new AnchorPositionJigger(inAnchor);

			PromptResult result = Session.GetEditor().Drag(anchorJigger);

			if (result.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.anchorPos = anchorJigger.anchor.anchorPos;

			Session.Log("");
		}

		void SelectAnchorRotation(TileAnchor inAnchor)
		{
			AnchorRotationJigger anchorJigger = new AnchorRotationJigger(inAnchor);

			PromptResult result = Session.GetEditor().Drag(anchorJigger);

			if (result.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileVector = anchorJigger.anchor.tileVector;

			Session.Log("");
		}

		void SelectAnchorTileVectorLengths(TileAnchor inAnchor)
		{
			AnchorTileVectorJigger anchorJigger1 = new AnchorTileVectorJigger(inAnchor);

			PromptResult result1 = Session.GetEditor().Drag(anchorJigger1);

			if (result1.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileVector = anchorJigger1.anchor.tileVector;
			anchor.tileLength1 = anchorJigger1.anchor.tileLength1;
			Session.Log("");

			AnchorTileLength2Jigger anchorJigger2 = new AnchorTileLength2Jigger(inAnchor);

			PromptResult result2 = Session.GetEditor().Drag(anchorJigger2);

			if (result2.Status != PromptStatus.OK)
			{
				return;
			}

			anchor.tileLength2 = anchorJigger2.anchor.tileLength2;
			Session.Log("");
		}
	}


}