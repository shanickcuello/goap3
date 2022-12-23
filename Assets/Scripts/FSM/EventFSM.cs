using System;
using UnityEngine;
namespace IA2
{
    public class EventFSM<T>
    {
        private State<T> current;
        public State<T> any;
        public EventFSM(State<T> initial, State<T> any = null)
        {
            current = initial;
            current.Enter(default);
            this.any = any != null ? any : new State<T>("<any>");
            this.any.OnEnter += a => { throw new Exception("Can't make transition to fsm's <any> state"); };
        }
        public bool Feed(T input)
        {
            State<T> newState;
            if (current.Feed(input, out newState) || any.Feed(input, out newState))
            {
                current.Exit(input);
                Debug.Log("FSM state: " + current.Name + "---" + input + "---> " + newState.Name);
                current = newState;
                current.Enter(input);
                return true;
            }
            return false;
        }
        public State<T> Current => current;
        public void Update()
        {
            current.Update();
        }
    }
}