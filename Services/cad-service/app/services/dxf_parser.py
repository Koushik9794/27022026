import os
import tempfile
import ezdxf
from typing import Optional, List, Dict, Any, Union, Tuple  # Add this import line
from ezdxf.document import Drawing
from ezdxf.math import BoundingBox, Vec3
from ezdxf.entities import DXFEntity
try:
    import ezdxf.bbox as bbox_calc
except ImportError:
    bbox_calc = None
from ..models.dxf_models import (
    DXFParseResponse, 
    FileMetadata, 
    LayerProperties, 
    EntityInfo, 
    ExtentsInfo, 
    BlockInfo, 
    LinetypeInfo
)

# DXF Version mapping
DXF_VERSIONS = {
    "AC1009": "AutoCAD R12",
    "AC1012": "AutoCAD R13",
    "AC1014": "AutoCAD R14",
    "AC1015": "AutoCAD 2000",
    "AC1018": "AutoCAD 2004",
    "AC1021": "AutoCAD 2007",
    "AC1024": "AutoCAD 2010",
    "AC1027": "AutoCAD 2013",
    "AC1032": "AutoCAD 2018",
}

# Standard ACI Colors to Hex (Partial mapping for 1-9)
ACI_COLORS = {
    1: "#FF0000", # Red
    2: "#FFFF00", # Yellow
    3: "#00FF00", # Green
    4: "#00FFFF", # Cyan
    5: "#0000FF", # Blue
    6: "#FF00FF", # Magenta
    7: "#FFFFFF", # White/Black
    8: "#808080", # Dark Gray
    9: "#C0C0C0", # Light Gray
}

def get_color_hex(aci: int) -> str:
    """Convert ACI color index to hex string"""
    if aci in ACI_COLORS:
        return ACI_COLORS[aci]
    return "#FFFFFF" # Default white


def get_point_dict(point) -> dict:
    """Safely convert various point types (Vec3, tuple, numpy array) to dict"""
    try:
        if hasattr(point, 'x') and hasattr(point, 'y'):
            return {"x": float(point.x), "y": float(point.y)}
        # Handle indexable types (tuple, list, numpy array)
        if hasattr(point, '__getitem__'):
            return {"x": float(point[0]), "y": float(point[1])}
    except:
        pass
    return {"x": 0.0, "y": 0.0}


def get_entity_bbox(entity: DXFEntity) -> dict | None:
    """Calculate precise bounding box for a single entity"""
    try:
        if bbox_calc:
            ext = bbox_calc.extents([entity])
            if ext.has_data:
                return {
                    "min_x": float(ext.extmin.x),
                    "min_y": float(ext.extmin.y),
                    "max_x": float(ext.extmax.x),
                    "max_y": float(ext.extmax.y)
                }
        
        # Fallback to custom calculation for basic types if ezdxf.bbox fails
        # This is a bit simplified but works for basic primitives
        if entity.dxftype() == "LINE":
            s, e = entity.dxf.start, entity.dxf.end
            return {
                "min_x": float(min(s.x, e.x)), "min_y": float(min(s.y, e.y)),
                "max_x": float(max(s.x, e.x)), "max_y": float(max(s.y, e.y))
            }
        elif entity.dxftype() == "CIRCLE":
            c, r = entity.dxf.center, entity.dxf.radius
            return {
                "min_x": float(c.x - r), "min_y": float(c.y - r),
                "max_x": float(c.x + r), "max_y": float(c.y + r)
            }
    except:
        pass
    return None


