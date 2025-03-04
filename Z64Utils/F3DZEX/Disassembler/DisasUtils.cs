using System;
using System.Collections.Generic;
using System.IO;
using Common;
using F3DZEX.Command;

#nullable enable

namespace F3DZEX
{
    public partial class Disassembler
    {
        private class CCMode
        {
            public static readonly List<CCMode> Modes = new List<CCMode>()
            {
                new CCMode(
                    "G_CC_PRIMITIVE",
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_PRIMITIVE
                ),
                new CCMode(
                    "G_CC_SHADE",
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_MODULATEI",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_MODULATEIA",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_MODULATEIDECALA",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_MODULATEI_PRIM",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_PRIMITIVE
                ),
                new CCMode(
                    "G_CC_MODULATEIA_PRIM",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_PRIMITIVE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_MODULATEIDECALA_PRIM",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_DECALRGB",
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_DECALRGBA",
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_BLENDI",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_BLENDIA",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_BLENDIDECALA",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_BLENDRGBA",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0_ALPHA,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_BLENDRGBDECALA",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0_ALPHA,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_ADDRGB",
                    G_CCMUX.G_CCMUX_1,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_ADDRGBDECALA",
                    G_CCMUX.G_CCMUX_1,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_REFLECTRGB",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_REFLECTRGBDECALA",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_HILITERGB",
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_HILITERGBA",
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_PRIMITIVE,
                    G_ACMUX.G_ACMUX_SHADE,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_HILITERGBDECALA",
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_SHADEDECALA",
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_BLENDPE",
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_BLENDPEDECALA",
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "_G_CC_BLENDPE",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "_G_CC_BLENDPEDECALA",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "_G_CC_TWOCOLORTEX",
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "_G_CC_SPARSEST",
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_LOD_FRACTION,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_PRIMITIVE,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_LOD_FRACTION,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_TEMPLERP",
                    G_CCMUX.G_CCMUX_TEXEL1,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_PRIM_LOD_FRAC,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_TEXEL1,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_PRIM_LOD_FRAC,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_TRILERP",
                    G_CCMUX.G_CCMUX_TEXEL1,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_LOD_FRACTION,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_TEXEL1,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_LOD_FRACTION,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_INTERFERENCE",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_TEXEL1,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL1,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_1CYUV2RGB",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_K4,
                    G_CCMUX.G_CCMUX_K5,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_YUV2RGB",
                    G_CCMUX.G_CCMUX_TEXEL1,
                    G_CCMUX.G_CCMUX_K4,
                    G_CCMUX.G_CCMUX_K5,
                    G_CCMUX.G_CCMUX_TEXEL1,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_PASS2",
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_COMBINED
                ),
                new CCMode(
                    "G_CC_MODULATEI2",
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_MODULATEIA2",
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_MODULATEI_PRIM2",
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_PRIMITIVE
                ),
                new CCMode(
                    "G_CC_MODULATEIA_PRIM2",
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_PRIMITIVE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_PRIMITIVE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_DECALRGB2",
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_0,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_BLENDI2",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_BLENDIA2",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_SHADE,
                    G_ACMUX.G_ACMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_CHROMA_KEY2",
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_CENTER,
                    G_CCMUX.G_CCMUX_SCALE,
                    G_CCMUX.G_CCMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0
                ),
                new CCMode(
                    "G_CC_HILITERGB2",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_SHADE
                ),
                new CCMode(
                    "G_CC_HILITERGBA2",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_ACMUX.G_ACMUX_ENVIRONMENT,
                    G_ACMUX.G_ACMUX_COMBINED,
                    G_ACMUX.G_ACMUX_TEXEL0,
                    G_ACMUX.G_ACMUX_COMBINED
                ),
                new CCMode(
                    "G_CC_HILITERGBDECALA2",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_TEXEL0
                ),
                new CCMode(
                    "G_CC_HILITERGBPASSA2",
                    G_CCMUX.G_CCMUX_ENVIRONMENT,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_CCMUX.G_CCMUX_TEXEL0,
                    G_CCMUX.G_CCMUX_COMBINED,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_0,
                    G_ACMUX.G_ACMUX_COMBINED
                ),
            };

