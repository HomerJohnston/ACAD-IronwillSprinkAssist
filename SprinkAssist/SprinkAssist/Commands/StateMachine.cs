using Autodesk.AutoCAD.DatabaseServices;
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
}
