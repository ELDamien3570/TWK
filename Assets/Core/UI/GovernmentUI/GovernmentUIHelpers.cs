using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TWK.UI
{
    /// <summary>
    /// Helper utilities for creating government UI dropdowns from enums.
    /// Use this when building reform panels or government creation UI.
    /// </summary>
    public static class GovernmentUIHelpers
    {
        /// <summary>
        /// Populate a TMP_Dropdown with all values from an enum type.
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="dropdown">The dropdown to populate</param>
        /// <param name="selectedValue">The currently selected value (optional)</param>
        public static void PopulateEnumDropdown<T>(TMP_Dropdown dropdown, T? selectedValue = null) where T : struct, Enum
        {
            if (dropdown == null) return;

            dropdown.ClearOptions();

            var options = new List<string>();
            var values = Enum.GetValues(typeof(T));
            int selectedIndex = 0;

            for (int i = 0; i < values.Length; i++)
            {
                T value = (T)values.GetValue(i);
                options.Add(FormatEnumName(value.ToString()));

                if (selectedValue.HasValue && EqualityComparer<T>.Default.Equals(value, selectedValue.Value))
                {
                    selectedIndex = i;
                }
            }

            dropdown.AddOptions(options);
            dropdown.value = selectedIndex;
        }

        /// <summary>
        /// Get the enum value from a dropdown's current selection.
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="dropdown">The dropdown</param>
        /// <returns>The selected enum value</returns>
        public static T GetEnumValueFromDropdown<T>(TMP_Dropdown dropdown) where T : struct, Enum
        {
            if (dropdown == null)
                return default(T);

            var values = Enum.GetValues(typeof(T));
            if (dropdown.value >= 0 && dropdown.value < values.Length)
            {
                return (T)values.GetValue(dropdown.value);
            }

            return default(T);
        }

        /// <summary>
        /// Format an enum name for display (adds spaces before capitals).
        /// Example: "StateStructure" -> "State Structure"
        /// </summary>
        private static string FormatEnumName(string enumName)
        {
            if (string.IsNullOrEmpty(enumName))
                return "";

            string result = "";
            for (int i = 0; i < enumName.Length; i++)
            {
                // Add space before capital letters (except first character)
                if (i > 0 && char.IsUpper(enumName[i]) && !char.IsUpper(enumName[i - 1]))
                {
                    result += " ";
                }
                result += enumName[i];
            }
            return result;
        }

        /// <summary>
        /// Create a labeled dropdown for an enum field.
        /// Returns the dropdown component for further configuration.
        /// </summary>
        /// <param name="parent">Parent transform</param>
        /// <param name="labelText">Label text</param>
        /// <returns>The created dropdown</returns>
        public static TMP_Dropdown CreateLabeledDropdown(Transform parent, string labelText)
        {
            // Create container
            GameObject container = new GameObject($"{labelText}_Container");
            container.transform.SetParent(parent, false);

            RectTransform containerRect = container.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(0, 40);

            HorizontalLayoutGroup layout = container.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.spacing = 10;

            // Create label
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(container.transform, false);

            TextMeshProUGUI label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = labelText;
            label.fontSize = 14;
            label.alignment = TextAlignmentOptions.MidlineLeft;

            RectTransform labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.sizeDelta = new Vector2(150, 0);

            LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
            labelLayout.preferredWidth = 150;

            // Create dropdown
            GameObject dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(container.transform, false);

            TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();

            RectTransform dropdownRect = dropdownObj.GetComponent<RectTransform>();
            dropdownRect.sizeDelta = new Vector2(250, 0);

            LayoutElement dropdownLayout = dropdownObj.AddComponent<LayoutElement>();
            dropdownLayout.preferredWidth = 250;
            dropdownLayout.flexibleWidth = 1;

            // Add background image
            Image background = dropdownObj.AddComponent<Image>();
            background.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            return dropdown;
        }

        // ========== GOVERNMENT ENUM HELPERS ==========

        /// <summary>
        /// Example usage: Populate RegimeForm dropdown
        /// </summary>
        public static void SetupRegimeFormDropdown(TMP_Dropdown dropdown, TWK.Government.RegimeForm currentValue)
        {
            PopulateEnumDropdown(dropdown, currentValue);
        }

        /// <summary>
        /// Example usage: Populate StateStructure dropdown
        /// </summary>
        public static void SetupStateStructureDropdown(TMP_Dropdown dropdown, TWK.Government.StateStructure currentValue)
        {
            PopulateEnumDropdown(dropdown, currentValue);
        }

        /// <summary>
        /// Example usage: Populate SuccessionLaw dropdown
        /// </summary>
        public static void SetupSuccessionLawDropdown(TMP_Dropdown dropdown, TWK.Government.SuccessionLaw currentValue)
        {
            PopulateEnumDropdown(dropdown, currentValue);
        }

        /// <summary>
        /// Example usage: Populate TaxationLaw dropdown
        /// </summary>
        public static void SetupTaxationLawDropdown(TMP_Dropdown dropdown, TWK.Government.TaxationLaw currentValue)
        {
            PopulateEnumDropdown(dropdown, currentValue);
        }

        // Add more specialized dropdown setups as needed for other government enums
    }
}
