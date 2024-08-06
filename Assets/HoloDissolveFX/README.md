Hologram & Dissolve - Documentation
by Chippalrus
=====================================
Customisable procedural Hologram and Dissolve Shader. Includes shaders with a combination of the two.


=====================================
- Includes 10 Shader: 4 Hologram Shaders, 6 for just dissolves
- Procedural shapes for Dissolve effects
- Procedural scanlines for Hologram distortions
=====================================
# Tested to work in the following render pipelines
- URP 2022.3.0

=====================================
# Hologram Materials
------------------------------------------------------
HoloDissolveFX/Materials/Hologram/Hologram.mat
HoloDissolveFX/Materials/Hologram/Hexagon/A.mat
HoloDissolveFX/Materials/Hologram/Hexagon/B.mat
HoloDissolveFX/Materials/Hologram/Hexagon/C.mat
HoloDissolveFX/Materials/Hologram/Square/A.mat
HoloDissolveFX/Materials/Hologram/Square/B.mat
HoloDissolveFX/Materials/Hologram/Square/C.mat
HoloDissolveFX/Materials/Hologram/Triangle/A.mat
HoloDissolveFX/Materials/Hologram/Triangle/B.mat
HoloDissolveFX/Materials/Hologram/Triangle/C.mat

=====================================
# Opaque Materials (Alpha Clip)
------------------------------------------------------
HoloDissolveFX/Materials/Opaque/Hologram.mat
HoloDissolveFX/Materials/Opaque/Hexagon/A.mat
HoloDissolveFX/Materials/Opaque/Hexagon/B.mat
HoloDissolveFX/Materials/Opaque/Hexagon/C.mat
HoloDissolveFX/Materials/Opaque/Square/A.mat
HoloDissolveFX/Materials/Opaque/Square/B.mat
HoloDissolveFX/Materials/Opaque/Square/C.mat
HoloDissolveFX/Materials/Opaque/Triangle/A.mat
HoloDissolveFX/Materials/Opaque/Triangle/B.mat
HoloDissolveFX/Materials/Opaque/Triangle/C.mat

=====================================
# Transition Materials
------------------------------------------------------
HoloDissolveFX/Materials/Transition/Hologram.mat
HoloDissolveFX/Materials/Transition/Hexagon/A.mat
HoloDissolveFX/Materials/Transition/Hexagon/B.mat
HoloDissolveFX/Materials/Transition/Hexagon/C.mat
HoloDissolveFX/Materials/Transition/Square/A.mat
HoloDissolveFX/Materials/Transition/Square/B.mat
HoloDissolveFX/Materials/Transition/Square/C.mat
HoloDissolveFX/Materials/Transition/Triangle/A.mat
HoloDissolveFX/Materials/Transition/Triangle/B.mat
HoloDissolveFX/Materials/Transition/Triangle/C.mat

=====================================
# Shader files
------------------------------------------------------
HoloDissolveFX/Shaders/Graphs/Dissolve/Holograms/Hexagon.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Holograms/Square.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Holograms/Triangle.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Opaque/Hexagon.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Opaque/Square.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Opaque/Triangle.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Transition/Hexagon.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Transition/Square.shadergraph
HoloDissolveFX/Shaders/Graphs/Dissolve/Transition/Triangle.shadergraph
HoloDissolveFX/Shaders/Graphs/Hologram/Procedural.shadergraph

=====================================
# Scripts
------------------------------------------------------
HoloDissolveFX/Scripts/CMeshBounds.cs
HoloDissolveFX/Scripts/CSkinnedMeshBounds.cs

=====================================
# HOW TO USE
------------------------------------------------------
## Provided Materials
------------------------------------------------------
1. Apply the material to an object.
2. Play around with the material properties in the inspector
    - See further down for material properties for definitions of what they each do.
3. Add CMeshBounds.cs or CSkinnedMeshBounds.cs
 - CSkinnedMeshBounds for SkinnedMesh objects
4. Drag the material from the mesh to the "Material" list on the script in the inspector
5. Drag the MeshRenderer or SkinnedMeshRenderer to the "Renderer" from the inspector
    - The scripts CMeshBounds.cs / CSkinnedMeshBounds.cs automatically tell the shader the bounding values of the mesh
    - this is required to adjust for dissolve deformation offsets
