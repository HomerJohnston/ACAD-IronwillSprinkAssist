using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands
{
	// ----------------------------
	internal class SprinkAssistCommand
	{
		protected DBDictionary cmdSettings;

		public SprinkAssistCommand()
		{
			using (Transaction transaction = Session.StartTransaction())
			{
				cmdSettings = XRecordLibrary.GetCommandDictionaryForClass(transaction, GetType());
				transaction.Commit();
			}
		}
	}

	// ----------------------------
	internal class KeywordActionManager
	{
		// State --------------------------------------
		SprinkAssistCommand command;
		
		public Dictionary<string, KeywordAction> actions = new Dictionary<string, KeywordAction>();

		// Constructor --------------------------------
		public KeywordActionManager(SprinkAssistCommand inCommand)
		{
			command = inCommand;
		}

		// API ----------------------------------------
		public bool Consume(PromptResult prompt)
		{
			string Keyword = prompt.StringResult;

			if (!actions.ContainsKey(Keyword))
			{
				Session.Log("ERROR: Keyword not handled");
				return false;
			}

			actions[Keyword].Execute();
			return true;
		}

		public void Register<T>(string keyword) where T : KeywordAction, new()
		{
			T newAction = new T();
			newAction.SetOwner(command);
			actions[keyword] = newAction;
		}

		public List<string> GetKeywords
		{
			get { return actions.Keys.ToList(); }
		}

		public void SetKeywordsForPrompt(PromptOptions promptOptions)
		{
			promptOptions.Keywords.Clear();

			foreach (string keyword in actions.Keys)
			{
				promptOptions.Keywords.Add(keyword);
			}
		}
	}

	// ----------------------------
	abstract class KeywordAction
	{
		// State --------------------------------------
		protected object owner;

		public void SetOwner(object inOwner)
		{
			owner = inOwner;
		}

		public abstract void Execute();
	}
}