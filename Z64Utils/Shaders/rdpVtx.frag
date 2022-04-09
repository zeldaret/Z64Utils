#version 330 core


// Render Mode
#define MODE_WIREFRAME      0
#define MODE_TEXTURED       1
#define MODE_SURFACE        2
#define MODE_NORMAL         3


// Geometry Mode
#define G_SHADE     4u
#define G_FOG       0x10000u
#define G_LIGHTING  0x20000u

// Color Combiner 
#define G_CCMUX_COMBINED 0
#define G_CCMUX_TEXEL0 1
#define G_CCMUX_TEXEL1 2
#define G_CCMUX_PRIMITIVE 3
#define G_CCMUX_SHADE 4
#define G_CCMUX_ENVIRONMENT 5
#define G_CCMUX_CENTER 6
#define G_CCMUX_SCALE 6
#define G_CCMUX_COMBINED_ALPHA 7
#define G_CCMUX_TEXEL0_ALPHA 8
#define G_CCMUX_TEXEL1_ALPHA 9
#define G_CCMUX_PRIMITIVE_ALPHA 10
#define G_CCMUX_SHADE_ALPHA 11
#define G_CCMUX_ENV_ALPHA 12
#define G_CCMUX_LOD_FRACTION 13
#define G_CCMUX_PRIM_LOD_FRAC 14
#define G_CCMUX_NOISE 7
#define G_CCMUX_K4 7
#define G_CCMUX_K5 15
#define G_CCMUX_1 6
#define G_CCMUX_0 31

#define G_ACMUX_COMBINED 0
#define G_ACMUX_TEXEL0 1
#define G_ACMUX_TEXEL1 2
#define G_ACMUX_PRIMITIVE 3
#define G_ACMUX_SHADE 4
#define G_ACMUX_ENVIRONMENT 5
#define G_ACMUX_LOD_FRACTION 0
#define G_ACMUX_PRIM_LOD_FRAC 6
#define G_ACMUX_1 6
#define G_ACMUX_0 7


// Other Mode LO
#define G_MDSFT_TEXTLUT     0b0000_0000_0000_0000_1100_0000_0000_0000
#define G_TT_NONE   0
#define G_TT_RGBA16 2
#define G_TT_IA16   3

#define G_MDSFT_CYCLETYPE   0b0000_0000_0011_0000_0000_0000_0000_0000
#define G_CYC_1CYCLE    0
#define G_CYC_2CYCLE    1
#define G_CYC_COPY      2
#define G_CYC_FILL      3

// Color Blender
#define G_BL_CLR_IN     0u
#define G_BL_CLR_MEM    1u
#define G_BL_CLR_BL     2u
#define G_BL_CLR_FOG    3u

#define G_BL_A_IN       0u
#define G_BL_A_FOG      1u
#define G_BL_A_SHADE    2u

#define G_BL_1MA        0u
#define G_BL_A_MEM      1u

#define G_BL_1          2u
#define G_BL_0          3u


#define G_TX_NOMIRROR   0
#define G_TX_MIRROR     1
#define G_TX_WRAP       0
#define G_TX_CLAMP      2


struct TileAttribute
{
    sampler2D tex;
    ivec2 cm;
    ivec2 mask;
    ivec2 shift;
    vec2 ul;
    vec2 lr;
};


// Input
in vec2 v_VtxTexCoords;
in vec3 v_VtxNormal;
in vec4 v_VtxColor;
flat in int v_VtxId;

// Output
out vec4 FragColor;


// Uniforms
uniform vec3 u_DirLight;
uniform TileAttribute u_Tiles[2];

uniform vec3 u_ChromaKeyCenter;
uniform vec3 u_ChromaKeyScale;

uniform vec4 u_HighlightColor;
uniform bool u_HighlightEnabled;
uniform vec4 u_WireFrameColor;
uniform int u_ModelRenderMode;
uniform bool u_LigthingEnabled;


struct RdpColor
{
    vec4 prim;
    float primLod;
    vec4 blend;
    vec4 env;
    vec4 fog;
};