            public string Name;
            G_CCMUX a;
            G_CCMUX b;
            G_CCMUX c;
            G_CCMUX d;
            G_ACMUX Aa;
            G_ACMUX Ab;
            G_ACMUX Ac;
            G_ACMUX Ad;

            public CCMode(
                string name,
                G_CCMUX a,
                G_CCMUX b,
                G_CCMUX c,
                G_CCMUX d,
                G_ACMUX Aa,
                G_ACMUX Ab,
                G_ACMUX Ac,
                G_ACMUX Ad
            )
            {
                Name = name;
                this.a = a & (G_CCMUX)((1 << 4) - 1);
                this.b = b & (G_CCMUX)((1 << 4) - 1);
                this.c = c & (G_CCMUX)((1 << 5) - 1);
                this.d = d & (G_CCMUX)((1 << 3) - 1);
                this.Aa = Aa & (G_ACMUX)((1 << 3) - 1);
                this.Ab = Ab & (G_ACMUX)((1 << 3) - 1);
                this.Ac = Ac & (G_ACMUX)((1 << 3) - 1);
                this.Ad = Ad & (G_ACMUX)((1 << 3) - 1);
            }

            public bool Match(GSetCombine cmd, int cycle)
            {
                switch (cycle)
                {
                    case 0:
                        return cmd.a0 == a
                            && cmd.b0 == b
                            && cmd.c0 == c
                            && cmd.d0 == d
                            && cmd.Aa0 == Aa
                            && cmd.Ab0 == Ab
                            && cmd.Ac0 == Ac
                            && cmd.Ad0 == Ad;
                    case 1:
                        return cmd.a1 == a
                            && cmd.b1 == b
                            && cmd.c1 == c
                            && cmd.d1 == d
                            && cmd.Aa1 == Aa
                            && cmd.Ab1 == Ab
                            && cmd.Ac1 == Ac
                            && cmd.Ad1 == Ad;
                    default:
                        return false;
                }
            }
        }

