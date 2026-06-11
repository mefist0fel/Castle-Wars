using CastleWars.Shared.Core;
using UnityEngine;

namespace CastleWars.Visualization
{
    public abstract class EntityVisualizer : MonoBehaviour
    {
        protected GameSession Session { get; private set; }

        public void Bind(GameSession session)
        {
            Session = session;
            session.OnEntityRegistered += HandleRegistered;
            session.OnEntityMutated    += HandleMutated;
        }

        protected abstract void HandleRegistered(BaseEntity entity);
        protected abstract void HandleMutated(BaseEntity entity);

        private void OnDestroy()
        {
            if (Session == null) return;
            Session.OnEntityRegistered -= HandleRegistered;
            Session.OnEntityMutated    -= HandleMutated;
        }
    }
}
