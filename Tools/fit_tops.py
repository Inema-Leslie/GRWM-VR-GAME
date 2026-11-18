# GRWM - fits the Genies tops (shirts + hoodies) to the girl in MannequinFromBlender.blend.
# Open MannequinFromBlender.blend in Blender, then run this from the Scripting tab.
# Every top comes in as a "Fit_<name>" object next to Worn_Top, already on her body.
# Long sleeves are trimmed at the shoulder, so every top ends up sleeveless.
#
# How it fits: all Genies clothes share one body, so one move + scale (worked out from a plain
# shirt against her own Worn_Top) puts every top on her without losing its cut, hood or length.
# Then a Shrinkwrap in "Outside" mode pushes out only the bits that sink into her skin.

import bpy, bmesh, os, glob, math
from mathutils import Vector

PROJECT = os.path.normpath(os.path.join(os.path.dirname(bpy.data.filepath), "..", "..", ".."))
CLOTHES = os.path.join(PROJECT, "Assets", "Models", "Clothes")
LOG = os.path.join(PROJECT, "Logs", "fit_tops.txt")
REFERENCE = "shirt-0001-campusComeback"   # a plain tee, used to line Genies space up with hers
EASE = 1.04          # a little room so it doesn't look painted on
BODY_EASE = 1.04
SKIN_GAP = 0.006     # metres between skin and fabric
FLIP = False         # set True if the tops come in facing backwards

log = []
def say(*a):
    line = " ".join(str(x) for x in a)
    print(line); log.append(line)

def world_box(obj):
    pts = [obj.matrix_world @ Vector(c) for c in obj.bound_box]
    lo = Vector((min(p.x for p in pts), min(p.y for p in pts), min(p.z for p in pts)))
    hi = Vector((max(p.x for p in pts), max(p.y for p in pts), max(p.z for p in pts)))
    return lo, hi

def import_top(path):
    before = set(bpy.data.objects)
    try:
        bpy.ops.import_scene.fbx(filepath=path)
    except (AttributeError, RuntimeError):
        bpy.ops.wm.fbx_import(filepath=path)   # newer Blender's built-in importer
    new = [o for o in bpy.data.objects if o not in before]
    meshes = [o for o in new if o.type == 'MESH']
    # keep the mesh in place, drop the rig and empties
    for o in meshes:
        mw = o.matrix_world.copy()
        for m in list(o.modifiers):
            if m.type == 'ARMATURE': o.modifiers.remove(m)
        o.parent = None
        o.matrix_world = mw
    for o in new:
        if o.type != 'MESH': bpy.data.objects.remove(o, do_unlink=True)
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes: o.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1: bpy.ops.object.join()
    g = bpy.context.view_layer.objects.active
    if FLIP: g.rotation_euler.z += 3.14159265
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    return g

# ---- her body and her own fitted tee ----
body = bpy.data.objects.get("Mannequin_Girl")
tee = bpy.data.objects.get("Worn_Top")
if body is None or tee is None:
    raise SystemExit("Open MannequinFromBlender.blend first (needs Mannequin_Girl and Worn_Top)")
if bpy.context.object and bpy.context.object.mode != 'OBJECT':
    bpy.ops.object.mode_set(mode='OBJECT')

# clear an earlier run
for o in [o for o in bpy.data.objects if o.name.startswith("Fit_")]:
    bpy.data.objects.remove(o, do_unlink=True)

tops = sorted(glob.glob(os.path.join(CLOTHES, "*", "*", "shirt-*_model.fbx")) +
              glob.glob(os.path.join(CLOTHES, "*", "*", "hoodie-*_model.fbx")))
say("tops found:", len(tops))

