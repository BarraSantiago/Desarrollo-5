namespace Store.ClientFSM.States
{
    public abstract class ClientBaseState
    {
        protected ClientBaseState(Client client)
        {
            Enter(client);
        }

        protected ClientBaseState()
        {
        }

        protected virtual void Enter(Client client) { }

        protected virtual void Update(Client client)
        {
        }

        protected virtual void Exit(Client client)
        {
            client.ClientStateManager.NextState(this, client);
        }
        public abstract bool CheckCondition(Client client);
    }
}