struct RdpOtherMode
{
    uint hi;
    uint lo;
};

struct RdpCombiner
{
    ivec4 c1;
    ivec4 a1;
    ivec4 c2;
    ivec4 a2;
};

struct RdpState
{
    RdpColor color;
    RdpOtherMode otherMode;
    uint geoMode;
    RdpCombiner combiner;
};

uniform RdpState u_RdpState;



// Constants
const vec3 LigthFacing = vec3(0, 0, 1);

vec4 Red = vec4(1, 0, 0, 1);
vec4 Green = vec4(0, 1, 0, 1);
vec4 Blue = vec4(0, 0, 1, 1);


int clampMirrorMask(int x, int l, int h, int mask, int cm)
{
    int bit = (1 << mask);

    // clamp
    if ((cm & G_TX_CLAMP) != 0)
    {
        x = min(x, h);
        x = max(x, l);
    }

    bool mirror = (abs(x) / bit) % 2 == (x >= 0 ? 1 : 0);

    x = ((x % bit) + bit) % bit;

    if (mirror && ((cm & G_TX_MIRROR) != 0))
        x = bit - 1 - x;
    
    return x;
}

vec4 texelAt(int i, ivec2 pos)
{
    int s = clampMirrorMask(pos.x, int(u_Tiles[i].ul.x), int(u_Tiles[i].lr.x), u_Tiles[i].mask.x, u_Tiles[i].cm.x);
    int t = clampMirrorMask(pos.y, int(u_Tiles[i].ul.y), int(u_Tiles[i].lr.y), u_Tiles[i].mask.y, u_Tiles[i].cm.y);

    return texelFetch(u_Tiles[i].tex, ivec2(s, t), 0);
}

vec4 bilinearSample(int tile, vec2 uv)
{
    vec2 f = fract(uv);
    ivec2 base = ivec2(floor(uv));
    vec4 p00 = texelAt(tile, base + ivec2(0, 0));
    vec4 p10 = texelAt(tile, base + ivec2(1, 0));
    vec4 p01 = texelAt(tile, base + ivec2(0, 1));
    vec4 p11 = texelAt(tile, base + ivec2(1, 1));

    return mix(mix(p00, p10, f.x), mix(p01, p11, f.x), f.y);
}

vec4 texel(int i)
{
    return bilinearSample(i, v_VtxTexCoords);
}


// this is technically phong shading but whatever
float gouraudShading(vec3 light, float ambient)
{
    float diffuse = max(0, dot(normalize(light), normalize(v_VtxNormal)));
    return (diffuse + ambient) * 0.6; // hack
}

float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}


vec4 blendCycle(vec4 x, bool first, float shadeAlpha)
{
    uint settings = u_RdpState.otherMode.lo >> 16;
    if (first)
        settings >>= 2;

    uint pFlag = settings >> 12 & 3u;
    uint aFlag = settings >> 8 & 3u;
    uint mFlag = settings >> 4 & 3u;
    uint bFlag = settings >> 0 & 3u;

    vec4 p, m;
    float a, b;

    // P
    switch (pFlag)
    {
        case G_BL_CLR_IN:
            p = vec4(x.xyz, 1);
            break;
        case G_BL_CLR_MEM:
            p = vec4(0); // todo
            break;
        case G_BL_CLR_BL:
            p = u_RdpState.color.blend;
            break;
        case G_BL_CLR_FOG:
            p = u_RdpState.color.fog;
            break;
    }

    // A
    switch (aFlag)
    {
        case G_BL_A_IN:
            a = x.a;
            break;
        case G_BL_A_FOG:
            a = u_RdpState.color.fog.a;
            break;
        case G_BL_A_SHADE:
            a = shadeAlpha;
            break;
        case G_BL_0:
            a = 0;
            break;
    }

    // M
    switch (mFlag)
    {
        case G_BL_CLR_IN:
            m = vec4(x.xyz, 1);
            break;
        case G_BL_CLR_MEM:
            m = vec4(0); // todo
            break;
        case G_BL_CLR_BL:
            m = u_RdpState.color.blend;
            break;
        case G_BL_CLR_FOG:
            m = u_RdpState.color.fog;
            break;
    }

    // B
    switch (bFlag)
    {
        case G_BL_1MA:
            b = 1.0 - a;
            break;
        case G_BL_A_MEM :
            b = 0.; // todo
            break;
        case G_BL_1:
            a = 1.;
            break;
        case G_BL_0:
            a = 0.;
            break;
    }
    
    // Color Blender Formula
    vec4 ret = (p * a + m * b);
    if (!first)
        ret /= (a + b);
    return ret;
}


