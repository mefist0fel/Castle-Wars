#nullable enable
using System;
using System.Collections.Generic;

namespace CastleWars.Shared.Core
{
    // Outer container: owns the registry, registers command handlers,
    // routes Apply() calls, and exposes entity change events for replication.
    public abstract class GameSession
    {
        protected readonly EntityRegistry Registry = new();
        private readonly Dictionary<Type, Func<ILogicCommand, bool>> _handlers = new();

        // Proxy events from the registry so external code subscribes here,
        // not directly to the registry.
        public event Action<BaseEntity> OnEntityRegistered
        {
            add    => Registry.OnEntityRegistered += value;
            remove => Registry.OnEntityRegistered -= value;
        }
        public event Action<BaseEntity> OnEntityMutated
        {
            add    => Registry.OnEntityMutated += value;
            remove => Registry.OnEntityMutated -= value;
        }

        protected void RegisterHandler<TCommand>(CommandHandler<TCommand> handler)
            where TCommand : ILogicCommand
        {
            _handlers[typeof(TCommand)] = cmd =>
            {
                var typed = (TCommand)cmd;
                if (!handler.CanExecute(typed, Registry)) return false;
                handler.Execute(typed, Registry);
                return true;
            };
        }

        // Returns false if CanExecute rejected the command; throws if no handler registered.
        public bool Apply(ILogicCommand command)
        {
            if (!_handlers.TryGetValue(command.GetType(), out var invoke))
                throw new InvalidOperationException($"No handler registered for {command.GetType().Name}");
            return invoke(command);
        }

        public T? Get<T>(ulong id) where T : BaseEntity
            => Registry.Get<T>(id);

        public IEnumerable<T> Query<T>() where T : BaseEntity
            => Registry.GetAll<T>();

        public void Mutate(BaseEntity entity)
            => Registry.Mutate(entity);
    }
}
