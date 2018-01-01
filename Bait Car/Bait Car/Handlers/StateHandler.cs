using System;

namespace Bait_Car.Handlers
{
    public class StateHandler
    {
        public event StateChangeEvent Event;
        public delegate void StateChangeEvent(StateHandler s, State previousState, State newState);

        private State _currentState;

        public State State
        {
            get => _currentState;
            set
            {
                Event?.Invoke(this, _currentState, value);
                _currentState = value;
            }
        }
    }
}