vec3 combineColorAny(vec4 x, int flag)
{
    switch (flag)
    {
        case G_CCMUX_COMBINED:
            return x.xyz;
        case G_CCMUX_TEXEL0:
            return texel(0).xyz;
        case G_CCMUX_TEXEL1:
            return texel(1).xyz;
        case G_CCMUX_PRIMITIVE:
            return u_RdpState.color.prim.xyz;
        case G_CCMUX_SHADE:
            return ((u_RdpState.geoMode & G_LIGHTING) != 0u)
                ? vec3(gouraudShading(LigthFacing, 0.5))
                : v_VtxColor.xyz;
        case G_CCMUX_ENVIRONMENT:
            return u_RdpState.color.env.xyz;
    }
    
    // defaults to G_CCMUX_0
    return vec3(0);
}

vec3 combineA(vec4 x, int flag)
{
    vec3 ret = combineColorAny(x, flag);
    switch (flag)
    {
        case G_CCMUX_1:
            return vec3(1);
        case G_CCMUX_NOISE:
            return vec3(rand(v_VtxColor.xy), rand(v_VtxColor.yz), rand(v_VtxColor.zw));
    }

    // 0x08-0x0F : G_CCMUX_0
    return ret;
}

vec3 combineB(vec4 x, int flag)
{
    vec3 ret = combineColorAny(x, flag);
    switch (flag)
    {
        case G_CCMUX_CENTER:
            return u_ChromaKeyCenter;
        // case G_CCMUX_K4:
    }
    
    // 0x08-0x0F : G_CCMUX_0
    return ret;
}

vec3 combineC(vec4 x, int flag, float shadeAlpha)
{
    vec3 ret = combineColorAny(x, flag);
    switch (flag)
    {
        case G_CCMUX_SCALE:
            return u_ChromaKeyScale;
        case G_CCMUX_COMBINED_ALPHA:
            return x.aaa;
        case G_CCMUX_TEXEL0_ALPHA:
            return texel(0).aaa;
        case G_CCMUX_TEXEL1_ALPHA:
            return texel(1).aaa;
        case G_CCMUX_PRIMITIVE_ALPHA:
            return u_RdpState.color.prim.aaa;
        case G_CCMUX_SHADE_ALPHA:
            return vec3(shadeAlpha);
        case G_CCMUX_ENV_ALPHA:
            return u_RdpState.color.env.aaa;
        case G_CCMUX_LOD_FRACTION:
            //return vec3(0);
            return vec3(u_RdpState.color.primLod);
        // G_CCMUX_PRIM_LOD_FRAC
        // G_CCMUX_K5
    }
    
    // 0x10-0x1F : G_CCMUX_0
    return ret;
}

vec3 combineD(vec4 x, int flag)
{
    vec3 ret = combineColorAny(x, flag);
    switch (flag)
    {
        case G_CCMUX_1:
            return vec3(1);
    }
    // 0x07 : G_CCMUX_0
    // if (flag == G_CCMUX_SHADE)
    //     ret = vec3(0);
    return ret;
}

float combineAlphaAny(vec4 x, int flag, float shadeAlpha)
{
    switch (flag)
    {
        case G_ACMUX_TEXEL0:
            return texel(0).a;
        case G_ACMUX_TEXEL1:
            return texel(1).a;
        case G_ACMUX_PRIMITIVE:
            return u_RdpState.color.prim.a;
        case G_ACMUX_SHADE:
            return shadeAlpha;
        case G_ACMUX_ENVIRONMENT:
            return u_RdpState.color.env.a;
        case G_ACMUX_0:
            return 0.;
    }
    return 0.;
}

