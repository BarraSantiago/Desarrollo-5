namespace Store.ClientFSM.States
{
    public class LeavingState : ClientBaseState
    {
        public LeavingState(Client client) : base(client)
        {
        }

        protected override void Enter(Client client)
        {
            throw new System.NotImplementedException();
        }

        protected override void Update(Client client)
        {
            throw new System.NotImplementedException();
        }

        public override bool CheckCondition(Client client)
        {
            throw new System.NotImplementedException();
        }
    }
}