------------------------------------------------------
## Create new Material from shader
------------------------------------------------------
1. Locate the shader files in "HoloDissolveFX/Shaders/Graphs/"
    - "/Dissolve/" for dissolve shaders (there will be additional folders, Hologram/Opaque/Transition)
    - - "/Hologram/" is a combination of both the Hologram shader with Dissolve shader (It also allows for blending with the Base Colour or Texture)
    - - "/Opaque/" has just Dissolve shader (use this if you want objects to use Textures and Normals)
    - - "/Transition/"  has a two layer dissolve going from Base Colour or Texture to Hologram, then the dissolve on the hologram. (can be adjusted)
    - "/Hologram/" for just the hologram shader
    
2. Right click on the desired shader and create a new material from it.
3. Apply the newly created material to an object.
4. Play around with the material properties in the inspector
    - See further down for material properties for definitions of what they each do.
5. Add CMeshBounds.cs or CSkinnedMeshBounds.cs
 - CSkinnedMeshBounds for SkinnedMesh objects
6. Drag the material from the mesh to the "Material" list on the script in the inspector
7. Drag the MeshRenderer or SkinnedMeshRenderer to the "Renderer" from the inspector
    - The scripts CMeshBounds.cs / CSkinnedMeshBounds.cs automatically tell the shader the bounding values of the mesh
    - this is required to adjust for dissolve deformation offsets

# MATERIAL PROPERTIES
=====================================
## Dissolve Style (if applicable)
-  Selects the tiling pattern for dissolve

## MAIN
------------------------------------------------------
Properties for Albedo, Normal, Colour

### Colour
- Base colour of object (Holograms set to Black)

### Texture
- Main texture

### Alpha Channel
- Enable/Disable Alpha transparency from Texture

### Alpha
- Transparency amount of alpha channel

### Normal
- Normal texture

### Normal Strength
- Adjusts intensity of normal texture

### Emissive Channel
- Enable/Disable Emissive texture

### Emissive
- Emissive texture

### Emissive Intensity
- Emissive texture brightness

## Height Value
- Mesh bounding value in Y
- This is used for shader data there are provided scripts
(CMeshBounds.cs/CSkinnedMeshBounds.cs) to automatically set the bounds
( Note: I'm aware "Object node" now provides this information,
  this method allows compatibility with Unity builds below 2022, I may update it later on )

------------------------------------------------------
## DISSOLVE FX
------------------------------------------------------
Main Dissolve properties.
Dissolve is relative to Screen space locked to the object.

### Dissolve
- Sets the position of the dissolve
- Use this as the dissolve controller

### Dissolve Rotation ( Degrees )
- Rotates the direction of the dissolve

### Dissolve Offset
- Adjusts the X and Y offset of Dissolve on the object
- Dissolve uses Screen space locked to the object

### Dissolve Pattern Tiling
- Increases or decreases Hexagon/Square/Triangle tiling

### Dissolve Pattern Offset
- Adjusts how condensed the dissolve becomes

### Dissolve Pattern Rotation ( Degrees )
- Rotates local to the pattern

### Dissolve Range
- Adjusts the position of dissolve relative to "Dissolve" property.

### EdgeLerpColour
- Lerps from "EdgeColour"

### EdgeLerpHeight
- Size of the transition between "EdgeColour" and "EdgeLerpColour"

### EdgeLerpTreshold
- When to transition between "EdgeColour" and "EdgeLerpColour"

### EdgeLerpFeathering
- Adjusts the gradient between the lerp'd colours

### EdgeColour
- Edge emissive colour

### EdgeWidth
- Size of emissive when dissolving

### EdgeWidthMult
- Multiplier for Edge Width

### Edge Intensity
- Emissive brightness

------------------------------------------------------
## BASE & DISSOLVE TO HOLOGRAM (If applicable)
------------------------------------------------------
### Base Intensity
- Brightness of the base colour/Texture

### Base Dissolve Offset
- Offsets Base Dissolve before "Dissolve" property
- Acts like a delay of sorts for Dissolve

### Base EdgeColour
- Edge emissive colour

### Base EdgeLerpHeight
- Size of the transition between "Base EdgeColour" and "Base EdgeLerpColour"

### Base EdgeLerpTreshold
- When to transition between "Base EdgeColour" and "Base EdgeLerpColour"

### Base EdgeLerpFeathering
- Adjusts the gradient between the lerped colours

### EdgeLerpColour
- Lerps from "EdgeColour"

### Base EdgeWidth
- Size of emissive when dissolving
- This is also influenced by "EdgeWidthMult" property

### Base Edge Intensity
- Emissive brightness

------------------------------------------------------
## BASE &  HOLOGRAM BLENDING (If applicable)
------------------------------------------------------

