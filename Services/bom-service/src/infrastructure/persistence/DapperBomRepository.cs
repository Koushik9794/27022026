using BomService.Domain.Aggregates;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BomService.Infrastructure.Persistence
{
    public interface IBomRepository
    {
        Task SaveAsync(BillOfMaterials bom);
    }

    public class DapperBomRepository : IBomRepository
    {
        private readonly IDbConnection _connection;

        public DapperBomRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task SaveAsync(BillOfMaterials bom)
        {
            using var transaction = _connection.BeginTransaction();
            try
            {
                const string bomSql = @"
                    INSERT INTO bill_of_materials (id, configuration_id, project_name, created_at)
                    VALUES (@Id, @ConfigurationId, @ProjectName, @CreatedAt)";
                
                await _connection.ExecuteAsync(bomSql, bom, transaction);

                const string itemSql = @"
                    INSERT INTO bom_items (bom_id, sku, qty, category)
                    VALUES (@BomId, @Sku, @Qty, @Category)";

                foreach (var item in bom.Items)
                {
                    await _connection.ExecuteAsync(itemSql, new 
                    { 
                        BomId = bom.Id, 
                        Sku = item.SKU, 
                        Qty = item.Qty, 
                        Category = item.Category.ToString() 
                    }, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
