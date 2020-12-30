using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace MVCApp.Infrastructure.ValueProviders
{
    public class CookieValueProvider : BindingSourceValueProvider, IEnumerableValueProvider
    {
        private readonly IRequestCookieCollection _values;
        private PrefixContainer _prefixContainer;

        public CookieValueProvider(BindingSource bindingSource, IRequestCookieCollection cookies) : base(bindingSource)
        {
            if (bindingSource == null)
            {
                throw new ArgumentNullException(nameof(bindingSource));
            }

            if (cookies == null)
            {
                throw new ArgumentNullException(nameof(cookies));
            }

            _values = cookies;
        }

        protected PrefixContainer PrefixContainer => _prefixContainer ??= new PrefixContainer(_values.Keys);

        public override bool ContainsPrefix(string prefix)
        {
            return PrefixContainer.ContainsPrefix(prefix);
            //OR
            //return _values.Keys.Contains(prefix);
        }

        public virtual IDictionary<string, string> GetKeysFromPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            return PrefixContainer.GetKeysFromPrefix(prefix);
            //OR
            //return _values.Keys.Where(x => x.StartsWith(prefix)).ToDictionary(x => x);
        }


        public override ValueProviderResult GetValue(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length == 0)
            {
                return ValueProviderResult.None;
            }

            string value = _values[key];

            if (string.IsNullOrEmpty(value))
            {


                return ValueProviderResult.None;
            }

            return new ValueProviderResult(value);
        }

        public override IValueProvider Filter(BindingSource bindingSource)
        {
            if (bindingSource == null)
            {
                throw new ArgumentNullException(nameof(bindingSource));
            }

            if (bindingSource.Id == "Cookie")
            {
                return this;
            }

            return null;
        }
    }
}