        private class OtherModeMacro
        {
            public static readonly List<OtherModeMacro> MacrosL = new List<OtherModeMacro>()
            {
                new OtherModeMacro(
                    "SetAlphaCompare",
                    (int)G_MDSFT_L.G_MDSFT_ALPHACOMPARE,
                    2,
                    "G_AC_NONE",
                    0,
                    "G_AC_THRESHOLD",
                    1 << 0,
                    "G_AC_DITHER",
                    3 << 0
                ),
                new OtherModeMacro(
                    "SetDepthSource",
                    (int)G_MDSFT_L.G_MDSFT_ZSRCSEL,
                    1,
                    "G_ZS_PIXEL",
                    0,
                    "G_ZS_PRIM",
                    1 << 2
                ),
            };
            public static readonly List<OtherModeMacro> MacrosH = new List<OtherModeMacro>()
            {
                new OtherModeMacro(
                    "SetAlphaDither",
                    (int)G_MDSFT_H.G_MDSFT_ALPHADITHER,
                    2,
                    "G_AD_PATTERN",
                    0,
                    "G_AD_NOTPATTERN",
                    0x10,
                    "G_AD_NOISE",
                    0x20,
                    "G_AD_DISABLE",
                    0x30
                ),
                new OtherModeMacro(
                    "SetColorDither",
                    (int)G_MDSFT_H.G_MDSFT_RGBDITHER,
                    2,
                    "G_CD_MAGICSQ",
                    0,
                    "G_CD_BAYER",
                    0x40,
                    "G_CD_NOISE",
                    0x80
                ), // HW version 1
                new OtherModeMacro(
                    "SetCombineKey",
                    (int)G_MDSFT_H.G_MDSFT_COMBKEY,
                    1,
                    "G_CK_NONE",
                    0,
                    "G_CK_KEY",
                    1 << 8
                ),
                new OtherModeMacro(
                    "SetTextureConvert",
                    (int)G_MDSFT_H.G_MDSFT_TEXTCONV,
                    3,
                    "G_TC_CONV",
                    0,
                    "G_TC_FILTCONV",
                    5 << 9,
                    "G_TC_FILT",
                    6 << 9
                ),
                new OtherModeMacro(
                    "SetTextureFilter",
                    (int)G_MDSFT_H.G_MDSFT_TEXTFILT,
                    2,
                    "G_TF_POINT",
                    0,
                    "G_TF_AVERAGE",
                    3 << 12,
                    "G_TF_BILERP",
                    2 << 12
                ),
                new OtherModeMacro(
                    "SetTextureLUT",
                    (int)G_MDSFT_H.G_MDSFT_TEXTLUT,
                    2,
                    "G_TT_NONE",
                    0,
                    "G_TT_RGBA16",
                    2 << 14,
                    "G_TT_IA16",
                    3 << 14
                ),
                new OtherModeMacro(
                    "SetTextureLOD",
                    (int)G_MDSFT_H.G_MDSFT_TEXTLOD,
                    1,
                    "G_TL_TILE",
                    0,
                    "G_TL_LOD",
                    1 << 16
                ),
                new OtherModeMacro(
                    "SetTextureDetail",
                    (int)G_MDSFT_H.G_MDSFT_TEXTDETAIL,
                    2,
                    "G_TD_CLAMP",
                    0,
                    "G_TD_SHARPEN",
                    1 << 17,
                    "G_TD_DETAIL",
                    2 << 17
                ),
                new OtherModeMacro(
                    "SetTexturePersp",
                    (int)G_MDSFT_H.G_MDSFT_TEXTPERSP,
                    1,
                    "G_TP_PERSP",
                    1 << 19,
                    "G_TP_NONE",
                    0 << 19
                ),
                new OtherModeMacro(
                    "SetCycleType",
                    (int)G_MDSFT_H.G_MDSFT_CYCLETYPE,
                    2,
                    "G_CYC_1CYCLE",
                    0,
                    "G_CYC_2CYCLE",
                    1 << 20,
                    "G_CYC_COPY",
                    2 << 20,
                    "G_CYC_FILL",
                    3 << 20
                ),
                new OtherModeMacro(
                    "SetCycleType",
                    (int)G_MDSFT_H.G_MDSFT_CYCLETYPE,
                    2,
                    "G_CYC_1CYCLE",
                    0,
                    "G_CYC_2CYCLE",
                    1 << 20,
                    "G_CYC_COPY",
                    2 << 20,
                    "G_CYC_FILL",
                    3 << 20
                ),
                new OtherModeMacro(
                    "SetColorDither",
                    (int)G_MDSFT_H.G_MDSFT_COLORDITHER,
                    2,
                    "G_CD_MAGICSQ",
                    0,
                    "G_CD_BAYER",
                    0x40,
                    "G_CD_NOISE",
                    0x80
                ),
                new OtherModeMacro(
                    "PipelineMode",
                    (int)G_MDSFT_H.G_MDSFT_PIPELINE,
                    1,
                    "G_PM_1PRIMITIVE",
                    1 << 23,
                    "G_PM_NPRIMITIVE",
                    0
                ),
            };

            public string Name;
            int shift;
            int len;
            public List<Tuple<string, int>> values;

            public OtherModeMacro(string name, int shift, int len, List<Tuple<string, int>> values)
            {
                Name = name;
                this.shift = shift;
                this.len = len;
                this.values = values;
            }

            public OtherModeMacro(
                string name,
                int shift,
                int len,
                string name0 = "-1",
                int value0 = -1,
                string name1 = "-1",
                int value1 = -1,
                string name2 = "-1",
                int value2 = -1,
                string name3 = "-1",
                int value3 = -1
            )
                : this(
                    name,
                    shift,
                    len,
                    new List<Tuple<string, int>>()
                    {
                        new Tuple<string, int>(name0, value0),
                        new Tuple<string, int>(name1, value1),
                        new Tuple<string, int>(name2, value2),
                        new Tuple<string, int>(name3, value3),
                    }
                ) { }

