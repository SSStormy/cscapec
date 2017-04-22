using System;
using System.Collections;
using System.Collections.Generic;
using CScape.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Game.Worldspace
{
    public class PlaneOfExistance : IEnumerable<AbstractEntity>
    {
        [NotNull]
        public GameServer Server { get; }

        private readonly EntityPool<AbstractEntity> _entityPool;

        public bool IsOverworld => Server.Overworld == this;

        private bool _isFreed;

        public PlaneOfExistance([NotNull] GameServer server)
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));

            _entityPool = new EntityPool<AbstractEntity>();
            Server.Entities.Add(_entityPool);
        }

        ~PlaneOfExistance()
        {
            if (!_isFreed && !IsOverworld)
            {
                Server.Log.Warning(this, "Finalizer called on unfreed PoE.");
                Free();
            }
        }

        public void Free()
        {
            if (_isFreed) return;
            if(IsOverworld) return;

            Server.Entities.Remove(_entityPool);
            _isFreed = true;
        }

        public void RemoveEntity([NotNull] AbstractEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));

            if(!ContainsObservable(ent))
                return;

            _entityPool.Remove(ent);
        }

        public void AddEntity([NotNull] AbstractEntity ent)
        {
            if (ent == null) throw new ArgumentNullException(nameof(ent));
            if (ContainsObservable(ent))
                return;

            _entityPool.Add(ent);
        }

        public bool ContainsObservable([NotNull] AbstractEntity obs)
        {
            if (obs == null) throw new ArgumentNullException(nameof(obs));
            return _entityPool.ContainsId(obs.UniqueEntityId);
        }

        public IEnumerator<AbstractEntity> GetEnumerator()
            => _entityPool.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}