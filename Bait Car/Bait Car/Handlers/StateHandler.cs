using System;

namespace Bait_Car.Handlers
{
    public class StateHandler
    {
        public event StateChangeEvent Event;
        public delegate void StateChangeEvent(StateHandler s, EventArgs e);

        private State _currentState;

        public State State
        {
            get => _currentState;
            set
            {
                _currentState = value;
                Event?.Invoke(this, new EventArgs());
            }
        }
    }
}
