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
	internal class SprinkAssistCommandSettings
	{
		readonly DBDictionary dic;

		public SprinkAssistCommandSettings(Transaction creationTransaction, DBDictionary dictionary)
        {
            dic = dictionary;
        }

		public CommandSetting<T> RegisterNew<T>(string settingName, T defaultValue)
		{
			CommandSetting<T> newSetting = new CommandSetting<T>(settingName, defaultValue, dic);
			return newSetting;
		}
	}

    // ----------------------------
    internal class SprinkAssistCommand
	{
		protected SprinkAssistCommandSettings settings;

		protected StateMachine stateMachine = new StateMachine();

		public SprinkAssistCommand()
		{
			// This ensures that the command settings are created and/or updated
			using (Transaction transaction = Session.StartTransaction())
			{
				DBDictionary cmdSettingsDictionary = XRecordLibrary.GetCommandDictionary(transaction, GetType());
                
                settings = new SprinkAssistCommandSettings(transaction, cmdSettingsDictionary);

				transaction.Commit();
			}
		}
	}

	// ----------------------------
	internal class KeywordActionHandler<T> where T : class
	{
		// State --------------------------------------
		T owner;
		
		public Dictionary<string, Action<Transaction, T>> actions = new Dictionary<string, Action<Transaction, T>>();

		public KeywordActionHandler(T inOwner)
		{
			owner = inOwner;
		}

		// API ----------------------------------------
		public bool Consume(Transaction transaction, string keyword)
		{
			if (!actions.ContainsKey(keyword))
			{
				Session.Log("ERROR: Keyword not handled");
				return false;
			}

			actions[keyword].Invoke(transaction, owner);
			return true;
		}

		public void RegisterKeyword(string keyword, Action<Transaction, T> actionToRunOnKeyword)
		{
			actions[keyword] = actionToRunOnKeyword;
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