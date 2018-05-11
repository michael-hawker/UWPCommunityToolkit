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
