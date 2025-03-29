using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecStore.Application.Entities
{
    public class ReportEntity : ITrackedEntity
    {
        public int Id { get; set; }
        public ICollection<MetadataEntity> Metadata { get; set; } = [];
        public ICollection<FeatureEntity> Features { get; set; } = [];

        public required DateTimeOffset UploadedAt { get; set; }
    }
}
