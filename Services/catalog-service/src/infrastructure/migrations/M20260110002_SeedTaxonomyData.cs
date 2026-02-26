using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260110002)]
public class SeedTaxonomyData : Migration
{
    public override void Up()
    {
        // Seeding disabled as per user request
    }

    private void SeedCategories()
    {
        var now = DateTime.UtcNow;

        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111001"), code = "FRAMES", name = "Frames & Uprights", description = "Vertical load-bearing structure components", sort_order = 1, is_active = true, created_at = now });
        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111002"), code = "HORIZONTAL", name = "Horizontal Members", description = "Pallet support and decking components", sort_order = 2, is_active = true, created_at = now });
        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111003"), code = "STABILITY", name = "Stability & Bracing", description = "Structural bracing and stability components", sort_order = 3, is_active = true, created_at = now });
        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111004"), code = "FOUNDATION", name = "Foundation & Anchoring", description = "Floor connection and anchoring components", sort_order = 4, is_active = true, created_at = now });
        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111005"), code = "SAFETY", name = "Safety & Protection", description = "Safety guards and protection components", sort_order = 5, is_active = true, created_at = now });
        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111006"), code = "CANTILEVER", name = "Cantilever Components", description = "Long goods storage components", sort_order = 6, is_active = true, created_at = now });
        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111007"), code = "ASRS", name = "ASRS Components", description = "Automated storage and retrieval components", sort_order = 7, is_active = true, created_at = now });
        Insert.IntoTable("component_categories").Row(new { id = Guid.Parse("11111111-1111-1111-1111-111111111008"), code = "ACCESSORIES", name = "Accessories", description = "Additional accessories and add-ons", sort_order = 8, is_active = true, created_at = now });
    }

