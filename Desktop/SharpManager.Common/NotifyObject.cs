using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager
{
    public abstract class NotifyObject : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <returns></returns>
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Dictionary<string, object> properties = new();

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        protected T GetProperty<T>([CallerMemberName] string? name = null)
        {
            if (name == null) throw new ArgumentNullException(name);
            #pragma warning disable CS8603 // Possible null return
            if (!properties.ContainsKey(name)) return default;
            #pragma warning restore CS8603 // Possible null return
            return (T)properties[name];
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected T GetProperty<T>(T defaultValue, [CallerMemberName] string? name = null)
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
    }
}
