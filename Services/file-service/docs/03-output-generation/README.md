# Output Generation (Excel & PDF)

## Purpose
To generate downloadable reports and documents for users based on system data.

## Supported Formats

### Excel (.xlsx)
- Used for: BOM (Bill of Materials), Pricing Lists, Detailed Data Exports.
- library: `EPPlus` or similar.
- Features:
    - Multi-sheet support.
    - Formatting (headers, bold text, currency).
    - Formulas (if needed).

### PDF (.pdf)
- Used for: Quotations, Layout Summaries, Printable Reports.
- Library: `QuestPDF`, `iTextSharp`, or similar.
- Features:
    - Image embedding (charts, layout snapshots).
    - Table layouts.
    - Branding (logos, headers/footers).
