namespace Store.ClientFSM.States
{
    public class NoneState : ClientBaseState
    {
        public NoneState(Client client) : base(client)
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