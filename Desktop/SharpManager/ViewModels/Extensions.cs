using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SharpManager.ViewModels
{
    public static class Extensions
    {
        /// <summary>
        /// Propagates the property changed event of another object
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="destination">The destination.</param>
        /// <param name="source">The source.</param>
        /// <param name="propertyLambda">The property expression.</param>
        /// <param name="localPropertyLambdas">The local property expression.</param>
        public static void PropagatePropertyChanged<TSource, TDestination>(this TDestination destination, TSource source, Expression<Func<TSource, object>> propertyLambda, params Expression<Func<TDestination, object>>[] localPropertyLambdas) where TDestination : BaseViewModel where TSource : INotifyPropertyChanged
        {
            string? propertyName = propertyLambda.GetMemberName();
            var localProperties = new List<string>();
            foreach (var localPropertyLambda in localPropertyLambdas)
            {
                var localPropertyName = localPropertyLambda.GetMemberName();
                if (localPropertyName == null) throw new ArgumentException($"Member name not found in lambda expression '{localPropertyLambda}'", nameof(localPropertyLambdas));
                localProperties.Add(localPropertyName);
            }
            if (propertyName == null) throw new ArgumentException($"Member name not found in lambda expression '{propertyLambda}'", nameof(propertyLambda));
            if (propertyName == null) throw new ArgumentException("Member name not found in lambda expression", nameof(propertyLambda));
            destination.PropagatePropertyChanged(source, propertyName, localProperties.ToArray());
        }
    }
}
