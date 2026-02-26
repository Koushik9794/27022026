using FluentMigrator;

namespace RuleService.Infrastructure.Migrations
{
    [Migration(20260125002)]
    public class M20260125002_AddFieldMetadataTable : Migration
    {
        public override void Up()
        {
            // Create field_metadata table for storing field display names and units
            Create.Table("field_metadata")
                .WithColumn("field_name").AsString(100).PrimaryKey()
                .WithColumn("display_name").AsString(200).NotNullable()
                .WithColumn("unit").AsString(20).Nullable()
                .WithColumn("data_type").AsString(50).NotNullable() // number, text, boolean
                .WithColumn("category").AsString(50).Nullable() // spatial, structural, etc.
                .WithColumn("description").AsString(500).Nullable()
                .WithColumn("created_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime)
                .WithColumn("updated_at").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentDateTime);

            // Insert default field metadata
            Execute.Sql(@"
                INSERT INTO field_metadata (field_name, display_name, unit, data_type, category, description) VALUES
                ('levels', 'Number of Levels', '', 'number', 'structural', 'Total number of storage levels in the rack'),
                ('palletWidth', 'Pallet Width', 'mm', 'number', 'spatial', 'Width of the pallet'),
                ('palletDepth', 'Pallet Depth', 'mm', 'number', 'spatial', 'Depth of the pallet'),
                ('palletHeight', 'Pallet Height', 'mm', 'number', 'spatial', 'Height of the pallet'),
                ('palletWeight', 'Pallet Weight', 'kg', 'number', 'structural', 'Weight of the loaded pallet'),
                ('palletOverhang', 'Pallet Overhang', 'mm', 'number', 'spatial', 'Overhang distance of pallet beyond beam'),
                ('frameDepth', 'Frame Depth', 'mm', 'number', 'spatial', 'Depth of the rack frame'),
                ('frameWidth', 'Frame Width', 'mm', 'number', 'spatial', 'Width of the rack frame'),
                ('beamSpan', 'Beam Span', 'mm', 'number', 'structural', 'Distance between uprights'),
                ('uprightHeight', 'Upright Height', 'mm', 'number', 'structural', 'Total height of the upright column'),
                ('levelHeight', 'Level Height', 'mm', 'number', 'spatial', 'Height between levels'),
                ('aisleWidth', 'Aisle Width', 'mm', 'number', 'spatial', 'Width of the aisle between racks'),
                ('seismicZone', 'Seismic Zone', '', 'text', 'compliance', 'Seismic zone classification'),
                ('anchorType', 'Anchor Type', '', 'text', 'structural', 'Type of floor anchor used'),
                ('concreteGrade', 'Concrete Grade', '', 'text', 'structural', 'Grade of floor concrete');
            ");
        }

        public override void Down()
        {
            Delete.Table("field_metadata");
        }
    }
}
