namespace Core.Interfaces
{
    // юнит может быть клонирован
    public interface ICanBeCloned
    {
        IUnit Clone(IRandomService random);
    }
}