#nullable enable
using System.Collections.Generic;
using CastleWars.Shared.Core;
using CastleWars.Shared.Game.Entities;

namespace CastleWars.Shared.Network
{
    public static class SnapshotFactory
    {
        public static EntitySnapshot? Create(BaseEntity entity)
        {
            switch (entity)
            {
                case SessionEntity s:
                    return new SessionSnapshot
                    {
                        EntityId = s.Id, Version = s.Version,
                        MapId = s.MapId, GameTick = s.GameTick,
                        PlayerIds = new List<ulong>(s.PlayerIds)
                    };
                case PlayerEntity p:
                    return new PlayerSnapshot
                    {
                        EntityId = p.Id, Version = p.Version,
                        Name = p.Name, ColorR = p.ColorR, ColorG = p.ColorG, ColorB = p.ColorB,
                        ArmyId = p.ArmyId, IsConnected = p.IsConnected
                    };
                case MapEntity m:
                    return new MapSnapshot
                    {
                        EntityId = m.Id, Version = m.Version,
                        Width = m.Width, Height = m.Height, RegionSize = m.RegionSize,
                        RegionIds = new List<ulong>(m.RegionIds)
                    };
                case RegionEntity r:
                    return new RegionSnapshot
                    {
                        EntityId = r.Id, Version = r.Version,
                        GridX = r.GridX, GridY = r.GridY,
                        ArmyIds = new List<ulong>(r.ArmyIds),
                        CityIds = new List<ulong>(r.CityIds)
                    };
                case ArmyEntity a:
                    return new ArmySnapshot
                    {
                        EntityId = a.Id, Version = a.Version,
                        OwnerId = a.OwnerId, X = a.X, Y = a.Y,
                        RegionId = a.RegionId, Health = a.Health, IsDead = a.IsDead
                    };
                case CityEntity c:
                    return new CitySnapshot
                    {
                        EntityId = c.Id, Version = c.Version,
                        Name = c.Name, X = c.X, Y = c.Y,
                        OwnerId = c.OwnerId, VisualVariant = c.VisualVariant
                    };
                default:
                    return null;
            }
        }

        public static ulong GetEntityId(EntitySnapshot snap)
        {
            switch (snap)
            {
                case SessionSnapshot s: return s.EntityId;
                case PlayerSnapshot p:  return p.EntityId;
                case MapSnapshot m:     return m.EntityId;
                case RegionSnapshot r:  return r.EntityId;
                case ArmySnapshot a:    return a.EntityId;
                case CitySnapshot c:    return c.EntityId;
                default:                return 0;
            }
        }
    }
}
