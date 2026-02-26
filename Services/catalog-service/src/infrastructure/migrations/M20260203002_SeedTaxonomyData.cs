using FluentMigrator;
using System.Data;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260203002)]
public class SeedDetailedTaxonomyData : Migration
{
    public override void Up()
    {
        // 1. Relax global unique constraint on Code in component_types
        // Use PL/pgSQL to dynamically find and drop ANY constraint or index on the 'code' column
        Execute.Sql(@"
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                -- 1. Drop constraints relying on 'code'
                FOR r IN (
                    SELECT conname 
                    FROM pg_constraint 
                    WHERE conrelid = 'component_types'::regclass 
                    AND array_length(conkey, 1) = 1 
                    AND conkey[1] = (SELECT attnum FROM pg_attribute WHERE attrelid = 'component_types'::regclass AND attname = 'code')
                    AND contype = 'u'
                ) LOOP
                    EXECUTE 'ALTER TABLE component_types DROP CONSTRAINT ""' || r.conname || '""';
                END LOOP;

                -- 2. Drop unique indexes involving only 'code' (not already dropped as constraint backing indexes)
                FOR r IN (
                    SELECT indexname 
                    FROM pg_indexes 
                    WHERE tablename = 'component_types' 
                    AND indexdef LIKE '%(code)%' 
                    AND indexdef LIKE '%UNIQUE%'
                ) LOOP
                    EXECUTE 'DROP INDEX IF EXISTS ""' || r.indexname || '""';
                END LOOP;
            END
            $$;
        ");
        
        // 2. Add composite unique constraint (Code + Group)
        Execute.Sql(@"
            DO $$
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname = 'UQ_component_types_code_group') THEN
                    ALTER TABLE component_types ADD CONSTRAINT ""UQ_component_types_code_group"" UNIQUE (code, component_group_id);
                END IF;
            END
            $$;
        ");

        // 3. Seed Data
        Execute.WithConnection((conn, tran) =>
        {
            SeedGroups(conn, tran);
        });
    }

    public override void Down()
    {
        // Remove composite constraint
        Delete.UniqueConstraint("UQ_component_types_code_group").FromTable("component_types");

        // Restore global unique constraint (Note: this will fail if duplicates exist)
        Create.UniqueConstraint("IX_component_types_code").OnTable("component_types").Column("code");
        Create.Index("idx_component_types_code").OnTable("component_types").OnColumn("code");

        // Optional: Delete seeded data. Skipping for safety as it might have user data linked.
    }

    private void SeedGroups(IDbConnection conn, IDbTransaction tran)
    {
        var componentGroups = new List<string>
        {
            "Structural Components",
            "Level Accessories",
            "Rack Accessories",
            "Catwalk Accessories",
            "Material Handling Equipment(MHE)",
            "Stock Keeping Unit(SKU)",
            "Miscellaneous Accessories",
            "Layout Accessories"
        };
        
        // ... remainder of method
        foreach (var groupName in componentGroups)
        {
            var groupId = UpsertGroup(conn, tran, groupName);
            SeedTypes(conn, tran, groupId, groupName);
        }
    }

    private void SeedTypes(IDbConnection conn, IDbTransaction tran, Guid groupId, string groupName)
    {
        if (!ComponentTypes.TryGetValue(groupName, out var types)) return;

        foreach (var typeName in types)
        {
            var typeId = UpsertType(conn, tran, groupId, typeName);
            SeedNames(conn, tran, typeId, typeName);
        }
    }

    private void SeedNames(IDbConnection conn, IDbTransaction tran, Guid typeId, string typeName)
    {
        if (!ModelTypes.TryGetValue(typeName, out var names)) return;

        foreach (var nameName in names)
        {
            UpsertName(conn, tran, typeId, nameName);
        }
    }

    private Guid UpsertGroup(IDbConnection conn, IDbTransaction tran, string name)
    {
        var code = name.ToUpperInvariant().Trim();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = "SELECT id FROM component_groups WHERE UPPER(code) = @Code";
        AddParameter(cmd, "Code", code);

        var existingId = cmd.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value)
        {
            return (Guid)existingId;
        }

        var newId = Guid.NewGuid();
        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = tran;
        insertCmd.CommandText = @"
            INSERT INTO component_groups (id, code, name, description, sort_order, is_active, created_at, updated_at)
            VALUES (@Id, @Code, @Name, '', 0, true, @Now, @Now)";
        AddParameter(insertCmd, "Id", newId);
        AddParameter(insertCmd, "Code", code);
        AddParameter(insertCmd, "Name", name);
        AddParameter(insertCmd, "Now", DateTime.UtcNow);
        insertCmd.ExecuteNonQuery();

        return newId;
    }

