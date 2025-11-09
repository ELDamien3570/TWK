using TWK.Simulation;

namespace TWK.Core
{ 
    public interface ISimulationAgent
    {
        void Initialize(WorldTimeManager worldTimeManager);
        void AdvanceDay();   
        public virtual void AdvanceSeason();
        public virtual void AdvanceYear();

    }
}