float combineAlphaABD(vec4 x, int flag, float shadeAlpha)
{
    float ret = combineAlphaAny(x, flag, shadeAlpha);
    switch (flag)
    {
        case G_ACMUX_COMBINED:
            return x.a;
        case G_ACMUX_1:
            return 1.;
    }
    return ret;
}

float combineAlphaC(vec4 x, int flag, float shadeAlpha)
{
    float ret = combineAlphaAny(x, flag, shadeAlpha);
    switch (flag)
    {
        case G_ACMUX_LOD_FRACTION:
            //return 0.;
            return u_RdpState.color.primLod;
        // G_ACMUX_PRIM_LOD_FRAC 
    }
    return ret;
}



vec4 combineCycle(vec4 x, ivec4 cFlag, ivec4 aFlag, float shadeAlpha)
{
    vec4 a, b, c, d;

    a = vec4(combineA(x, cFlag.x), combineAlphaABD(x, aFlag.x, shadeAlpha));
    b = vec4(combineB(x, cFlag.y), combineAlphaABD(x, aFlag.y, shadeAlpha));
    c = vec4(combineC(x, cFlag.z, shadeAlpha), combineAlphaC(x, aFlag.z, shadeAlpha));
    d = vec4(combineD(x, cFlag.w), combineAlphaABD(x, aFlag.w, shadeAlpha));

    // Color Combine Formula
    return (a - b) * c + d;
}

// CC1 -> CC2 -> BL1 -> BL2
vec4 calcColor()
{
    vec4 x = vec4(0);

    float shadeAlpha = v_VtxColor.a;
    if ((u_RdpState.geoMode & G_FOG) != 0u)
        shadeAlpha = u_RdpState.color.fog.a;

    // CC1
    x = combineCycle(x, u_RdpState.combiner.c1, u_RdpState.combiner.a1, shadeAlpha);
    // CC2
    x = combineCycle(x, u_RdpState.combiner.c2, u_RdpState.combiner.a2, shadeAlpha);
    // BL1
    x = blendCycle(x, true, shadeAlpha);
    // BL2
    x = blendCycle(x, false, shadeAlpha);

    return x;
}


vec4 addHighlight(vec4 color)
{
    return (u_HighlightEnabled)
        ? mix(color, u_HighlightColor, vec4(u_HighlightColor.a))
        : color;
}

float getDiffuse(vec3 light)
{
    return max(0, dot(normalize(light), normalize(v_VtxNormal)));
}

vec4 addLighting(vec4 color)
{
    if (!u_LigthingEnabled)
        return color;

    float ambient = 0.5;
    float diffuse = getDiffuse(LigthFacing);

    return vec4(color.xyz * (diffuse + ambient), color.a);
}

/* Debugging */

vec4 debugDepth(vec4 color)
{
    float z = gl_FragCoord.z;
    z -= 0.999;
    z *= 1000.;
    z -= 0.4;
    return color * vec4(vec3(z), 1.0);
}

vec4 debugVertexId()
{
    if (v_VtxId % 3 == 0)
        return  Red;
    else if (v_VtxId % 3 == 1)
        return Green;
    else
        return Blue;
}


void main()
{
    if (u_ModelRenderMode == MODE_WIREFRAME)
    {
        FragColor = u_WireFrameColor;
    }
    else if (u_ModelRenderMode == MODE_SURFACE)
    {
        FragColor = vec4(1);
        FragColor = addLighting(FragColor);
    }
    else if (u_ModelRenderMode == MODE_TEXTURED)
    {
        
        FragColor = texel(0);

        /* lazy alpha check */
        if (FragColor.a < 0.1)
            discard;

            
        FragColor = calcColor();
    }
    else if (u_ModelRenderMode == MODE_NORMAL)
    {
        FragColor = v_VtxColor;
    }
    else // invalid mode
    {
        FragColor = vec4(1, 0, 0, 1);
    }

    /* highlight */
    FragColor = addHighlight(FragColor);
}