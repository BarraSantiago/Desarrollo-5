using System;

namespace FSM
{
    public abstract class BaseState<T> : IState
    {
        protected StateSettings settings;
        
        public event Action onEnter;
        public event Action onExit;
        
        public T ID { get; private set; }

        protected BaseState(T id)
        {
            ID = id;
        }
        
        protected BaseState(T id, StateSettings settings) : this(id)
        {
            this.settings = settings;
        }

        public virtual void OnEnter()
        {
            onEnter?.Invoke();
        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnFixedUpdate()
        {

        }

        public virtual void OnExit()
        {
            onExit?.Invoke();
        }
    }
}