# ---- Genies space -> her space, from the reference tee ----
ref_path = [p for p in tops if REFERENCE in p][0]
ref = import_top(ref_path)
rlo, rhi = world_box(ref)
tlo, thi = world_box(tee)
rsize, tsize = rhi - rlo, thi - tlo
# depth and height set the scale; width follows them, because the reference tee may have no
# sleeves while hers does (comparing widths would stretch every sleeved top)
s_yz = (tsize.y / rsize.y + tsize.z / rsize.z) / 2
scale = Vector((s_yz * BODY_EASE, tsize.y / rsize.y * BODY_EASE, tsize.z / rsize.z * EASE))
# line up the middles across, and the shoulders (tops) up and down
anchor_src = Vector(((rlo.x + rhi.x) / 2, (rlo.y + rhi.y) / 2, rhi.z))
anchor_dst = Vector(((tlo.x + thi.x) / 2, (tlo.y + thi.y) / 2, thi.z))
bpy.data.objects.remove(ref, do_unlink=True)
say("her tee size", tuple(round(v, 3) for v in tsize), "genies tee size", tuple(round(v, 3) for v in rsize))
say("scale", tuple(round(v, 3) for v in scale))

# ---- her arm: shoulder pivot and how far it hangs down ----
bw = body.matrix_world
bverts = [bw @ v.co for v in body.data.vertices]
shoulder_z = thi.z - 0.02   # top of her shoulder, so sleeves keep covering it
band = [p for p in bverts if abs(p.z - shoulder_z) < 0.03]
pivot_x = max(abs(p.x) for p in band) - 0.04          # where the arm leaves the torso
hand = max((p for p in bverts if p.z > 0.6), key=lambda p: abs(p.x))   # furthest-out point = hand
her_angle = math.atan2(shoulder_z - hand.z, abs(hand.x) - pivot_x)   # radians below horizontal
say("shoulder pivot x", round(pivot_x, 3), "z", round(shoulder_z, 3), "her arm angle", round(math.degrees(her_angle), 1))

