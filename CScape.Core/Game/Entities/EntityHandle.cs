﻿using System;
using CScape.Models.Game.Entity;
using JetBrains.Annotations;

namespace CScape.Core.Game.Entities
{
    public sealed class EntityHandle : IEntityHandle
    {
        public IEntitySystem System { get; }
        public int Generation { get; }
        public int Id { get; }

        private readonly int _baked;

        public EntityHandle([NotNull] IEntitySystem system, int generation, int id)
        {
            System = system ?? throw new ArgumentNullException(nameof(system));
            Generation = generation;
            Id = id;

            _baked = (Id << EntitySystem.GenerationBits) | Generation;
        }

        public bool Equals(IEntityHandle other)
        {
            return Generation == other.Generation && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is EntityHandle && Equals((EntityHandle) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Generation * 397) ^ Id;
            }
        }

        public override string ToString() => $"Entity handle: Id: {Id} Generation: {Generation} Baked: {_baked}";
    }
}