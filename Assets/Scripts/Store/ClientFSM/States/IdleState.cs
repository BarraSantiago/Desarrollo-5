namespace Store.ClientFSM.States
{
    public class IdleState : ClientBaseState
    {
        public IdleState(Client client) : base(client)
        {
            
        }
        protected override void Enter(Client client)
        {
            client.agent.SetDestination(Client.ClientTransforms.Entrance.position);
        }

        protected override void Update(Client client)
        {
            if(CheckCondition(client)) Exit(client);
        }
        
        public override bool CheckCondition(Client client)
        {
            return client.NearEntrance();
        }
    }
}