def smooth(a, b, x):
    t = min(1.0, max(0.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)

def drop_sleeves(me):
    """Swing the sleeves down around the shoulder so they follow her arms."""
    far = max(me.vertices, key=lambda v: abs(v.co.x)).co
    if abs(far.x) < pivot_x + 0.08:
        return None   # short or no sleeves: nothing to swing
    top_angle = math.atan2(shoulder_z - far.z, abs(far.x) - pivot_x)
    delta = her_angle - top_angle
    for v in me.vertices:
        ax = abs(v.co.x)
        w = smooth(pivot_x - 0.02, pivot_x + 0.04, ax)
        if w <= 0: continue
        dx, dz = ax - pivot_x, v.co.z - shoulder_z
        r = math.hypot(dx, dz)
        th = math.atan2(-dz, dx) + w * delta
        nx, nz = pivot_x + r * math.cos(th), shoulder_z - r * math.sin(th)
        v.co.x = nx if v.co.x >= 0 else -nx
        v.co.z = nz
    me.update()
    return math.degrees(delta)

# her arm, sampled around any height: centre (|x|, y) and half-thickness
STEP = 0.02
arm_pts = [(abs(p.x), p.y, p.z) for p in bverts
           if abs(p.x) > pivot_x + 0.05 and hand.z - 0.02 < p.z < shoulder_z]   # clear of the torso
def stats(pts):
    xs = [a for a, _ in pts]; ys = [b for _, b in pts]
    return ((min(xs) + max(xs)) / 2, (min(ys) + max(ys)) / 2, (max(xs) - min(xs)) / 2, (max(ys) - min(ys)) / 2)
def arm_at(z):
    for half in (0.04, 0.08, 0.12):   # her arm is low-poly, so widen the window until it finds a ring
        near = [(x, y) for x, y, pz in arm_pts if abs(pz - z) < half]
        if len(near) >= 6: return stats(near)
    return None
say("arm points", len(arm_pts))

def wrap_arms(me):
    """Centre each slice of sleeve on her arm and make it just big enough to go round it."""
    sleeve = {}
    for v in me.vertices:
        if smooth(pivot_x - 0.02, pivot_x + 0.04, abs(v.co.x)) > 0.5:
            sleeve.setdefault(int(v.co.z / STEP), []).append((abs(v.co.x), v.co.y))
    fit = {}
    for k, pts in sleeve.items():
        a = arm_at((k + 0.5) * STEP)
        if a is None or len(pts) < 4: continue
        sx, sy, srx, sry = stats(pts)
        ax, ay, arx, ary = a
        if srx < 0.015 or sry < 0.015: continue   # a sliver of cuff or seam: leave it alone
        kx = min(2.0, max(1.0, (arx + 0.012) / srx))   # capped, so nothing gets flung out
        ky = min(2.0, max(1.0, (ary + 0.012) / sry))
        fit[k] = (sx, sy, ax, ay, kx, ky)
    for v in me.vertices:
        k = int(v.co.z / STEP)
        if k not in fit: continue
        w = smooth(pivot_x - 0.02, pivot_x + 0.04, abs(v.co.x))
        if w <= 0: continue
        sx, sy, ax, ay, kx, ky = fit[k]
        nx = ax + (abs(v.co.x) - sx) * kx
        ny = ay + (v.co.y - sy) * ky
        x = abs(v.co.x) + w * (nx - abs(v.co.x))
        v.co.x = x if v.co.x >= 0 else -x
        v.co.y = v.co.y + w * (ny - v.co.y)
    me.update()
    return len(fit)

collection = tee.users_collection[0]
for path in tops:
    name = os.path.basename(path).replace("_model.fbx", "")
    g = import_top(path)
    me = g.data
    for v in me.vertices:
        p = v.co - anchor_src
        v.co = Vector((p.x * scale.x, p.y * scale.y, p.z * scale.z)) + anchor_dst
    me.update()
    # sleeveless: trim each sleeve off at the shoulder (the Genies arms sit somewhere else)
    bm = bmesh.new(); bm.from_mesh(me)
    cut = [v for v in bm.verts if abs(v.co.x) > pivot_x + 0.025 and v.co.z < shoulder_z + 0.03]
    if cut:
        bmesh.ops.delete(bm, geom=cut, context='VERTS')
        say(name, "sleeves trimmed:", len(cut), "verts")
    bm.to_mesh(me); bm.free(); me.update()

    # everything below the neckline hugs her body with a little room; hoods and collars keep their shape
    wrap = g.vertex_groups.new(name="Wrap")
    wrap.add([v.index for v in me.vertices if v.co.z < shoulder_z + 0.02], 1.0, 'REPLACE')
    mods = []
    m = g.modifiers.new("Soften", 'CORRECTIVE_SMOOTH'); m.factor = 0.5; m.iterations = 5; mods.append(m.name)
    m = g.modifiers.new("Hug", 'SHRINKWRAP'); m.target = body; m.wrap_method = 'NEAREST_SURFACEPOINT'
    m.wrap_mode = 'OUTSIDE_SURFACE'; m.offset = 0.012; m.vertex_group = "Wrap"; mods.append(m.name)
    m = g.modifiers.new("Relax", 'CORRECTIVE_SMOOTH'); m.factor = 0.3; m.iterations = 3; mods.append(m.name)
    m = g.modifiers.new("FitToBody", 'SHRINKWRAP'); m.target = body; m.wrap_method = 'NEAREST_SURFACEPOINT'
    m.wrap_mode = 'OUTSIDE'; m.offset = SKIN_GAP; mods.append(m.name)   # last: nothing ends up inside her
    bpy.ops.object.select_all(action='DESELECT')
    g.select_set(True)
    bpy.context.view_layer.objects.active = g
    for name_ in mods:
        bpy.ops.object.modifier_apply(modifier=name_)

    g.name = g.data.name = "Fit_" + name
    for c in list(g.users_collection): c.objects.unlink(g)
    collection.objects.link(g)
    if tee.parent:   # sit under the same parent as her own tee, keeping where it is
        mw = g.matrix_world.copy()
        g.parent = tee.parent
        g.matrix_world = mw
    lo, hi = world_box(g)
    say(g.name, "verts", len(me.vertices), "box", tuple(round(v, 3) for v in lo), tuple(round(v, 3) for v in hi))

lo, hi = world_box(body)
say("body box", tuple(round(v, 3) for v in lo), tuple(round(v, 3) for v in hi))
say("tee box", tuple(round(v, 3) for v in tlo), tuple(round(v, 3) for v in thi))
bpy.ops.wm.save_mainfile()
say("saved", bpy.data.filepath)
os.makedirs(os.path.dirname(LOG), exist_ok=True)
open(LOG, "w").write("\n".join(log))
