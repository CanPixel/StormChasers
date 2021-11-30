// Shapes © Freya Holmér - https://twitter.com/FreyaHolmer/
// Website & Documentation - https://acegikmo.com/shapes/
#include "Shapes Config.cginc"
#include "Shapes Math.cginc"

// macros
#define PROP(p) UNITY_ACCESS_INSTANCED_PROP(Props,p)
#define PROP_DEF(t,p) UNITY_DEFINE_INSTANCED_PROP(t,p)

// can't be in FillUtils.cginc because of definition order
#define SHAPES_FILL_PROPERTIES \
PROP_DEF(half4, _ColorEnd) \
PROP_DEF(int, _FillType) \
PROP_DEF(int, _FillSpace) \
PROP_DEF(float4, _FillStart) /* xyz = pos, w = radius*/ \
PROP_DEF(float3, _FillEnd) /* xyz = pos */

// can't be in DashUtils.cginc because of definition order
#define SHAPES_DASH_PROPERTIES \
PROP_DEF(int, _DashType) \
PROP_DEF(half, _DashSize) \
PROP_DEF(half, _DashShapeModifier) \
PROP_DEF(half, _DashOffset) \
PROP_DEF(half, _DashSpacing) \
PROP_DEF(int, _DashSpace) \
PROP_DEF(int, _DashSnap)



// parameters for selection outlines
#ifdef SCENE_VIEW_OUTLINE_MASK
	int _ObjectId;
	int _PassValue;
#endif
#ifdef SCENE_VIEW_PICKING
	uniform float4 _SelectionID;
#endif

// used for the final output. supports branching based on opaque vs transparent and outline functions
inline half4 ShapesOutput( half4 shape_color, float shape_mask ){
    half4 outColor = half4(shape_color.rgb, shape_mask * shape_color.a);

    clip(outColor.a - VERY_SMOL); // todo: this disallows negative colors, which, might be bad? idk

    // fade toward black if
    #if defined( SCREEN ) || defined( SUBTRACTIVE ) || defined( ADDITIVE ) || defined( LIGHTEN ) || defined( COLORDODGE )
        outColor.rgb *= outColor.a;
    #endif

    // fade toward white if
    #if defined( MULTIPLICATIVE ) || defined( LINEARBURN ) || defined( DARKEN ) || defined( COLORBURN )
        outColor.rgb = 1 + outColor.a * ( outColor.rgb - 1 ); // lerp(1,b,t) = 1 + t(b - 1);
    #endif

    // linear burn is additive minus one, so we're doing the subtract here
    #ifdef LINEARBURN
        outColor.rgb -= 1;
    #endif
    #ifdef COLORDODGE
        outColor.rgb = 1.0/max(VERY_SMOL,1-outColor.rgb);
    #endif
    #ifdef COLORBURN
        outColor.rgb = 1-(1.0/max(VERY_SMOL,outColor.rgb));
    #endif
    
    #if defined(SCENE_VIEW_OUTLINE_MASK) || defined(SCENE_VIEW_PICKING)
        clip(shape_mask - 0.5 + VERY_SMOL); // Don't take color into account
    #endif 
    
    #if defined( SCENE_VIEW_OUTLINE_MASK )
        return float4(_ObjectId, _PassValue, 1, 1);
    #elif defined( SCENE_VIEW_PICKING )
        return _SelectionID;
    #else
        return outColor; // Render mesh
    #endif
}