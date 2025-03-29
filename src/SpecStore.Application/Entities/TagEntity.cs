using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecStore.Application.Entities
{
    public class TagEntity
    {
        public int Id { get; set; }
        public required string Tag { get; set; }
    }
}