    private Guid UpsertType(IDbConnection conn, IDbTransaction tran, Guid groupId, string name)
    {
        var code = name.ToUpperInvariant().Trim();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = "SELECT id FROM component_types WHERE UPPER(code) = @Code AND component_group_id = @GroupId";
        AddParameter(cmd, "Code", code);
        AddParameter(cmd, "GroupId", groupId);

        var existingId = cmd.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value)
        {
            return (Guid)existingId;
        }

        var newId = Guid.NewGuid();
        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = tran;
        insertCmd.CommandText = @"
            INSERT INTO component_types (id, code, name, description, component_group_id, is_active, created_at, updated_at)
            VALUES (@Id, @Code, @Name, '', @GroupId, true, @Now, @Now)";
        AddParameter(insertCmd, "Id", newId);
        AddParameter(insertCmd, "Code", code);
        AddParameter(insertCmd, "Name", name);
        AddParameter(insertCmd, "GroupId", groupId);
        AddParameter(insertCmd, "Now", DateTime.UtcNow);
        insertCmd.ExecuteNonQuery();

        return newId;
    }

    private Guid UpsertName(IDbConnection conn, IDbTransaction tran, Guid typeId, string name)
    {
        var code = name.ToUpperInvariant().Trim();
        using var cmd = conn.CreateCommand();
        cmd.Transaction = tran;
        cmd.CommandText = "SELECT id FROM component_names WHERE UPPER(code) = @Code AND component_type_id = @TypeId";
        AddParameter(cmd, "Code", code);
        AddParameter(cmd, "TypeId", typeId);

        var existingId = cmd.ExecuteScalar();
        if (existingId != null && existingId != DBNull.Value)
        {
            return (Guid)existingId;
        }

        var newId = Guid.NewGuid();
        using var insertCmd = conn.CreateCommand();
        insertCmd.Transaction = tran;
        insertCmd.CommandText = @"
            INSERT INTO component_names (id, code, name, description, component_type_id, is_active, created_at, updated_at)
            VALUES (@Id, @Code, @Name, '', @TypeId, true, @Now, @Now)";
        AddParameter(insertCmd, "Id", newId);
        AddParameter(insertCmd, "Code", code);
        AddParameter(insertCmd, "Name", name);
        AddParameter(insertCmd, "TypeId", typeId);
        AddParameter(insertCmd, "Now", DateTime.UtcNow);
        insertCmd.ExecuteNonQuery();

        return newId;
    }

    private void AddParameter(IDbCommand cmd, string name, object value)
    {
        var p = cmd.CreateParameter();
        p.ParameterName = name;
        p.Value = value;
        cmd.Parameters.Add(p);
    }

    private static readonly Dictionary<string, string[]> ComponentTypes = new()
    {
        { "Structural Components", new[] { "Upright", "Base plate", "Base foot", "Beams", "Bracing", "Diagonal Bracing", "Horizontal Bracing", "Shim", "Splice", "Stiffeners", "Metal Angle Frame" } },
        { "Level Accessories", new[] { "Decking panel", "Panel", "Panel Connector", "Panel Spacer", "Panel Support Bar", "Guided Type Pallet Support Bar", "Pallet Stopper", "Pallet stopper Bracket", "Pallet Stopper End Clamp", "Fork Entry Bar", "Shelf Panel", "Shelf End Stopper", "Shelf Stiffener", "Shelf Clip", "Cross Tie", "Divider", "Inner Cladding G50", "Back Plain Cladding Kit", "Folio Inner Cladding G50", "Mesh Support Bracket", "Partition  cladding", "FBC & RBC Assembly Kit", "EBC Assembly Kit", "FIXED U/C MODULE", "FIXED U/C ASSEMBLY", "MOVABLE U/C ASSEMBLY", "Antitoppling Assembly kit", "Rail Assembly Kit", "Drive Unit Assembly", "Stability Beam", "P&D Station" } },
        { "Rack Accessories", new[] { "Aisle tie", "Back Tie Rod", "Back Tie Rod cleat", "Cladding Clip", "End Cladding G50", "Guide Rail", "Inner Cladding G50", "Mesh Cladding", "Nylon Netting", "Plain Cladding Bottom Piece", "Plain cladding top piece", "Plain Cladding Middle Piece", "Rear  Frame Cladding", "Rear Cladding G50", "Rear mesh Cladding G50", "Row Connector", "Row guard", "Signages", "Tie Beam", "Tie Rod", "Upright Guard", "Turn Buckle" } },
        { "Catwalk Accessories", new[] { "Railing Pipe and accessories", "Clamp", "BEAM", "Channel", "Walkway Panel", "Gap panel", "Step", "Cross Tie", "Kick plate", "Beading", "Chequered plate", "GATE ASSY", "Bracket", "SWG ARM Assembly", "Side Support Assembly", "VRC Cross aisle beams", "End Joining Piece", "Staircase Angle", "Pillow Block", "SWG STIFFR" } },
        { "Material Handling Equipment(MHE)", new[] { "Stacker", "Forklift", "Articulated forklift", "Reach truck", "HPT", "Trolley" } },
        { "Stock Keeping Unit(SKU)", new[] { "Pallets", "Bins", "Crates", "Gunny bags", "Loose items", "Spray Can" } },
        { "Layout Accessories", new[] { "Aisle Ties", "Sliding gate", "Swing gate", "VRC", "Spiral chute", "Gravity chute", "Conveyors", "Vertiflow", "Staircase", "MHE Stopper", "Bracing tower" } },
        { "Miscellaneous Accessories", new[] { "Aisle Placard", "Anchor Bolt", "Bearing", "Bolts", "Bush", "Capsules", "Clamp", "Fastener", "General accessories kit", "Hanging Chain", "Label Holder", "Nuts", "Pins", "Pin Assembly", "Post", "Screw", "Signal Box", "Spring", "Sticker", "SWG STIFFR", "Uâ€‘Shaped Bolt", "Washer", "Tape" } }
    };

    private static readonly Dictionary<string, string[]> ModelTypes = new()
    {
        { "Upright", new[] { "UPRIGHT G", "UPRIGHT GX", "UPRIGHT GXL", "UPRIGHT CAP GX", "Upright cover G50", " Upright Connector G50" } },
        { "Beams", new[] { "BEAM GBX", "BEAM GBHX", "BEAM HEM+", "BEAM GSB", "Beam locking pin" } },
        { "Horizontal Bracing", new[] { "Horizontal Bracing - G50", "Horizontal Bracing - GL", "Horizontal Bracing - GX" } },
        { "Diagonal Bracing", new[] { "Diagonal Bracing-GL", "Diagonal Bracing-GX" } },
        { "Decking panel", new[] { "HD 6B RF PANEL(GSB)", "HD 6B FLAT END PANEL", "RF 4B PLAIN PANEL", "4B PANEL" } },
        { "Panel support Bar", new[] { "Chnl 4B PSB", "Welded Type PSB" } },
        { "Tie Beam", new[] { "CA Tie Beam Box", "CA Tie Beam HEM+", "CA Tie Beam GBHX", "Hook CA Tie Beam Box", "Hook CA Tie Beam HEM+", "Hook CA Tie Beam GBHX", "Hook PA Tie Beam HEM+", "Hook PA Tie Beam GBHX", "Hook PA Tie Beam I-SEC", "PA Tie Beam HEM+ ", "PA Tie Beam GBHX", "PA Tie Beam I-SEC", "Tie Beam GXL", "Tie Beam HEM+", "Tie Beam Support Bracket -GX90", "Tie Beam Support Bracket -GX110", "Tie Beam Support Bracket -GX120" } },
        { "Base plate", new[] { "Base plate T3", "Base plate SF G50" } },
        { "Base foot", new[] { "BASE FOOT " } },
        { "Bracing", new[] { "Bracing G50", "Bracing GX", "Bracing Spacer", "Bracing Spacer-GA", "PP BRACING SPACER GX70", "Bracing Cleat - GX70", "Bracing Cleat - GX90", "Bracing Cleat - GX110", "Bracing Cleat - GX120" } },
        { "Shim", new[] { "Shim GX" } },
        { "Stiffeners", new[] { "Stiffner GX", "Bottom Stiffner GX 70", "Stiffner GX 70" } },
        { "Splice", new[] { "Splice Joint Front GX70", "Splice Joint Rear  GX70", "Splice Joint Front GX90", "Splice Joint Front GX110", "Splice Joint Front GX120", "Splice Joint Rear  GX/GXL", "Splice Joint Rear  GX/GXXL" } },
        { "Panel", new[] { "4 Bend Panel", "Folio Top Panel End", "Folio Top Panel Mid", "Top Panel Joining Piece-Mid", "Top Panel Joining Piece-End", "Top Panel End", "Top Panel End Light", "Top Panel Mid", "Top Panel Mid Light" } },
        { "Pallet Stopper", new[] { "Pallet stopper Pipe Rectangle", "Pallet stopper joining Piece Rectangle", "Pallet stopper Bracket Rectangle", "Pallet stopper Bracket Rectangle Non Std", "Pallet stopper Pipe Cap", "Channel Pallet Stopper" } },
        { "Spray Can", new[] { "Spray Can - Sky Blue", "Spray Can - Light Grey", "Spray Can - Oxford Blue", "Spray Can - Orange", "Spray Can - Yellow", "Spray Can - Oxford Blue", "Spray Can - Light Grey RAL7035" } },
        { "Nuts", new[] { "Hex. Nuts M10", "Hex Nut M8 Zinc Blue", "Hex Nut M12 1.75mm MS IS 1364", "Hex Nut M6 Zinc Blue", "Hex. Nuts M6", "Hex. Nuts M5", "Hex. Nuts M4", "Serrated Nut M8 Zinc Blue", "Serrated Nut M10 Zinc Blue", "Hex. Cap Nuts M8" } }
    };
}
