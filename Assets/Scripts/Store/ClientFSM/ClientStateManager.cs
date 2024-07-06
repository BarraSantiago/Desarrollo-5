using Store.ClientFSM.States;

namespace Store.ClientFSM
{
    public class ClientStateManager
    {
        public void NextState(ClientBaseState state, Client client)
        {
            client._CurrentState = state switch
            {
                NoneState => new IdleState(client),
                IdleState => new ChoosingState(client),
                ChoosingState => new GrabbingState(client),
                GrabbingState => new WaitingLineState(client),
                WaitingLineState => new BuyingState(client),
                BuyingState => new LeavingState(client),
                LeavingState => new NoneState(client),
                _ => client._CurrentState
            };
        }

        public void PreviousState(ClientBaseState state, Client client)
        {
            client._CurrentState = state switch
            {
                NoneState => new LeavingState(client),
                IdleState => new NoneState(client),
                ChoosingState => new IdleState(client),
                GrabbingState => new ChoosingState(client),
                WaitingLineState => new GrabbingState(client),
                BuyingState => new WaitingLineState(client),
                LeavingState => new BuyingState(client),
                _ => client._CurrentState
            };
        }

        public void SetState(ClientBaseState state, Client client)
        {
            client._CurrentState = state;
        }
    }
}