def extract_entity_geometry(entity: DXFEntity) -> dict:
    """Extract geometry data from entity for frontend rendering"""
    geometry = {
        "type": entity.dxftype(),
        "lineweight": getattr(entity.dxf, 'lineweight', -1),
        "linetype": getattr(entity.dxf, 'linetype', 'BYLAYER')
    }
    
    try:
        if entity.dxftype() == "LINE":
            geometry["start"] = get_point_dict(entity.dxf.start)
            geometry["end"] = get_point_dict(entity.dxf.end)
            
        elif entity.dxftype() == "CIRCLE":
            geometry["center"] = get_point_dict(entity.dxf.center)
            geometry["radius"] = entity.dxf.radius
            
        elif entity.dxftype() == "ARC":
            geometry["center"] = get_point_dict(entity.dxf.center)
            geometry["radius"] = entity.dxf.radius
            geometry["start_angle"] = entity.dxf.start_angle
            geometry["end_angle"] = entity.dxf.end_angle
            
        elif entity.dxftype() == "POINT":
            geometry["location"] = get_point_dict(entity.dxf.location)
            
        elif entity.dxftype() == "LWPOLYLINE":
            points = []
            for point in entity.get_points():
                # get_points for LWPOLYLINE returns (x, y, start_width, end_width, bulge)
                points.append({"x": float(point[0]), "y": float(point[1]), "bulge": float(point[4]) if len(point) > 4 else 0.0})
            geometry["points"] = points
            geometry["closed"] = entity.closed
            
        elif entity.dxftype() == "POLYLINE":
            points = []
            for vertex in entity.vertices:
                points.append(get_point_dict(vertex.dxf.location))
            geometry["points"] = points
            geometry["closed"] = entity.is_closed
            
        elif entity.dxftype() == "SPLINE":
            control_points = []
            if hasattr(entity, 'control_points'):
                for point in entity.control_points:
                    control_points.append(get_point_dict(point))
            geometry["control_points"] = control_points
            geometry["degree"] = getattr(entity.dxf, 'degree', 3)
            
        elif entity.dxftype() == "ELLIPSE":
            geometry["center"] = get_point_dict(entity.dxf.center)
            geometry["major_axis"] = get_point_dict(entity.dxf.major_axis)
            geometry["ratio"] = entity.dxf.ratio
            geometry["start_param"] = entity.dxf.start_param
            geometry["end_param"] = entity.dxf.end_param
            
        elif entity.dxftype() == "TEXT":
            geometry["insert"] = get_point_dict(entity.dxf.insert)
            geometry["text"] = entity.dxf.text
            geometry["height"] = entity.dxf.height
            geometry["rotation"] = getattr(entity.dxf, 'rotation', 0)
            
        elif entity.dxftype() == "MTEXT":
            geometry["insert"] = get_point_dict(entity.dxf.insert)
            geometry["text"] = entity.text
            geometry["char_height"] = entity.dxf.char_height
            
        elif entity.dxftype() == "INSERT":
            geometry["insert"] = get_point_dict(entity.dxf.insert)
            geometry["block_name"] = entity.dxf.name
            geometry["xscale"] = getattr(entity.dxf, 'xscale', 1)
            geometry["yscale"] = getattr(entity.dxf, 'yscale', 1)
            geometry["rotation"] = getattr(entity.dxf, 'rotation', 0)
            
        elif entity.dxftype() == "HATCH":
            geometry["pattern_name"] = getattr(entity.dxf, 'pattern_name', 'SOLID')
            paths = []
            try:
                for path in entity.paths:
                    if hasattr(path, 'vertices'):
                        path_points = [get_point_dict(v) for v in path.vertices]
                        paths.append({"type": "polyline", "points": path_points})
                    elif hasattr(path, 'edges'):
                        # Handle edge paths
                        edge_points = []
                        for edge in path.edges:
                            if hasattr(edge, 'start'):
                                edge_points.append(get_point_dict(edge.start))
                            if hasattr(edge, 'end'):
                                edge_points.append(get_point_dict(edge.end))
                        if edge_points:
                            paths.append({"type": "edges", "points": edge_points})
            except:
                pass
            geometry["paths"] = paths
            
        elif entity.dxftype() == "DIMENSION":
            # Extract dimension line geometry
            geometry["defpoint"] = get_point_dict(entity.dxf.defpoint)
            if hasattr(entity.dxf, 'defpoint2'):
                geometry["defpoint2"] = get_point_dict(entity.dxf.defpoint2)
            if hasattr(entity.dxf, 'defpoint3'):
                geometry["defpoint3"] = get_point_dict(entity.dxf.defpoint3)
            if hasattr(entity.dxf, 'text_midpoint'):
                geometry["text_midpoint"] = get_point_dict(entity.dxf.text_midpoint)
            geometry["text"] = entity.dxf.text
            
        elif entity.dxftype() == "LEADER":
            geometry["vertices"] = [get_point_dict(v) for v in entity.vertices]
            geometry["has_arrowhead"] = getattr(entity.dxf, 'has_arrowhead', True)

        elif entity.dxftype() in ("SOLID", "3DFACE", "TRACE"):
            points = []
            for i in range(4):
                attr = f"vtx{i}"
                if hasattr(entity.dxf, attr):
                    points.append(get_point_dict(getattr(entity.dxf, attr)))
            geometry["points"] = points

    except Exception as e:
        print(f"Error extracting geometry for {entity.dxftype()}: {e}")
        
    return geometry


