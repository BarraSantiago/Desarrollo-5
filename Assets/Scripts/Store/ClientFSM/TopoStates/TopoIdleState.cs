using FSM;

namespace Store.ClientFSM.TopoStates
{
    public class TopoIdleState<T> : TopoClientBaseState<T>
    {
        public TopoIdleState(T id) : base(id)
        {
        }

        public TopoIdleState(T id, StateSettings settings) : base(id, settings)
        {
        }

        public override void OnEnter(Client client)
        {
            base.OnEnter(client);
            client.agent.SetDestination(Client.ClientTransforms.Entrance.position);
        }
    }
}