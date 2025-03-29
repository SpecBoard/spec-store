using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecStore.Application.Entities
{
    public class MetadataEntity
    {
        public int Id { get; set; }
        public required string Key { get; set; }
        public required string Value { get; set; }
    }
}
