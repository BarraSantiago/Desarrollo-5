namespace Store.ClientFSM.States
{
    public class WaitingLineState : ClientBaseState
    {
        public WaitingLineState(Client client) : base(client)
        {
        }

        public override bool CheckCondition(Client client)
        {
            throw new System.NotImplementedException();
        }

        protected override void Enter(Client client)
        {
            throw new System.NotImplementedException();
        }

        protected override void Update(Client client)
        {
            throw new System.NotImplementedException();
        }
    }
}