            private static readonly BitFlag _renderModeFlags = new BitFlag(
                new BitFlag.BoolField("AA_EN", 0),
                new BitFlag.BoolField("Z_CMP", 1),
                new BitFlag.BoolField("Z_UPD", 2),
                new BitFlag.BoolField("IM_RD", 3),
                new BitFlag.BoolField("CLR_ON_CVG", 4),
                BitFlag.EnumField.FromBits(
                    5,
                    "CVG_DST_CLAMP",
                    "CVG_DST_WRAP",
                    "CVG_DST_FULL",
                    "CVG_DST_SAVE"
                ),
                BitFlag.EnumField.FromBits(7, "ZMODE_OPA", "ZMODE_INTER", "ZMODE_XLU", "ZMODE_DEC"),
                new BitFlag.BoolField("CVG_X_ALPHA", 9),
                new BitFlag.BoolField("ALPHA_CVG_SEL", 10),
                new BitFlag.BoolField("FORCE_BL", 11)
            );

            public bool Match(GSetOtherMode cmd)
            {
                return cmd.shift == shift && cmd.len == len;
            }

            private static Tuple<string[], string[]> GetCycleDepRenderFlags(uint x)
            {
                string[] pmValues = new string[]
                {
                    "G_BL_CLR_IN",
                    "G_BL_CLR_MEM",
                    "G_BL_CLR_BL",
                    "G_BL_CLR_FOG",
                };

                string[] aValues = new string[]
                {
                    "G_BL_A_IN",
                    "G_BL_A_FOG",
                    "G_BL_A_SHADE",
                    "G_BL_0",
                };

                string[] bValues = new string[] { "G_BL_1MA", "G_BL_A_MEM", "G_BL_1", "G_BL_0" };

                string p1 = pmValues[x >> 14 & 3];
                string a1 = aValues[x >> 10 & 3];
                string m1 = pmValues[x >> 6 & 3];
                string b1 = bValues[x >> 2 & 3];

                string p2 = pmValues[x >> 12 & 3];
                string a2 = aValues[x >> 8 & 3];
                string m2 = pmValues[x >> 4 & 3];
                string b2 = bValues[x >> 0 & 3];

                return new Tuple<string[], string[]>(
                    new string[] { p1, a1, m1, b1 },
                    new string[] { p2, a2, m2, b2 }
                );
            }

            public static (string, List<string>) DisasRenderMode(GSetOtherMode cmd)
            {
                uint cyclIndep = BitUtils.GetBits(cmd.data, 3, 13);
                uint cyclDep = BitUtils.GetBits(cmd.data, 16, 16);

                List<string> flags = _renderModeFlags.GetFlags(cyclIndep);
                var depFlags = GetCycleDepRenderFlags(cyclDep);
                string blc1 = $"GBL_c1({string.Join(", ", depFlags.Item1)})";
                string blc2 = $"GBL_c2({string.Join(", ", depFlags.Item2)})";

                flags.Insert(0, blc2);

                string c1 = blc1;
                string c2 = string.Join(" | ", flags);
                //flags.Add($"0x{cyclDep:X}<<16");

                // (P * A + M * B) / (A + B)
                Func<string[], string> formatExpr = (blc) =>
                {
                    string p = blc[0].Remove(0, "G_BL_".Length);
                    string a = blc[1].Remove(0, "G_BL_".Length);
                    string m = blc[2].Remove(0, "G_BL_".Length);
                    string b = blc[3].Remove(0, "G_BL_".Length);

                    if (a == "0")
                        return m;

                    if (b == "0")
                        return p;

                    if (b == "1MA")
                        return $"({p} * {a} + {m} * (1 - {a}))";

                    return $"({p} * {a} + {m} * {b}) / ({a} + {b})";
                };

                List<string> comments = new List<string>()
                {
                    $"BL1: {formatExpr(depFlags.Item1)}",
                    $"BL2: {formatExpr(depFlags.Item2)}",
                };

                return ($"gsDPSetRenderMode({c1}, {c2})", comments);
            }
        }

