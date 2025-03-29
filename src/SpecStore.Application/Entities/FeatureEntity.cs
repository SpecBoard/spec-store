using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecStore.Application.Entities
{
    public class FeatureEntity
    {
        public int Id { get; set; }
        public ICollection<TagEntity> Tags { get; set; } = [];
        public required string Name { get; set; }
        public ICollection<RuleEntity> Rules { get; set; } = [];
        public ICollection<ScenarioEntity> Scenarios { get; set; } = [];
    }
}
