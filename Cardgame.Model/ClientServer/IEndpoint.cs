using System;

namespace Cardgame.Model.ClientServer
{
    public interface IEndpoint<T>
    {        
        T Subscribe(Action<T> update);
        void Unsubscribe(Action<T> update);
    }
}