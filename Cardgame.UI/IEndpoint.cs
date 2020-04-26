using System;

namespace Cardgame.UI
{
    public interface IEndpoint<T>
    {        
        T Subscribe(Action<T> update);
        void Unsubscribe(Action<T> update);
    }
}