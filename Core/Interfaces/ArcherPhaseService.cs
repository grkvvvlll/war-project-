using gaaameee.Core.Interfaces;

namespace gaaameee.Core.Interfaces
{
    public interface IArcherPhaseService
    {
        int Execute(IArmy army,
                     IArmy enemy,
                     bool isArmy1);
    }
}