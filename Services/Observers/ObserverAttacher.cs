using Core.Interfaces;

namespace Services.Observers
{
    public class ObserverAttacher
    {
        public void AttachArmy(IArmy army)
        {
            ObserverRegistry.Instance.Attach(army);
        }

        public void DetachArmy(IArmy army)
        {
            ObserverRegistry.Instance.Detach(army);
        }
    }
}