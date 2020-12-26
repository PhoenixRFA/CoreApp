using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

//https://github.com/aspnet/Mvc/blob/master/src/Microsoft.AspNetCore.Mvc.Core/Routing/KnownRouteValueConstraint.cs

namespace MVCApp.Constraints
{
    public class MyKnownRouteValueConstraint : IRouteConstraint
    {
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private RouteValuesCollection _cachedValuesCollection;

        public MyKnownRouteValueConstraint(IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            if (actionDescriptorCollectionProvider == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptorCollectionProvider));
            }

            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        }

        public bool Match(
            HttpContext httpContext,
            IRouter route,
            string routeKey,
            RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (routeKey == null)
            {
                throw new ArgumentNullException(nameof(routeKey));
            }

            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (values.TryGetValue(routeKey, out var obj))
            {
                var value = Convert.ToString(obj, CultureInfo.InvariantCulture);
                if (value != null)
                {
                    var actionDescriptors = GetAndValidateActionDescriptors(httpContext);

                    var allValues = GetAndCacheAllMatchingValues(routeKey, actionDescriptors);
                    foreach (var existingValue in allValues)
                    {
                        if (string.Equals(value, existingValue, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private ActionDescriptorCollection GetAndValidateActionDescriptors(HttpContext httpContext)
        {
            var actionDescriptorsProvider = _actionDescriptorCollectionProvider;

            if (actionDescriptorsProvider == null)
            {
                // Only validate that HttpContext was passed to constraint if it is needed
                if (httpContext == null)
                {
                    throw new ArgumentNullException(nameof(httpContext));
                }

                var services = httpContext.RequestServices;
                actionDescriptorsProvider = services.GetRequiredService<IActionDescriptorCollectionProvider>();
            }

            var actionDescriptors = actionDescriptorsProvider.ActionDescriptors;
            if (actionDescriptors == null)
            {
                throw new InvalidOperationException(nameof(IActionDescriptorCollectionProvider.ActionDescriptors));
            }

            return actionDescriptors;
        }

        private string[] GetAndCacheAllMatchingValues(string routeKey, ActionDescriptorCollection actionDescriptors)
        {
            var version = actionDescriptors.Version;
            var valuesCollection = _cachedValuesCollection;

            if (valuesCollection == null ||
                version != valuesCollection.Version)
            {
                var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < actionDescriptors.Items.Count; i++)
                {
                    var action = actionDescriptors.Items[i];

                    if (action.RouteValues.TryGetValue(routeKey, out var value) &&
                        !string.IsNullOrEmpty(value))
                    {
                        values.Add(value);
                    }
                }

                valuesCollection = new RouteValuesCollection(version, values.ToArray());
                _cachedValuesCollection = valuesCollection;
            }

            return _cachedValuesCollection.Items;
        }

        private class RouteValuesCollection
        {
            public RouteValuesCollection(int version, string[] items)
            {
                Version = version;
                Items = items;
            }

            public int Version { get; }

            public string[] Items { get; }
        }
    }
}
