using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Uwp.SampleApp.Models
{
    [System.AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class SampleMetadataAttribute : Attribute, ISampleMetadata
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        // Not required on individual Sample as retrieved from parent.
        public string Icon => throw new NotImplementedException();

        // Not required on individual Sample as retrieved from parent.
        public string BadgeUpdateVersionRequired => throw new NotImplementedException();

        public SampleMetadataAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public SampleMetadataAttribute(string name, string description, string[] tags)
            : this(name, description)
        {
            Tags = tags.ToList();
        }
    }
}
