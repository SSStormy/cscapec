﻿using System.Collections.Generic;
using System.Linq;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Entity.Component;
using CScape.Core.Game.Entity.Message;
using CScape.Core.Game.Interface;
using CScape.Core.Network.Packet;
using CScape.Models.Extensions;
using CScape.Models.Game.Entity;
using CScape.Models.Game.Interface;
using CScape.Models.Game.Skill;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Component
{
    [RequiresComponent(typeof(NetworkingComponent))]
    public sealed class SkillNetworkSyncComponent : EntityComponent
    {
        private class SkillModelComparer : IEqualityComparer<ISkillModel>
        {
            public static SkillModelComparer Instance { get; } = new SkillModelComparer();

            private SkillModelComparer()
            {
                
            }

            public bool Equals(ISkillModel x, ISkillModel y) => x.Id.Equals(y.Id);
            public int GetHashCode(ISkillModel obj) => obj.Id.GetHashCode();

        }
        public override int Priority { get; }

        public NetworkingComponent Network => Parent.Components.AssertGet<NetworkingComponent>();
        
        private readonly HashSet<ISkillModel> _dirty 
            = new HashSet<ISkillModel>(SkillModelComparer.Instance);

        public SkillNetworkSyncComponent([NotNull] IEntity parent) : base(parent)
        {

        }

        private void Levelup(ISkillModel skill)
        {
            // fireworks
            Parent.ShowParticleEffect(ParticleEffect.LevelUp);

            // level up dialog
            var interf = Parent.GetInterfaces();
            interf?.Show(
                InterfaceMetadata.Chat(
                    new LevelUpChatInterface(
                        skill.Id.LevelupInterfaceId,
                        skill.Id.Name, 
                        skill.Level)));
        }

        private void GainExp(ExperienceGainMessage gains) => _dirty.Add(gains.Skill);
        
        private void Sync()
        {
            if (!_dirty.Any()) return;

            var net = Network;
            foreach (var skill in _dirty)
            {
                net.SendPacket(new SetSkillDataPacket(
                    skill.Id.ClientIndex, 
                    (int)skill.Experience,
                    (byte)skill.Level.Clamp(0, byte.MaxValue)));
            }
            _dirty.Clear();
        }

        private void MarkAllAsDirty()
        {
            var skills = Parent.GetSkills();
            if (skills == null)
                return;

            foreach (var skill in skills.All.Values)
                _dirty.Add(skill);
        }

        public override void ReceiveMessage(IGameMessage msg)
        {
            switch (msg.EventId)
            {
                case (int)MessageId.NetworkReinitialize:
                case (int)MessageId.Initialize:
                    MarkAllAsDirty();
                    break;
                case (int)MessageId.NetworkPrepare:
                    Sync();
                    break;
                case (int)MessageId.LevelUp:
                    Levelup(msg.AsLevelUp().Skill);
                    break;
                case (int)MessageId.GainExperience:
                    GainExp(msg.AsExperienceGain());
                    break;
            }
        }
    }
}