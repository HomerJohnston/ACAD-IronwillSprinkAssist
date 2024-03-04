using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ironwill.Commands
{
	internal struct Transition
	{
		public readonly System.Type targetState;
		public readonly Func<SprinkAssistCommand, Transaction, bool> condition;

		public Transition(System.Type inTargetState, Func<SprinkAssistCommand, Transaction, bool> inCondition)
		{
			targetState = inTargetState;
			condition = inCondition;
		}
	}

	internal abstract class State
	{
		List<Transition> transitions = new List<Transition>();
		
		public SprinkAssistCommand command;
		public Transaction transaction;

		private bool forceExit = false;

		public State(SprinkAssistCommand inCommand, Transaction inTransaction)
		{
			command = inCommand;
			transaction = inTransaction;
		}

		// 
		public void RegisterTransition<T>(System.Type targetState, Func<T, Transaction, bool> condition) where T : SprinkAssistCommand
		{
			Transition transition = new Transition(targetState, (Func<SprinkAssistCommand, Transaction, bool>)condition);

			transitions.Add(transition);
		}

		public State ActivateStateLoop()
		{
			Enter();

			while (!forceExit)
			{
				foreach (Transition transition in transitions)
				{
					if (transition.condition.Invoke(command, transaction))
					{
						Exit();
						return (State)Activator.CreateInstance(transition.targetState, command, transaction);
					}
				}

				Run();
			}

			Exit();

			return null;
		}

		protected void ForceExit()
		{
			forceExit = true;
		}

		protected virtual void Enter() { }

		protected virtual void Run() { }

		protected virtual void Exit() { }

		protected T GetCommand<T>() where T : SprinkAssistCommand
		{
			return (T)command;
		}
	}

	internal class StateMachine
	{
		State currentState;

		public StateMachine()
		{
		}

		public void Run(State startState)
		{
			currentState = startState;

			while (currentState != null)
			{
				currentState = currentState.ActivateStateLoop();
			}
		}
	}

	// ----------------------------
	internal class SprinkAssistCommand
	{
		protected DBDictionary cmdSettings;

		protected StateMachine stateMachine = new StateMachine();

		public SprinkAssistCommand()
		{
			// This ensures that the command settings are created and/or updated
			using (Transaction transaction = Session.StartTransaction())
			{
				cmdSettings = XRecordLibrary.GetCommandDictionaryForClass(transaction, GetType());
				transaction.Commit();
			}
		}
	}

	// ----------------------------
	internal class KeywordActionManager<T> where T : class
	{
		// State --------------------------------------
		T owner;
		
		public Dictionary<string, Action<Transaction, T>> actions = new Dictionary<string, Action<Transaction, T>>();

		public KeywordActionManager(T inOwner)
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

		public List<string> GetKeywords
		{
			get { return actions.Keys.ToList(); }
		}

		public void Register(string keyword, Action<Transaction, T> x)
		{
			actions[keyword] = x;
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