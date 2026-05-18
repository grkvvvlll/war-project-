using Core.Interfaces;
using Services.Observers;

namespace Services.Observers
{
    public class ObserverAttacher
    {
        public void AttachArmy(IArmy army)
        {
            ObserverRegistry.Attach(army);
        }

        public void DetachArmy(IArmy army)
        {
            ObserverRegistry.Detach(army);
        }
    }
}