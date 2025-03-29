using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecStore.Application.Entities
{
    public class StepEntity
    {
        public int Id { get; set; }
        public StepType Type { get; set; }
        public required string Text { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
