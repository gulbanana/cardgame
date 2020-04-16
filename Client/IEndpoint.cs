using System;

namespace Cardgame.Client
{
    public interface IEndpoint<T>
    {        
        T Subscribe(Action<T> update);
        void Unsubscribe(Action<T> update);
    }
}