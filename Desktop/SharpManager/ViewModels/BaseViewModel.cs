using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SharpManager.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <returns></returns>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The propagated properties
        /// </summary>
        private readonly ConditionalWeakTable<INotifyPropertyChanged, Dictionary<string, List<string>>> propagatedProperties = new();

        private readonly Dictionary<string, object> properties = new();

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected T GetProperty<T>(T defaultValue = default, [CallerMemberName] string? name = null)
        {
            if (name == null) throw new ArgumentNullException(name);
            if (!properties.ContainsKey(name)) return defaultValue;
            return (T)properties[name];
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected bool SetProperty<T>(T value, [CallerMemberName] string? name = null) 
        {
            if (name == null) throw new ArgumentNullException(name);
            if (!properties.ContainsKey(name)) 
            {
                if (value == null || value.Equals(default)) return false;
            }
            else
            {
                T currentValue = (T)properties[name];
                if (currentValue == null && value == null) return false;
                if (currentValue != null && currentValue.Equals(value)) return false;
            }
            #pragma warning disable CS8601 // Possible null reference assignment.
            properties[name] = value;
            #pragma warning restore CS8601 // Possible null reference assignment.
            OnPropertyChanged(name);
            return true;
        }

        /// <summary>
        /// Sets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="action">The action.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected bool SetProperty<T>(T value, Action action, [CallerMemberName] string? name = null)
        {
            bool result = SetProperty(value, name);
            if (result) action?.Invoke();
            return result;
        }

        /// <summary>
        /// Sets the property value and raises the changed event.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="value">The value.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>True if property was changed</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(name);
            return true;
        }

        /// <summary>
        /// Called when the property changed.
        /// </summary>
        /// <param name="name">The property name.</param>
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Propagates the property changed event of another object
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="instancePropertyName">Name of the instance property.</param>
        /// <param name="localProperties">The local properties.</param>
        internal protected void PropagatePropertyChanged(INotifyPropertyChanged instance, string instancePropertyName, params string[] localProperties)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var instanceProperties = propagatedProperties.GetOrCreateValue(instance);
            if (!instanceProperties.TryGetValue(instancePropertyName, out var thisProperties))
            {
                thisProperties = new List<string>();
                instanceProperties.Add(instancePropertyName, thisProperties);
            }
            thisProperties.AddRange(localProperties);
            instance.PropertyChanged -= PropagatePropertyChangeEventHandler;
            instance.PropertyChanged += PropagatePropertyChangeEventHandler;
        }

        /// <summary>
        /// Propagating property change event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void PropagatePropertyChangeEventHandler(object? sender, PropertyChangedEventArgs e)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (!propagatedProperties.TryGetValue((INotifyPropertyChanged)sender, out var fields)) return;
            if (e.PropertyName == null || !fields.ContainsKey(e.PropertyName)) return;
            foreach (var propertyName in fields[e.PropertyName]) OnPropertyChanged(propertyName);
        }
    }
}
