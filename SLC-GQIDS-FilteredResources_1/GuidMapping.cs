namespace SLC_GQIDS_FilteredResources_1
{
    using System;
    using System.Collections.Generic;

    internal static class GuidMapping
    {
        internal static readonly Dictionary<string, Guid> Mapping = new Dictionary<string, Guid>
        {
            {"Max Quantity", Guid.Parse("edf81906-8bd1-4fd7-aa86-ef2ecd25f0b4")},
            {"Bandwidth", Guid.Parse("4d8a960c-754c-46f4-b5aa-b0c5ae9750ba")},
            {"Type", Guid.Parse("c1f9c8bb-8f82-4941-833b-2d8506c9f382")},
        };
    }
}