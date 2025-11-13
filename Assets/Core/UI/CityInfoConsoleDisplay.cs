using UnityEngine;
using TWK.UI.ViewModels;

namespace TWK.UI
{
    /// <summary>
    /// Simple console-based debug display for city information.
    /// Logs city ViewModel data to the Unity console.
    /// Useful for quick testing without setting up UI components.
    /// </summary>
    public class CityInfoConsoleDisplay : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int cityId = 0;
        [SerializeField] private bool logOnStart = true;
        [SerializeField] private bool autoLog = false;
        [SerializeField] private float logInterval = 5f;

        private float lastLogTime;

        private void Start()
        {
            if (logOnStart)
            {
                Invoke(nameof(LogCityInfo), 0.5f); // Small delay to ensure ViewModels are initialized
            }
        }

        private void Update()
        {
            if (autoLog && Time.time - lastLogTime > logInterval)
            {
                LogCityInfo();
                lastLogTime = Time.time;
            }
        }

        [ContextMenu("Log City Info")]
        public void LogCityInfo()
        {
            if (ViewModelService.Instance == null)
            {
                Debug.LogWarning("[CityInfoConsoleDisplay] ViewModelService not found");
                return;
            }

            var cityVM = ViewModelService.Instance.GetCityViewModel(cityId);

            if (cityVM == null)
            {
                Debug.LogWarning($"[CityInfoConsoleDisplay] No ViewModel found for city ID: {cityId}");
                return;
            }

            LogFullCityReport(cityVM);
        }

        [ContextMenu("Log All Cities")]
        public void LogAllCities()
        {
            if (ViewModelService.Instance == null)
            {
                Debug.LogWarning("[CityInfoConsoleDisplay] ViewModelService not found");
                return;
            }

            foreach (var cityVM in ViewModelService.Instance.GetAllCityViewModels())
            {
                LogFullCityReport(cityVM);
                Debug.Log("=====================================\n");
            }
        }

