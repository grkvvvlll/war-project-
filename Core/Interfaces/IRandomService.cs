namespace Core.Interfaces
{
    public interface IRandomService
    {
        int Next(int minInclusive, int maxExclusive);
    }
}