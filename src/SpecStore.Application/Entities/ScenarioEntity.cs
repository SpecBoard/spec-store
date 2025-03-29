using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecStore.Application.Entities
{
    public class ScenarioEntity
    {
        public int Id { get; set; }
        public ICollection<TagEntity> Tags { get; set; } = [];
        public required string Name { get; set; }
        public ICollection<StepEntity> Steps { get; set; } = [];
    }
}
