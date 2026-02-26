using GssCommon.Common.Models.Configurator;
using System;
using System.Collections.Generic;

namespace BomService.Domain.Aggregates
{
    public class BillOfMaterials
    {
        public Guid Id { get; private set; }
        public Guid ConfigurationId { get; private set; }
        public string ProjectName { get; private set; }
        public List<BomItem> Items { get; private set; } = new();
        public DateTime CreatedAt { get; private set; }

        private BillOfMaterials() { }

        public static BillOfMaterials Create(Guid configurationId, string projectName)
        {
            return new BillOfMaterials
            {
                Id = Guid.NewGuid(),
                ConfigurationId = configurationId,
                ProjectName = projectName,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddItems(IEnumerable<BomItem> items)
        {
            Items.AddRange(items);
        }
    }
}
