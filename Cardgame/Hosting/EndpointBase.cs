using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Cardgame.Model;

namespace Cardgame.Hosting
{
    abstract class EndpointBase<T>
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters =
            {
                new InstanceDictionaryConverter<Instance>()
            }
        };
        private readonly List<Action<T>> subscriptions;

        public EndpointBase()
        {
            subscriptions = new List<Action<T>>();
        }

        protected abstract T GetModel();

        public T Subscribe(Action<T> update)
        {
            subscriptions.Add(update);
            return GetModel();
        }

        public void Unsubscribe(Action<T> update)
        {
            subscriptions.Remove(update);
        }

        protected void Notify()
        {
            var model = GetModel();
            var serialised = JsonSerializer.Serialize(model, options);
            foreach (var subscriber in subscriptions.ToList())
            {
                var clone = JsonSerializer.Deserialize<T>(serialised, options);
                subscriber(clone);
            }
        }
    }
}