# Glow Up Morning - clothes fitted to the Blender mannequin (Mannequin_Girl)
# Open MannequinFromBlender.blend first, then: Scripting tab > Open this file > Run Script.
# Adds three objects at the same origin as the mannequin and saves the .blend:
#   Worn_Top   - pastel pink T-shirt with short sleeves
#   Worn_Skirt - lilac A-line skirt
#   Worn_Shoes - white shoes
# The numbers below are the same body measurements used to build the mannequin.

import bpy
import bmesh
import math
from mathutils import Vector, Matrix

LIFT = Matrix.Translation((0, 0, 0.03))   # the body stands on a 3 cm display base
T, R = Matrix.Translation, Matrix.Rotation

for n in ["Worn_Top", "Worn_Skirt", "Worn_Shoes"]:
    old = bpy.data.objects.get(n)
    if old:
        bpy.data.objects.remove(old, do_unlink=True)

girl = bpy.data.objects.get("Mannequin_Girl")
col = girl.users_collection[0] if girl else bpy.context.scene.collection


def material(name, rgb, rough=0.7):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    m.use_nodes = True
    bsdf = m.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
    bsdf.inputs["Roughness"].default_value = rough
    m.diffuse_color = (*rgb, 1.0)
    return m


def loft(bm, profile, matrix, seg=20):
    """Open tube through rings (z, rx, ry), placed with 'matrix'."""
    rings = []
    for z, rx, ry in profile:
        rings.append([bm.verts.new((rx * math.cos(2 * math.pi * j / seg),
                                    ry * math.sin(2 * math.pi * j / seg), z)) for j in range(seg)])
    for a, b in zip(rings, rings[1:]):
        for j in range(seg):
            f = bm.faces.new((a[j], a[(j + 1) % seg], b[(j + 1) % seg], b[j]))
            f.smooth = True
    for v in [v for r in rings for v in r]:
        v.co = matrix @ v.co


def make_object(name, bm, mat, thickness):
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces[:])
    me = bpy.data.meshes.new(name)
    bm.to_mesh(me)
    bm.free()
    me.materials.append(mat)
    obj = bpy.data.objects.new(name, me)
    col.objects.link(obj)
    solid = obj.modifiers.new("Thickness", "SOLIDIFY")   # gives the cloth a little thickness
    solid.thickness = thickness
    solid.offset = 1.0
    return obj


PINK = material("GU_TeePink", (0.96, 0.55, 0.70))
LILAC = material("GU_SkirtLilac", (0.72, 0.60, 0.95))
WHITE = material("GU_ShoeWhite", (0.95, 0.95, 0.95), 0.4)

# ---------- T-shirt: body + two short sleeves ----------
bm = bmesh.new()
loft(bm, [(0.90, 0.180, 0.120),     # hem, just over the skirt waist
          (0.95, 0.160, 0.112),
          (1.05, 0.155, 0.106),
          (1.15, 0.176, 0.116),
          (1.24, 0.205, 0.112),
          (1.29, 0.240, 0.088),     # over the shoulders
          (1.33, 0.125, 0.076),
          (1.355, 0.066, 0.056)],   # neckline
     LIFT)
ang = math.radians(14)              # same arm angle as the mannequin
for sx in (-1, 1):
    shoulder = Vector((sx * 0.19, 0, 1.27))
    loft(bm, [(0.04, 0.072, 0.072), (-0.06, 0.070, 0.070), (-0.16, 0.063, 0.063)],
         LIFT @ T(shoulder) @ R(-sx * ang, 4, "Y"), seg=14)
make_object("Worn_Top", bm, PINK, 0.006)

# ---------- skirt: A-line from waist to above the knee ----------
bm = bmesh.new()
loft(bm, [(0.96, 0.150, 0.104),
          (0.86, 0.192, 0.135),
          (0.72, 0.232, 0.180),
          (0.60, 0.262, 0.210)], LIFT, seg=24)
make_object("Worn_Skirt", bm, LILAC, 0.006)

# ---------- shoes: a rounded shell over each foot, flat sole on the floor ----------
bm = bmesh.new()
for sx in (-1, 1):
    ret = bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=1.0)
    for v in ret["verts"]:
        p = Vector((v.co.x * 0.056, v.co.y * 0.118, v.co.z * 0.048)) + Vector((sx * 0.085, -0.035, 0.04))
        p.z = max(p.z, 0.0)          # flat sole
        v.co = LIFT @ p
    for f in {f for v in ret["verts"] for f in v.link_faces}:
        f.smooth = True
bmesh.ops.remove_doubles(bm, verts=bm.verts[:], dist=0.0001)
make_object("Worn_Shoes", bm, WHITE, 0.0)

bpy.ops.wm.save_mainfile()
print("Glow Up clothes added and saved")
