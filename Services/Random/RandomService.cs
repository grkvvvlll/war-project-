using gaaameee.Core.Interfaces;

namespace Services.Random
{
    public class RandomService : IRandomService
    {
        private readonly System.Random _random = new System.Random();
        public int Next(int minInclusive, int maxExclusive)
        {
            return _random.Next(minInclusive, maxExclusive);
        }
    }
}