    private void SeedComponentTypes()
    {
        var now = DateTime.UtcNow;

        // FRAMES category
        var framesId = Guid.Parse("11111111-1111-1111-1111-111111111001");
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222001"), code = "UPRIGHT", name = "Upright", description = "Vertical column, carries all vertical loads", category_id = framesId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222002"), code = "UPRIGHT_EXTENSION", name = "Upright Extension", description = "Splice to extend upright height", category_id = framesId, is_active = true, created_at = now });

        // HORIZONTAL category
        var horizontalId = Guid.Parse("11111111-1111-1111-1111-111111111002");
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222003"), code = "BEAM", name = "Step Beam", description = "Primary horizontal member, supports pallets", category_id = horizontalId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222004"), code = "BEAM_CONNECTOR", name = "Beam Connector", description = "Hook/clip that attaches beam to upright", category_id = horizontalId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222005"), code = "LOCK_PIN", name = "Safety Lock Pin", description = "Prevents accidental beam dislodgement", category_id = horizontalId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222006"), code = "PANEL", name = "Panel/Decking", description = "Solid surface for non-pallet loads", category_id = horizontalId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222007"), code = "WIRE_DECK", name = "Wire Decking", description = "Mesh surface for sprinkler compliance", category_id = horizontalId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222008"), code = "PSB", name = "Pallet Support Bar", description = "Additional support for single-entry pallets", category_id = horizontalId, is_active = true, created_at = now });

        // STABILITY category
        var stabilityId = Guid.Parse("11111111-1111-1111-1111-111111111003");
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222009"), code = "FRAME_BRACING_X", name = "X-Bracing (Frame)", description = "Diagonal cross-bracing within frame", category_id = stabilityId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222010"), code = "FRAME_BRACING_H", name = "Horizontal Bracing", description = "Horizontal strut within frame", category_id = stabilityId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222011"), code = "ROW_SPACER", name = "Row Spacer", description = "Back-to-back row connection", category_id = stabilityId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222012"), code = "SPINE_BRACING", name = "Spine Bracing", description = "Longitudinal bracing at row ends", category_id = stabilityId, is_active = true, created_at = now });

        // FOUNDATION category
        var foundationId = Guid.Parse("11111111-1111-1111-1111-111111111004");
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222013"), code = "BASE_PLATE", name = "Base Plate", description = "Distributes load to slab", category_id = foundationId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222014"), code = "BASE_PLATE_HD", name = "Heavy-Duty Base Plate", description = "Heavy-duty base plate for seismic zones", category_id = foundationId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222015"), code = "ANCHOR_BOLT_M12", name = "Anchor Bolt M12", description = "M12 anchor bolt for standard applications", category_id = foundationId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222016"), code = "ANCHOR_BOLT_M16", name = "Anchor Bolt M16", description = "M16 anchor bolt for heavy-duty/seismic", category_id = foundationId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222017"), code = "SHIM", name = "Leveling Shim", description = "Compensates for floor unevenness", category_id = foundationId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222018"), code = "GROUT", name = "Grout Pack", description = "Fill under base plate for load distribution", category_id = foundationId, is_active = true, created_at = now });

        // SAFETY category
        var safetyId = Guid.Parse("11111111-1111-1111-1111-111111111005");
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222019"), code = "COLUMN_PROTECTOR", name = "Column Protector", description = "Yellow guard at upright base", category_id = safetyId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222020"), code = "ROW_END_GUARD", name = "Row End Guard", description = "Barrier at aisle end of row", category_id = safetyId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222021"), code = "FRAME_PROTECTOR", name = "Frame Protector", description = "Full frame protection barrier", category_id = safetyId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222022"), code = "SAFETY_NETTING", name = "Safety Netting", description = "Fall protection between racks", category_id = safetyId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222023"), code = "LOAD_NOTICE", name = "Load Notice", description = "Capacity signage", category_id = safetyId, is_active = true, created_at = now });

        // CANTILEVER category
        var cantileverId = Guid.Parse("11111111-1111-1111-1111-111111111006");
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222024"), code = "CANT_COLUMN", name = "Cantilever Column", description = "Vertical member with arm slots", category_id = cantileverId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222025"), code = "CANT_ARM", name = "Cantilever Arm", description = "Horizontal load-bearing arm", category_id = cantileverId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222026"), code = "CANT_BRACE", name = "Cantilever Brace", description = "Diagonal/horizontal stability", category_id = cantileverId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222027"), code = "CANT_BASE", name = "Cantilever Base", description = "Foundation for column", category_id = cantileverId, is_active = true, created_at = now });

        // ASRS category
        var asrsId = Guid.Parse("11111111-1111-1111-1111-111111111007");
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222028"), code = "RAIL", name = "Crane Rail", description = "Stacker crane running surface", category_id = asrsId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222029"), code = "RAIL_SUPPORT", name = "Rail Support Beam", description = "Carries rail load", category_id = asrsId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222030"), code = "SHUTTLE_TRACK", name = "Shuttle Track", description = "Pallet shuttle running surface", category_id = asrsId, is_active = true, created_at = now });
        Insert.IntoTable("component_types").Row(new { id = Guid.Parse("22222222-2222-2222-2222-222222222031"), code = "BIN_RAIL", name = "Bin Rail", description = "Mini-load tote support", category_id = asrsId, is_active = true, created_at = now });
    }

    private void SeedProductGroups()
    {
        var now = DateTime.UtcNow;

        // Main product groups
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333001"), code = "SPR", name = "Selective Pallet Racking", description = "Standard selective pallet racking system", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333002"), code = "DOUBLE_DEEP", name = "Double Deep Racking", description = "Double deep pallet racking system", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333003"), code = "DRIVE_IN", name = "Drive-In Racking", description = "Drive-in/drive-through racking system", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333004"), code = "PUSH_BACK", name = "Push Back Racking", description = "Push back pallet racking system", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333005"), code = "CANTILEVER", name = "Cantilever Racking", description = "Long goods cantilever storage system", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333006"), code = "ASRS", name = "Automated Storage", description = "Automated storage and retrieval system", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333007"), code = "MOBILE", name = "Mobile Racking", description = "Mobile/compact racking system", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333008"), code = "SHELVING", name = "Shelving Systems", description = "Industrial shelving systems", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333009"), code = "MEZZANINE", name = "Mezzanine Floors", description = "Mezzanine floor systems", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333010"), code = "CARTON_FLOW", name = "Carton Flow Racking", description = "Carton flow gravity racking", is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333011"), code = "PALLET_FLOW", name = "Pallet Flow Racking", description = "Pallet flow gravity racking", is_active = true, created_at = now });

        // SPR variants
        var sprId = Guid.Parse("33333333-3333-3333-3333-333333333001");
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333101"), code = "SPR_WIDE", name = "SPR Wide Span", description = "Wide span selective pallet racking", parent_group_id = sprId, is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333102"), code = "SPR_VNA", name = "SPR Very Narrow Aisle", description = "Very narrow aisle pallet racking", parent_group_id = sprId, is_active = true, created_at = now });

        // Cantilever variants
        var cantileverId = Guid.Parse("33333333-3333-3333-3333-333333333005");
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333501"), code = "CANT_SINGLE", name = "Single-Sided Cantilever", description = "Single-sided cantilever system", parent_group_id = cantileverId, is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333502"), code = "CANT_DOUBLE", name = "Double-Sided Cantilever", description = "Double-sided cantilever system", parent_group_id = cantileverId, is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333503"), code = "CANT_HD", name = "Heavy-Duty Cantilever", description = "Heavy-duty cantilever for large loads", parent_group_id = cantileverId, is_active = true, created_at = now });

        // ASRS variants
        var asrsId = Guid.Parse("33333333-3333-3333-3333-333333333006");
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333601"), code = "ASRS_MINILOAD", name = "Mini-Load ASRS", description = "Mini-load automated storage", parent_group_id = asrsId, is_active = true, created_at = now });
        Insert.IntoTable("product_groups").Row(new { id = Guid.Parse("33333333-3333-3333-3333-333333333602"), code = "ASRS_UNIT", name = "Unit-Load ASRS", description = "Unit-load pallet ASRS system", parent_group_id = asrsId, is_active = true, created_at = now });
    }

    public override void Down()
    {
        Delete.FromTable("product_groups").AllRows();
        Delete.FromTable("component_types").AllRows();
        Delete.FromTable("component_categories").AllRows();
    }
}
