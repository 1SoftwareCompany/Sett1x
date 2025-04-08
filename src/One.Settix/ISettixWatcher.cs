using Microsoft.Extensions.Primitives;
using System;

namespace One.Settix
{
    public interface ISettixWatcher : IDisposable
    {
        IChangeToken Watch();
    }
}