        // These 2 functions are reimps of oot's code
        private static string DisCCM(G_CCMUX value, int idx)
        {
            switch (value)
            {
                case G_CCMUX.G_CCMUX_COMBINED:
                case G_CCMUX.G_CCMUX_TEXEL0:
                case G_CCMUX.G_CCMUX_TEXEL1:
                case G_CCMUX.G_CCMUX_PRIMITIVE:
                case G_CCMUX.G_CCMUX_SHADE:
                case G_CCMUX.G_CCMUX_ENVIRONMENT:
                    return value.ToString().Replace("G_CCMUX_", "");
                case (G_CCMUX)6:
                    return (idx == 2) ? "CENTER"
                        : (idx == 3) ? "SCALE"
                        : "1";
                case (G_CCMUX)7:
                    return (idx == 1) ? "NOISE"
                        : (idx == 2) ? "K4"
                        : (idx == 3) ? "COMBINED_ALPHA"
                        : "0";
                default:
                {
                    if (idx == 3)
                    {
                        switch (value)
                        {
                            case G_CCMUX.G_CCMUX_TEXEL0_ALPHA:
                            case G_CCMUX.G_CCMUX_TEXEL1_ALPHA:
                            case G_CCMUX.G_CCMUX_PRIMITIVE_ALPHA:
                            case G_CCMUX.G_CCMUX_SHADE_ALPHA:
                            case G_CCMUX.G_CCMUX_ENV_ALPHA:
                            case G_CCMUX.G_CCMUX_LOD_FRACTION:
                            case G_CCMUX.G_CCMUX_PRIM_LOD_FRAC:
                            case G_CCMUX.G_CCMUX_K5:
                                return value.ToString().Replace("G_CCMUX_", "");
                            default:
                                return "0";
                        }
                    }
                    else
                        return "0";
                }
            }
        }

        private static string DisACM(G_ACMUX value, int idx)
        {
            switch (value)
            {
                case 0:
                    return (idx == 3) ? "LOD_FRACTION" : "COMBINED";

                case G_ACMUX.G_ACMUX_TEXEL0:
                case G_ACMUX.G_ACMUX_TEXEL1:
                case G_ACMUX.G_ACMUX_PRIMITIVE:
                case G_ACMUX.G_ACMUX_SHADE:
                case G_ACMUX.G_ACMUX_ENVIRONMENT:
                case G_ACMUX.G_ACMUX_0:
                    return value.ToString().Replace("G_ACMUX_", "");
                case (G_ACMUX)6:
                    return (idx == 3) ? "PRIM_LOD_FRAC" : "1";
                default:
                    return "?";
            }
        }

        private static string DisMtxParams(int v)
        {
            G_MTX_PARAM param = (G_MTX_PARAM)v;
            string push = param.HasFlag(G_MTX_PARAM.G_MTX_PUSH) ? "G_MTX_PUSH" : "G_MTX_NOPUSH";
            string load = param.HasFlag(G_MTX_PARAM.G_MTX_LOAD) ? "G_MTX_LOAD" : "G_MTX_MUL";
            string projection = param.HasFlag(G_MTX_PARAM.G_MTX_PROJECTION)
                ? "G_MTX_PROJECTION"
                : "G_MTX_MODELVIEW";
            return $"{push} | {load} | {projection}";
        }

        private static string DisTexWrap(G_TX_TEXWRAP v)
        {
            var mirror = v.HasFlag(G_TX_TEXWRAP.G_TX_MIRROR) ? "G_TX_MIRROR" : "G_TX_NOMIRROR";
            var wrap = v.HasFlag(G_TX_TEXWRAP.G_TX_CLAMP) ? "G_TX_CLAMP" : "G_TX_WRAP";
            return $"{mirror} | {wrap}";
        }

        private static string DisTexWrap(int v)
        {
            return DisTexWrap((G_TX_TEXWRAP)v);
        }
    }
}
