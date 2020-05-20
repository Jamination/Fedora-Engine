using System;

namespace FedoraEngine
{
    public sealed class Export : Attribute
    {
        public string Name { get; set; }
    }
}