### Blending Overwrite
- Switches blending to to use "Blending Amount" property
- If its set to false, it will transition using "Dissolve" property

### Blending Amount
- Lerp between texture and hologram

### Base Intensity
- Brightness of the base colour/texture

### Blending Threshold
- Blending limits blending from "Blending Amount"
- Acts like a strength when blending is applied

### Blending Alpha
- Transparency range of the blend
- Higher the more transparent

------------------------------------------------------
## DISSOLVE DEFORMATION
------------------------------------------------------
Vertex position properties when dissolving
Distortion from this deformation is local to the object
The deformation's orientation follows the root rotation of the object

### Deformation Power
- Deformation strength

### Deform Range
- Adjusts the position of deform relative to "Dissolve" property.

### Deformation Absolute Y
- Force Y deformation to only positive direction

### Deformation Axis
- Plane and size of deformation

------------------------------------------------------
## OBJECT FADING
------------------------------------------------------
Give object a gradient fade based on Screen UV locked to object
This can be used as height limit for dissolve deformation or
for giving Hologram a directional projection look.

### Fade Height
- Maximum scale of the fading

### Fade Threshold
- Scale of fading relative to "Fade Height"

### Fade Feathering
- Amount of gradient of the fade where 0f creates a hard line and 1.0f stretches the fade

### Fade Rotation ( Degrees )
- Direction of the fading
------------------------------------------------------
## HOLOGRAM
------------------------------------------------------
Main hologram properties

### Hologram Colour
- Colour of the fresnel, This is also the hologram's overall colour

### Hologram Lerp Colour
- Secondary colour of hologram

### Hologram Lerp Height
- Size of the transition between "Hologram Colour" and "Hologram Lerp Colour"

### Hologram Lerp Threshold
- Transition offset between "Hologram Colour" and "Hologram Lerp Colour"

### Hologram Lerp Feathering
- Gradient edge between "Hologram Colour" and "Hologram Lerp Colour"

### Hologram Intensity
- Overall intensity of hologram

### Hologram Transparency
- Transparency in between Fresnel

### Fresnel Power
- Sharpness of the edge of Fresnel

### Fresnel Edge Intensity
- Emissive intensity of fresnel's edge

### Fresnel Intensity
- Overall emissive intensity fresnel

### Fresnel Sub-Scanline Blending
- Cutout amount using Sub-Scanline on Fresnel edges

### Intersection Scale
- Scale of the fresnel effect when intersecting on other objects

### Scanline Tiling
- Primary scanline tiling

### Scanline Speed
- Primary scanline offset based on Time

### Sub-Scanline Tiling
- Secondary scanline tiling

### Sub-Scanline Speed
- Secondary scanline offset based on Time

### Sub-Scanline Dissolve Edge
- Scale of the transparency between the tiling of Secondary Scanline

### Sub-Scanline Noise Cutout
- Intensity of the Simple noise alpha cutout on secondary scanlines
- This give it a static like effect

------------------------------------------------------
## HOLOGRAM DISTORTION (Scanline/Sub-Scanline)
------------------------------------------------------
These properties adjust the strength and intervals

>> Distortion influenced by Primary & Secondary Scanlines
>> "Tertiary" also uses Secondary Scanline similar to "Sub-Scan Distortion"
>> "Tertiary" just with a different calculation.

### Distortion Axis
- Turn on or off distortion on Axis, where 0 is off
- This does not affect "Dissolve Deformation" section

### Distortion intensity
- Overall distortion scale, this is basically a factor of properties below it

### Scan Distortion
- Interval amount of distortion, where 1.0f is always in effect

### Scan Distortion Scale
- Strength of the distortion

### Scan Direction Delay
- Amount of time it stays on a single direction when distorting
- The lower this is the more "jitter" it could look

### Sub-Scan Distortion
- Interval amount of distortion, where 1.0f is always in effect

### Sub-Scan Distortion Scale
- Strength of the distortion

### Sub-Scan Direction Delay
- Interval amount of distortion, where 1.0f is always in effect

### Tertiary Distortion
- Interval amount of distortion, where 1.0f is always in effect

### Tertiary Distortion Scale
- Strength of the distortion

### Tertiary Direction Delay
- Interval amount of distortion, where 1.0f is always in effect

### Lerp Sub-Scan and Tertiary
- Lerps between Sub-Scan and Tertiary
- 0f   = Sub-Scan only
- 1.0f = Tertiary only

=====================================
## Notes:
=====================================
With high FOV there are some warping/position offsets
when objects move closer to the screen edge.
