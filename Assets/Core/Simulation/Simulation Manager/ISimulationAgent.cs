using TWK.Simulation;

namespace TWK.Core
{ 
    public interface ISimulationAgent
    {
        void Initialize(WorldTimeManager worldTimeManager);
        void AdvanceDay();   
        void AdvanceSeason();
        void AdvanceYear();

    }
}
