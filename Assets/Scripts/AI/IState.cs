namespace MilehighWorld.AI
{
    public interface IState
    {
        void Enter(DesolateEchoAI ai);
        void Tick(DesolateEchoAI ai);
        void Exit(DesolateEchoAI ai);
    }
}