        private void LogFullCityReport(CityViewModel vm)
        {
            Debug.Log($"╔════════════════════════════════════════════════════════════════");
            Debug.Log($"║ <b>{vm.CityName}</b> (ID: {vm.CityID})");
            Debug.Log($"╠════════════════════════════════════════════════════════════════");

            // Population Summary
            Debug.Log($"║ <b>POPULATION</b>: {vm.TotalPopulation:N0}");
            Debug.Log($"║   Growth Rate: {vm.GrowthRate:F2}");
            Debug.Log($"║   Average Age: {vm.AverageAge:F1} years");
            Debug.Log($"║   Gender: Male {vm.MalePercentage:F1}% | Female {vm.FemalePercentage:F1}%");
            Debug.Log($"║");

            // Population Breakdown
            Debug.Log($"║ <b>POPULATION BY CLASS</b>");
            Debug.Log($"║   Slaves:    {vm.SlaveCount,8:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Slave):F1}%)");
            Debug.Log($"║   Laborers:  {vm.LaborerCount,8:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Laborer):F1}%)");
            Debug.Log($"║   Artisans:  {vm.ArtisanCount,8:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Artisan):F1}%)");
            Debug.Log($"║   Merchants: {vm.MerchantCount,8:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Merchant):F1}%)");
            Debug.Log($"║   Nobles:    {vm.NobleCount,8:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Noble):F1}%)");
            Debug.Log($"║   Clergy:    {vm.ClergyCount,8:N0}  ({vm.GetArchetypePercentage(Realms.Demographics.PopulationArchetypes.Clergy):F1}%)");
            Debug.Log($"║");

            // Demographics
            Debug.Log($"║ <b>DEMOGRAPHICS</b>");
            Debug.Log($"║   Youth (0-17):   {vm.TotalYouth,8:N0}  ({(vm.TotalPopulation > 0 ? (vm.TotalYouth / (float)vm.TotalPopulation) * 100f : 0):F1}%)");
            Debug.Log($"║   Adults (18-45): {vm.TotalAdults,8:N0}  ({(vm.TotalPopulation > 0 ? (vm.TotalAdults / (float)vm.TotalPopulation) * 100f : 0):F1}%)");
            Debug.Log($"║   Middle (46-60): {vm.TotalMiddleAge,8:N0}  ({(vm.TotalPopulation > 0 ? (vm.TotalMiddleAge / (float)vm.TotalPopulation) * 100f : 0):F1}%)");
            Debug.Log($"║   Elderly (60+):  {vm.TotalElderly,8:N0}  ({(vm.TotalPopulation > 0 ? (vm.TotalElderly / (float)vm.TotalPopulation) * 100f : 0):F1}%)");
            Debug.Log($"║");

            // Aggregate Stats
            Debug.Log($"║ <b>AGGREGATE STATS</b>");
            Debug.Log($"║   Average Wealth:     {vm.AverageWealth:F1}");
            Debug.Log($"║   Average Education:  {vm.AverageEducation:F1}");
            Debug.Log($"║   Average Fervor:     {vm.AverageFervor:F1}");
            Debug.Log($"║   Average Loyalty:    {vm.AverageLoyalty:F1}");
            Debug.Log($"║   Average Happiness:  {vm.AverageHappiness * 100f:F1}%");
            Debug.Log($"║");

            // Labor & Military
            Debug.Log($"║ <b>LABOR FORCE</b>");
            Debug.Log($"║   Available Workers: {vm.TotalAvailableWorkers:N0}");
            Debug.Log($"║   Employed:          {vm.TotalEmployed:N0}");
            Debug.Log($"║   Unemployed:        {vm.TotalUnemployed:N0} ({vm.UnemploymentRateFormatted})");
            Debug.Log($"║");
            Debug.Log($"║ <b>MILITARY</b>");
            Debug.Log($"║   Eligible:          {vm.TotalMilitaryEligible:N0} ({vm.MilitaryEligiblePercentage:F1}% of total)");
            Debug.Log($"║");

            // Economy
            string foodStatus = vm.DailyFoodNet >= 0 ? "SURPLUS" : "DEFICIT";
            Debug.Log($"║ <b>ECONOMY</b>");
            Debug.Log($"║   Food Stockpile:    {vm.FoodStockpile,8:N0}  ({vm.DailyFoodNet:+#;-#;0}/day) {foodStatus}");
            Debug.Log($"║   Gold Stockpile:    {vm.GoldStockpile,8:N0}");
            Debug.Log($"║   Buildings:         {vm.BuildingCount}");
            Debug.Log($"║");

            // Social Mobility
            Debug.Log($"║ <b>SOCIAL MOBILITY</b>");
            Debug.Log($"║   {vm.GetSocialMobilitySummary()}");
            Debug.Log($"║");

            // Population Groups Detail
            Debug.Log($"║ <b>POPULATION GROUPS ({vm.PopulationGroups.Count})</b>");
            foreach (var popVM in vm.PopulationGroups)
            {
                Debug.Log($"║   ─────────────────────────────────────────");
                Debug.Log($"║   <b>{popVM.ArchetypeName}</b> (ID: {popVM.ID})");
                Debug.Log($"║     Population:  {popVM.TotalPopulation:N0}");
                Debug.Log($"║     Average Age: {popVM.AverageAge:F0}");
                Debug.Log($"║     Workers:     {popVM.AvailableWorkers} available / {popVM.EmployedCount} employed");
                Debug.Log($"║     Military:    {popVM.MilitaryEligible} eligible");
                Debug.Log($"║     Education:   {popVM.EducationFormatted}");
                Debug.Log($"║     Wealth:      {popVM.WealthFormatted}");
                Debug.Log($"║     Happiness:   {popVM.HappinessFormatted}");
                Debug.Log($"║     Loyalty:     {popVM.LoyaltyFormatted}");
                Debug.Log($"║     Fervor:      {popVM.FervorFormatted}");
                Debug.Log($"║     Status:      {popVM.GetPromotionStatus()}");
                Debug.Log($"║     Risk:        {popVM.GetDemotionRisk()}");
            }

            Debug.Log($"╚════════════════════════════════════════════════════════════════");
        }

        public void SetCityId(int newCityId)
        {
            cityId = newCityId;
        }
    }
}
