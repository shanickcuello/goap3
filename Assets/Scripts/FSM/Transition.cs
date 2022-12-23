using System;
namespace IA2
{
    public class Transition<T>
    {
        public event Action<T> OnTransition = delegate { };
        public T Input => input;
        public State<T> TargetState => targetState;
        private T input;
        private State<T> targetState;
        public void OnTransitionExecute(T input)
        {
            OnTransition(input);
        }
        public Transition(T input, State<T> targetState)
        {
            this.input = input;
            this.targetState = targetState;
        }
    }
}