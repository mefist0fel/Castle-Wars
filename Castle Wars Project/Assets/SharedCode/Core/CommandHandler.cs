#nullable enable
namespace CastleWars.Shared.Core
{
    public abstract class CommandHandler<TCommand> where TCommand : ILogicCommand
    {
        // Override to validate preconditions before Execute is called.
        // Return false to reject the command — Execute will not be called.
        public virtual bool CanExecute(TCommand command, EntityRegistry registry) => true;

        public abstract void Execute(TCommand command, EntityRegistry registry);
    }
}
