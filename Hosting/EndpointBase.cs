using System;
using System.Collections.Generic;

namespace Cardgame.Hosting
{
    abstract class EndpointBase<T>
    {
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
            foreach (var subscriber in subscriptions)
            {
                subscriber(model);
            }
        }
    }
}