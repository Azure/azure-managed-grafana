using System;
using System.Collections.Generic;

namespace TestUtility
{
    public static class TrackedResourceDataExtensions
    {
        public static Azure.ResourceManager.Models.TrackedResourceData AppendTags(this Azure.ResourceManager.Models.TrackedResourceData trackedResourceData, IDictionary<string, string> tags)
        {
            if (trackedResourceData == null)
            {
                throw new ArgumentNullException(nameof(trackedResourceData));
            }

            if (tags == null)
            {
                return trackedResourceData;
            }

            foreach (var kvp in tags)
            {
                trackedResourceData.Tags.Add(kvp);
            }

            return trackedResourceData;
        }
    }
}
