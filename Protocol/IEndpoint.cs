using System;

namespace Cardgame
{
    public interface IEndpoint<T>
    {        
        T Subscribe(Action<T> update);
        void Unsubscribe(Action<T> update);
    }
}