using System;
using UnityEngine;

namespace TWK.UI.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels.
    /// Provides property change notification for UI binding.
    /// </summary>
    public abstract class BaseViewModel
    {
        /// <summary>
        /// Invoked when any property in this ViewModel changes.
        /// </summary>
        public event Action OnPropertyChanged;

        /// <summary>
        /// Notify listeners that a property has changed.
        /// Call this after updating any property that the UI should react to.
        /// </summary>
        protected void NotifyPropertyChanged()
        {
            OnPropertyChanged?.Invoke();
        }

        /// <summary>
        /// Update all properties from the data source.
        /// Override this in derived classes to refresh data.
        /// </summary>
        public abstract void Refresh();
    }
}
