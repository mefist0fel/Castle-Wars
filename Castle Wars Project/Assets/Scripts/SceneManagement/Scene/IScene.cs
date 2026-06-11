public interface IScene
{
    string TokenId { get; }

    void Enter(object tokenPayload);

    void Exit();
}
