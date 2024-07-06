using FSM;

namespace Store.ClientFSM.TopoStates
{
    public abstract class TopoClientBaseState<T> : BaseState<T>
    {
        public TopoClientBaseState(T id) : base(id)
        {
        }

        public TopoClientBaseState(T id, StateSettings settings) : base(id, settings)
        {
        }

        public virtual void OnEnter(Client client)
        {
            base.OnEnter();
        }
        
        public virtual void OnUpdate(Client client)
        {
            base.OnUpdate();
        }

        public virtual void OnExit(Client client)
        {
            base.OnExit();
        }
        
    }
}