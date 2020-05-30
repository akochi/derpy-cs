using Discord;
using System.Collections.Generic;

namespace Derpy
{
    public class EntityComparer : IEqualityComparer<ISnowflakeEntity>
    {
        public bool Equals(ISnowflakeEntity left, ISnowflakeEntity right) => left.Id == right.Id;
        public int GetHashCode(ISnowflakeEntity entity) => entity.Id.GetHashCode();

        public static readonly EntityComparer Instance = new EntityComparer();
    }
}
