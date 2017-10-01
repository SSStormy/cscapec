﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CScape.Core.Game.Entities.Fragment;
using CScape.Core.Game.Entities.Fragment.Component;
using CScape.Core.Game.Entities.Interface;
using CScape.Core.Game.Entity;
using CScape.Core.Injection;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class Entity : IEquatable<Entity>, IEnumerable<IEntityFragment>
    {
        public sealed class EntityFragmentContainer<TFragment>
            : IEnumerable<TFragment>
            where TFragment : class, IEntityFragment
        {
            [NotNull]
            public Entity Parent { get; }

            private readonly Dictionary<Type, TFragment> _lookup
                = new Dictionary<Type, TFragment>();

            // TODO : write tests for entity fragment sorting
            [NotNull]
            public IEnumerable<TFragment> All { get; private set; } = Enumerable.Empty<TFragment>();

            public EntityFragmentContainer([NotNull] Entity parent)
            {
                Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            }

            private void Sort()
            {
                All = _lookup.Values.OrderBy(f => f.Priority);
            }

            public void Add<T>([NotNull] T fragment)
                where T : class, TFragment
            {
                if (fragment == null) throw new ArgumentNullException(nameof(fragment));

                var type = typeof(T);

                if (ContainsFragment<T>())
                    throw new EntityComponentAlreadyExists(type);

                _lookup.Add(type, fragment);
                Sort();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool ContainsFragment<T>()
                where T : class, TFragment
                => _lookup.ContainsKey(typeof(T));

            [CanBeNull]
            public T Get<T>()
                where T : class, TFragment
            {
                var type = typeof(T);

                if (!ContainsFragment<T>())
                    return null;

                return (T)_lookup[type];
            }

            public void Remove<T>()
                where T : class, TFragment
            {
                // TODO : assert that fragment requirements are still satisfied after removal of fragment

                var type = typeof(T);

                if (!ContainsFragment<T>())
                    return;

                var statusLookup = _lookup.Remove(type);

                Debug.Assert(statusLookup);
            }

            public IEnumerator<TFragment> GetEnumerator() => All.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        }

        [NotNull]
        public string Name { get; }

        [NotNull]
        public EntityHandle Handle { get; }

        [NotNull]
        public IGameServer Server => Handle.System.Server;

        public ILogger Log => Handle.System.Server.Services.ThrowOrGet<ILogger>();

        public ServerTransform GetTransform() => Components.Get<ServerTransform>();

        public EntityFragmentContainer<IEntityComponent> Components { get; }
        public EntityFragmentContainer<IEntityNetFragment> Network { get; }

        public Entity([NotNull] string name, [NotNull] EntityHandle handle)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Handle = handle ?? throw new ArgumentNullException(nameof(handle));

            Components = new EntityFragmentContainer<IEntityComponent>(this);
            Network = new EntityFragmentContainer<IEntityNetFragment>(this);
        }

        public bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Handle.Equals(other.Handle);
        }

        public IEnumerator<IEntityFragment> GetEnumerator()
        {
            foreach (var c in Components) yield return c;
            foreach (var c in Network) yield return c;
        }

        public void DoTickUpdate([NotNull] IMainLoop loop)
        {
            foreach(var component in Components)
                component.Update(loop);
        }

        public void DoNetworkUpdate([NotNull] IMainLoop loop)
        {
            var net = Components.Get<NetworkingComponent>();

            net?.Sync(loop);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Entity && Equals((Entity) obj);
        }

        /// <summary>
        /// Sends out an <see cref="EntityMessage"/> to each and every component
        /// of this entity. The sender of the message will not receive the message.
        /// </summary>
        /// <param name="message">The message to be sent.</param>
        public void SendMessage([NotNull] EntityMessage message)
        {
            foreach (var frag in this)
            {
                frag.ReceiveMessage(message);
            }
        }

        /// <summary>
        /// Sends a system message to the entity.
        /// </summary>
        public void SystemMessage([NotNull] string msg)
        {
            if (msg == null) throw new ArgumentNullException(nameof(msg));

            SendMessage(
                new EntityMessage(
                    null, EntityMessage.EventType.NewSystemMessage, msg));
        }

        /// <summary>
        /// Asserts that are component dependencies are satisfied.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="EntityComponentNotSatisfied">Thrown, when a component is not satisfied.</exception>
        public void AssertComponentRequirementsSatisfied()
        {
            foreach (var frag in this)
            {
                foreach (var attrib in
                    frag.GetType().GetTypeInfo().GetCustomAttributes<RequiresFragment>())
                {
                    // look for required attrib

                    var match = this.FirstOrDefault(c => c.GetType() == attrib.FragmentType);
                    if (match == null)
                    {
                        throw new EntityComponentNotSatisfied
                            (frag.GetType(), $"Requires fragment of type {attrib.FragmentType.Name} to be attached to the entity but it is not.");
                    }
                }
            }
        }

        public override int GetHashCode()
        {
            return Handle.GetHashCode() * 13;
        }

        public override string ToString()
        {
            return $"Entity \"{Name}\" {Handle}";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}


