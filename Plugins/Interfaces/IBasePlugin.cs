﻿namespace Plugins.Interfaces
{
    public interface IBasePlugin
    {
        public string Name { get; }

        Task InitAsync();

        public string? AddSubscription(ulong id, params string[] additionalInfo);

        public string? UpdateSubscription(ulong id, string oldData, params string[] additionalInfo);
    }
}