def format_file_size(size_bytes):
    """Format file size in human readable format"""
    if size_bytes == 0:
        return "0B"
    size_name = ("B", "KB", "MB", "GB", "TB")
    import math
    i = int(math.floor(math.log(size_bytes, 1024)))
    p = math.pow(1024, i)
    s = round(size_bytes / p, 2)
    return "%s %s" % (s, size_name[i])


async def parse_dxf_file(file_content: bytes, filename: str) -> DXFParseResponse:
    """Parse DXF file content and return metadata and entities"""
    
    # Ensure content is not empty
    if not file_content or len(file_content) == 0:
        print(f"ERROR: Received empty file content for {filename}")
        raise ValueError("File content is empty")

    # Use a temporary file to save content since ezdxf works better with file paths
    # Using a more robust flush/close pattern
    fd, tmp_path = tempfile.mkstemp(suffix=".dxf")
    try:
        with os.fdopen(fd, 'wb') as tmp:
            tmp.write(file_content)
            tmp.flush()
            os.fsync(tmp.fileno())
        
        doc = ezdxf.readfile(tmp_path)
        header = doc.header
        file_size = os.path.getsize(tmp_path)
        dxf_version = header.get('$ACADVER', 'Unknown')
        
        print(f"Successfully loaded DXF: {filename}, Size: {file_size}, Version: {dxf_version}")
        
        metadata = FileMetadata(
            filename=filename,
            dxf_version=dxf_version,
            dxf_version_name=DXF_VERSIONS.get(dxf_version, "Unknown Version"),
            created_by=header.get('$LASTSAVEDBY', None),
            file_size=file_size,
            file_size_formatted=format_file_size(file_size),
            units=str(header.get('$INSUNITS', 0)),
        )
        
        # 1. Collect entities and count per layer (including virtual ones)
        entities: list[EntityInfo] = []
        entity_types: dict[str, int] = {}
        layer_entity_counts: dict[str, int] = {}
        MAX_ENTITIES = 200000 # Safety limit for frontend performance
        
        def add_entity(entity, is_virtual=False):
            if len(entities) >= MAX_ENTITIES:
                return False
                
            try:
                etype = entity.dxftype()
                entity_types[etype] = entity_types.get(etype, 0) + 1
                
                # Get entity specific properties
                lineweight = getattr(entity.dxf, 'lineweight', -1)
                linetype = getattr(entity.dxf, 'linetype', 'BYLAYER')
                
                # Virtual entities don't have handles
                handle = getattr(entity.dxf, 'handle', None)
                
                # Track layer entity counts
                layer_name = entity.dxf.layer
                layer_entity_counts[layer_name] = layer_entity_counts.get(layer_name, 0) + 1
                
                # Dimension specific metadata
                dim_text = None
                dim_type = None
                if etype == "DIMENSION":
                    dim_text = getattr(entity.dxf, 'text', '')
                    dim_type = str(entity.dimtype)
                
                # Geometry extraction is the most expensive part
                geom = extract_entity_geometry(entity)
                if not geom:
                    return True
                    
                entities.append(EntityInfo(
                    entity_type=etype,
                    layer=layer_name,
                    color=getattr(entity.dxf, 'color', 256),
                    handle=handle,
                    lineweight=lineweight,
                    linetype=linetype,
                    is_virtual=is_virtual,
                    geometry=geom,
                    bbox=get_entity_bbox(entity),
                    dimension_text=dim_text,
                    dimension_type=dim_type
                ))
                return True
            except Exception as e:
                if not is_virtual:
                    print(f"Error parsing entity {entity.dxftype()}: {e}")
                return True

        def collect_entities(entity_list, depth=0, max_depth=3, is_virtual=False):
            if len(entities) >= MAX_ENTITIES:
                return
                
            for entity in entity_list:
                if len(entities) >= MAX_ENTITIES:
                    break
                
                etype = entity.dxftype()
                    
                if etype == "INSERT":
                    if depth < max_depth:
                        try:
                            # Explode block references
                            collect_entities(entity.virtual_entities(), depth + 1, max_depth, is_virtual=True)
                        except:
                            add_entity(entity, is_virtual=is_virtual)
                    else:
                        # Too deep, just add the insert point
                        add_entity(entity, is_virtual=is_virtual)
                elif etype in ("DIMENSION", "LEADER"):
                    try:
                        # Replicate exact CAD layout by exploding dimensions and leaders
                        # This gives us the lines, text and arrows exactly as rendered by the CAD engine
                        collect_entities(entity.virtual_entities(), depth + 1, max_depth, is_virtual=True)
                        # We still add the original entity (marked as virtual/ignored in rendering if needed)
                        # for metadata purposes if desired, but here we focus on visual replication
                    except:
                        add_entity(entity, is_virtual=is_virtual)
                else:
                    add_entity(entity, is_virtual=is_virtual)

        # Process modelspace
        msp = doc.modelspace()
        collect_entities(msp)
        
        # 2. Extract layers using accurate counts
        layers: list[LayerProperties] = []
        for layer in doc.layers:
            try:
                rgb_color = None
                if hasattr(layer, 'rgb') and layer.rgb is not None:
                    rgb = layer.rgb
                    rgb_color = f"#{rgb[0]:02x}{rgb[1]:02x}{rgb[2]:02x}"
                elif layer.dxf.color in ACI_COLORS:
                    rgb_color = ACI_COLORS[layer.dxf.color]
                else:
                    rgb_color = get_color_hex(layer.dxf.color)
                
                transparency = 0.0
                if hasattr(layer.dxf, 'transparency'):
                    trans_value = layer.dxf.transparency
                    if trans_value is not None and trans_value > 0:
                        transparency = (trans_value & 0xFF) / 255.0
                
                layer_props = LayerProperties(
                    name=layer.dxf.name,
                    color=layer.dxf.color,
                    rgb_color=rgb_color,
                    true_color=getattr(layer.dxf, 'true_color', None),
                    linetype=layer.dxf.linetype,
                    lineweight=layer.dxf.lineweight,
                    plot=getattr(layer.dxf, 'plot', True),
                    is_frozen=layer.is_frozen(),
                    is_locked=layer.is_locked(),
                    is_off=layer.is_off(),
                    is_on=layer.is_on(),
                    transparency=transparency,
                    description=getattr(layer.dxf, 'description', None),
                    material=getattr(layer.dxf, 'material_handle', None),
                    plot_style_name=getattr(layer.dxf, 'plotstyle_name', None),
                    entity_count=layer_entity_counts.get(layer.dxf.name, 0),
                )
                layers.append(layer_props)
            except Exception as e:
                print(f"Error processing layer {layer.dxf.name}: {e}")
                continue
        
        # Calculate extents from all collected entities for maximum reliability
        min_x, min_y = float('inf'), float('inf')
        max_x, max_y = float('-inf'), float('-inf')
        
        has_entities = False
        all_points = []
        
        for ent in entities:
            geom = ent.geometry
            pts = []
            if "points" in geom:
                pts.extend(geom["points"])
            if "start" in geom:
                pts.append(geom["start"])
            if "end" in geom:
                pts.append(geom["end"])
            if "center" in geom:
                c = geom["center"]
                r = geom.get("radius", 0)
                pts.append({"x": c["x"] - r, "y": c["y"] - r})
                pts.append({"x": c["x"] + r, "y": c["y"] + r})
            if "insert" in geom:
                pts.append(geom["insert"])
            if "location" in geom:
                pts.append(geom["location"])
            if "vertices" in geom:
                pts.extend(geom["vertices"])
            if "control_points" in geom:
                pts.extend(geom["control_points"])
                
            for p in pts:
                if p:
                    all_points.append(p)

        if all_points:
            has_entities = True
            # Smart Outlier Detection for High-Fidelity Layouts:
            # We calculate raw extents first
            raw_min_x = min(p["x"] for p in all_points)
            raw_max_x = max(p["x"] for p in all_points)
            raw_min_y = min(p["y"] for p in all_points)
            raw_max_y = max(p["y"] for p in all_points)
            
            # Simple heuristic: If the spread is massive, filter by median
            # Massive means > 100,000 units (common in civil drawings for garbage markers)
            if (raw_max_x - raw_min_x > 100000) or (raw_max_y - raw_min_y > 100000):
                import statistics
                mid_x = statistics.median(p["x"] for p in all_points)
                mid_y = statistics.median(p["y"] for p in all_points)
                
                # Filter points within 50,000 units of median (covers most civil layouts)
                filtered_points = [p for p in all_points if abs(p["x"] - mid_x) < 50000 and abs(p["y"] - mid_y) < 50000]
                
                if filtered_points:
                    min_x = min(p["x"] for p in filtered_points)
                    min_y = min(p["y"] for p in filtered_points)
                    max_x = max(p["x"] for p in filtered_points)
                    max_y = max(p["y"] for p in filtered_points)
                else:
                    min_x, min_y, max_x, max_y = raw_min_x, raw_min_y, raw_max_x, raw_max_y
            else:
                min_x, min_y, max_x, max_y = raw_min_x, raw_min_y, raw_max_x, raw_max_y

        if has_entities:
            width = max(1.0, max_x - min_x)
            height = max(1.0, max_y - min_y)
            extents = ExtentsInfo(
                min_x=float(min_x),
                min_y=float(min_y),
                max_x=float(max_x),
                max_y=float(max_y),
                width=float(width),
                height=float(height),
            )
        else:
            # Fallback if no geometry found
            extents = ExtentsInfo(min_x=0, min_y=0, max_x=100, max_y=100, width=100, height=100)
            
        # Block information
        blocks_info = []
        for block in doc.blocks:
            is_layout = getattr(block, 'is_layout_block', False)
            is_paper = getattr(block, 'is_paper_space', False)
            if is_layout or is_paper or block.name.upper().startswith('*PAPER_SPACE') or block.name.upper().startswith('*MODEL_SPACE'):
                continue
                
            block_entities = []
            for entity in block:
                try:
                    block_entities.append(EntityInfo(
                        entity_type=entity.dxftype(),
                        layer=entity.dxf.layer,
                        color=getattr(entity.dxf, 'color', 256),
                        handle=getattr(entity.dxf, 'handle', None),
                        geometry=extract_entity_geometry(entity),
                        is_virtual=True
                    ))
                except:
                    continue

            blocks_info.append(BlockInfo(
                name=block.name,
                base_point={"x": block.base_point[0], "y": block.base_point[1]},
                entity_count=len(block_entities),
                is_anonymous=getattr(block, 'is_anonymous', False) or getattr(getattr(block, 'block', None), 'is_anonymous', False),
                is_xref=getattr(block, 'is_xref', False) or getattr(getattr(block, 'block', None), 'is_xref', False),
                entities=block_entities
            ))
            
        linetypes_info = [
            LinetypeInfo(
                name=lt.dxf.name, 
                description=getattr(lt.dxf, 'description', None),
                pattern_length=getattr(lt.dxf, 'pattern_len', 0.0)
            ) for lt in doc.linetypes
        ]

        return DXFParseResponse(
            metadata=metadata,
            extents=extents,
            total_layers=len(layers),
            layers=layers,
            total_entities=len(entities),
            entity_types=entity_types,
            entities=entities,
            total_blocks=len(blocks_info),
            blocks=blocks_info,
            linetypes=linetypes_info
        )

    except Exception as e:
        import traceback
        error_msg = f"Error parsing DXF file: {str(e)}"
        print(f"Parse Error: {error_msg}")
        traceback.print_exc()
        
        return DXFParseResponse(
            success=False,
            message=error_msg,
            metadata=FileMetadata(
                filename=filename,
                dxf_version="Unknown",
                dxf_version_name="Error",
                file_size=0,
                file_size_formatted="0B"
            ),
            total_layers=0,
            layers=[],
            total_entities=0,
            entities=[],
            entity_types={},
            total_blocks=0,
            blocks=[],
            linetypes=[]
        )
    finally:
        if os.path.exists(tmp_path):
            os.remove(tmp_path)
