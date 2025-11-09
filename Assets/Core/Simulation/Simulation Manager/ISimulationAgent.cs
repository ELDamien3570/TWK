using TWK.Simulation;

namespace TWK.Core
{ 
    public interface ISimulationAgent
    {
        public virtual void Initialize(WorldTimeManager worldTimeManager) { }
        public virtual void AdvanceDay() { }    
        public virtual void AdvanceSeason() { }
        public virtual void AdvanceYear() { }

    }
}
