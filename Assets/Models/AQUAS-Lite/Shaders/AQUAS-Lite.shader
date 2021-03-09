// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

/*SF_DATA;ver:1.21;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:1,spmd:0,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:0,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False;n:type:ShaderForge.SFN_Final,id:4013,x:35020,y:33389,varname:node_4013,prsc:2|diff-2391-OUT,spec-3031-OUT,gloss-5005-OUT,normal-6439-OUT,lwrap-8103-OUT,alpha-1886-OUT,refract-6291-OUT;n:type:ShaderForge.SFN_DepthBlend,id:1214,x:34327,y:34009,varname:node_1214,prsc:2|DIST-2877-OUT;n:type:ShaderForge.SFN_Multiply,id:5923,x:32792,y:32706,varname:node_5923,prsc:2|A-6486-RGB,B-3380-OUT;n:type:ShaderForge.SFN_SceneColor,id:6486,x:32569,y:32661,varname:node_6486,prsc:2;n:type:ShaderForge.SFN_Power,id:3380,x:32569,y:32795,varname:node_3380,prsc:2|VAL-2388-OUT,EXP-1198-OUT;n:type:ShaderForge.SFN_Clamp01,id:2388,x:32377,y:32738,varname:node_2388,prsc:2|IN-2891-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1198,x:32377,y:32899,ptovrint:True,ptlb:Fade,ptin:_Fade,varname:_Fade,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1.45;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:2891,x:32197,y:32738,varname:node_2891,prsc:2|IN-2055-OUT,IMIN-6686-OUT,IMAX-7248-OUT,OMIN-3685-OUT,OMAX-1633-OUT;n:type:ShaderForge.SFN_Add,id:7248,x:31956,y:32864,varname:node_7248,prsc:2|A-2499-OUT,B-3337-OUT;n:type:ShaderForge.SFN_Vector1,id:3685,x:31956,y:33033,varname:node_3685,prsc:2,v1:1;n:type:ShaderForge.SFN_Vector1,id:1633,x:31956,y:33143,varname:node_1633,prsc:2,v1:0.5;n:type:ShaderForge.SFN_Subtract,id:2055,x:31956,y:32555,varname:node_2055,prsc:2|A-8281-OUT,B-8872-OUT;n:type:ShaderForge.SFN_SceneDepth,id:8281,x:31758,y:32532,varname:node_8281,prsc:2;n:type:ShaderForge.SFN_Depth,id:8872,x:31758,y:32665,varname:node_8872,prsc:2;n:type:ShaderForge.SFN_Color,id:5957,x:31566,y:32665,ptovrint:True,ptlb:Main Color,ptin:_MainColor,varname:_MainColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.4627451,c3:1,c4:1;n:type:ShaderForge.SFN_Multiply,id:2499,x:31758,y:32808,varname:node_2499,prsc:2|A-5957-RGB,B-5924-OUT;n:type:ShaderForge.SFN_Vector1,id:3337,x:31758,y:32950,varname:node_3337,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:6398,x:31387,y:32848,varname:node_6398,prsc:2,v1:10;n:type:ShaderForge.SFN_Slider,id:992,x:31230,y:32933,ptovrint:True,ptlb:Density,ptin:_Density,varname:_Density,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.74,max:10;n:type:ShaderForge.SFN_Divide,id:5924,x:31566,y:32882,varname:node_5924,prsc:2|A-6398-OUT,B-992-OUT;n:type:ShaderForge.SFN_Vector3,id:6686,x:31956,y:32725,varname:node_6686,prsc:2,v1:0,v2:0,v3:0;n:type:ShaderForge.SFN_Color,id:1253,x:32792,y:32495,ptovrint:True,ptlb:Deep Water Color,ptin:_DeepWaterColor,varname:_DeepWaterColor,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0,c2:0.3411765,c3:0.6235294,c4:1;n:type:ShaderForge.SFN_Blend,id:2072,x:33013,y:32605,varname:node_2072,prsc:2,blmd:8,clmp:True|SRC-1253-RGB,DST-5923-OUT;n:type:ShaderForge.SFN_ScreenPos,id:796,x:32031,y:31962,varname:node_796,prsc:2,sctp:0;n:type:ShaderForge.SFN_Lerp,id:5011,x:33436,y:32440,varname:node_5011,prsc:2|A-2696-RGB,B-2072-OUT,T-7195-OUT;n:type:ShaderForge.SFN_Slider,id:3280,x:32685,y:32392,ptovrint:False,ptlb:Reflection Intensity,ptin:_ReflectionIntensity,varname:node_3280,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.6,max:1;n:type:ShaderForge.SFN_OneMinus,id:7195,x:33013,y:32392,varname:node_7195,prsc:2|IN-3280-OUT;n:type:ShaderForge.SFN_SwitchProperty,id:7912,x:33656,y:32598,ptovrint:False,ptlb:Enable Reflections,ptin:_EnableReflections,varname:node_7912,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,on:True|A-2072-OUT,B-5011-OUT;n:type:ShaderForge.SFN_Tex2d,id:2696,x:32842,y:32208,ptovrint:False,ptlb:Reflection Tex,ptin:_ReflectionTex,varname:node_2696,prsc:2,glob:False,taghide:True,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5462-OUT;n:type:ShaderForge.SFN_DepthBlend,id:2496,x:33450,y:32914,varname:node_2496,prsc:2|DIST-6334-OUT;n:type:ShaderForge.SFN_ValueProperty,id:6334,x:33273,y:32914,ptovrint:False,ptlb:Foam Blend,ptin:_FoamBlend,varname:node_6334,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.15;n:type:ShaderForge.SFN_RemapRange,id:2434,x:33652,y:32914,varname:node_2434,prsc:2,frmn:0,frmx:1,tomn:1,tomx:0|IN-2496-OUT;n:type:ShaderForge.SFN_Tex2d,id:8133,x:32449,y:33126,varname:node_8133,prsc:2,ntxv:0,isnm:False|UVIN-4290-OUT,TEX-668-TEX;n:type:ShaderForge.SFN_Multiply,id:5136,x:33833,y:32914,varname:node_5136,prsc:2|A-2434-OUT,B-6900-OUT;n:type:ShaderForge.SFN_ValueProperty,id:1887,x:33273,y:33340,ptovrint:False,ptlb:Foam Intensity,ptin:_FoamIntensity,varname:node_1887,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:10;n:type:ShaderForge.SFN_Color,id:6064,x:33273,y:33156,ptovrint:False,ptlb:Foam Color,ptin:_FoamColor,varname:node_6064,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.3382353,c2:0.3456964,c3:0.3456964,c4:1;n:type:ShaderForge.SFN_Multiply,id:3918,x:33450,y:33070,varname:node_3918,prsc:2|A-6325-OUT,B-6064-RGB;n:type:ShaderForge.SFN_Lerp,id:2391,x:34217,y:32890,varname:node_2391,prsc:2|A-7912-OUT,B-4466-OUT,T-3617-OUT;n:type:ShaderForge.SFN_Slider,id:3617,x:33833,y:33071,ptovrint:False,ptlb:Foam Visibility,ptin:_FoamVisibility,varname:node_3617,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3,max:1;n:type:ShaderForge.SFN_Desaturate,id:7689,x:33032,y:32927,varname:node_7689,prsc:2|COL-9137-OUT;n:type:ShaderForge.SFN_RemapRangeAdvanced,id:6325,x:33273,y:32998,varname:node_6325,prsc:2|IN-7689-OUT,IMIN-5180-OUT,IMAX-1540-OUT,OMIN-9363-OUT,OMAX-4711-OUT;n:type:ShaderForge.SFN_Slider,id:5180,x:32860,y:33128,ptovrint:False,ptlb:Foam Contrast,ptin:_FoamContrast,varname:node_5180,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.25,max:0.5;n:type:ShaderForge.SFN_OneMinus,id:1540,x:33017,y:33221,varname:node_1540,prsc:2|IN-5180-OUT;n:type:ShaderForge.SFN_Vector1,id:9363,x:33017,y:33369,varname:node_9363,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:4711,x:33017,y:33432,varname:node_4711,prsc:2,v1:1;n:type:ShaderForge.SFN_Multiply,id:6900,x:33652,y:33126,varname:node_6900,prsc:2|A-3918-OUT,B-3110-OUT;n:type:ShaderForge.SFN_Multiply,id:4466,x:33990,y:32914,varname:node_4466,prsc:2|A-5136-OUT,B-5136-OUT;n:type:ShaderForge.SFN_Vector1,id:3790,x:33273,y:33411,varname:node_3790,prsc:2,v1:-1;n:type:ShaderForge.SFN_Multiply,id:3110,x:33450,y:33224,varname:node_3110,prsc:2|A-1887-OUT,B-3790-OUT;n:type:ShaderForge.SFN_Tex2dAsset,id:668,x:32195,y:32982,ptovrint:False,ptlb:Foam Texture,ptin:_FoamTexture,varname:node_668,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:3613,x:32449,y:33293,varname:node_3613,prsc:2,ntxv:0,isnm:False|UVIN-8960-OUT,TEX-668-TEX;n:type:ShaderForge.SFN_Subtract,id:9137,x:32644,y:33220,varname:node_9137,prsc:2|A-8133-RGB,B-3613-RGB;n:type:ShaderForge.SFN_TexCoord,id:6141,x:31349,y:33064,varname:node_6141,prsc:2,uv:0;n:type:ShaderForge.SFN_Rotator,id:6960,x:31743,y:33093,varname:node_6960,prsc:2|UVIN-6141-UVOUT,ANG-1944-OUT;n:type:ShaderForge.SFN_Vector1,id:1944,x:31349,y:33202,varname:node_1944,prsc:2,v1:1.5708;n:type:ShaderForge.SFN_ObjectScale,id:8796,x:31349,y:33586,varname:node_8796,prsc:2,rcp:False;n:type:ShaderForge.SFN_ComponentMask,id:7078,x:31540,y:33586,varname:node_7078,prsc:2,cc1:0,cc2:2,cc3:-1,cc4:-1|IN-8796-XYZ;n:type:ShaderForge.SFN_ValueProperty,id:7092,x:31540,y:33770,ptovrint:False,ptlb:Foam Tiling,ptin:_FoamTiling,varname:node_7092,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:3;n:type:ShaderForge.SFN_Multiply,id:2878,x:31748,y:33657,varname:node_2878,prsc:2|A-7078-OUT,B-7092-OUT;n:type:ShaderForge.SFN_Multiply,id:4290,x:32178,y:33205,varname:node_4290,prsc:2|A-4294-OUT,B-9910-OUT;n:type:ShaderForge.SFN_Multiply,id:8960,x:32178,y:33351,varname:node_8960,prsc:2|A-1774-OUT,B-9910-OUT;n:type:ShaderForge.SFN_Append,id:4592,x:31349,y:33259,varname:node_4592,prsc:2|A-2605-OUT,B-3017-OUT;n:type:ShaderForge.SFN_Multiply,id:4993,x:31518,y:33338,varname:node_4993,prsc:2|A-4592-OUT,B-8982-OUT;n:type:ShaderForge.SFN_Time,id:6613,x:31152,y:33365,varname:node_6613,prsc:2;n:type:ShaderForge.SFN_Vector1,id:3017,x:31152,y:33315,varname:node_3017,prsc:2,v1:0;n:type:ShaderForge.SFN_ValueProperty,id:5272,x:30967,y:33179,ptovrint:False,ptlb:Foam Speed,ptin:_FoamSpeed,varname:node_5272,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:120;n:type:ShaderForge.SFN_Divide,id:2605,x:31152,y:33179,varname:node_2605,prsc:2|A-5272-OUT,B-9910-OUT;n:type:ShaderForge.SFN_Vector1,id:2220,x:31152,y:33491,varname:node_2220,prsc:2,v1:100;n:type:ShaderForge.SFN_Divide,id:8982,x:31349,y:33419,varname:node_8982,prsc:2|A-6613-TSL,B-2220-OUT;n:type:ShaderForge.SFN_Add,id:4294,x:31956,y:33205,varname:node_4294,prsc:2|A-6960-UVOUT,B-4993-OUT;n:type:ShaderForge.SFN_Add,id:1774,x:31956,y:33351,varname:node_1774,prsc:2|A-6141-UVOUT,B-4993-OUT;n:type:ShaderForge.SFN_Tex2d,id:470,x:33863,y:33455,varname:node_470,prsc:2,ntxv:0,isnm:False|UVIN-8041-OUT,TEX-1511-TEX;n:type:ShaderForge.SFN_ValueProperty,id:8651,x:34301,y:33099,ptovrint:False,ptlb:Specular,ptin:_Specular,varname:node_8651,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_ValueProperty,id:5005,x:34641,y:33380,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:node_5005,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.7;n:type:ShaderForge.SFN_Color,id:5329,x:34301,y:33181,ptovrint:False,ptlb:Specular Color,ptin:_SpecularColor,varname:node_5329,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.6985294,c2:0.7088019,c3:0.7088019,c4:1;n:type:ShaderForge.SFN_Multiply,id:3031,x:34528,y:33280,varname:node_3031,prsc:2|A-8651-OUT,B-5329-RGB;n:type:ShaderForge.SFN_Slider,id:2808,x:33934,y:33769,ptovrint:False,ptlb:Refraction,ptin:_Refraction,varname:node_2808,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.67,max:1;n:type:ShaderForge.SFN_Lerp,id:6439,x:34641,y:33450,varname:node_6439,prsc:2|A-2592-OUT,B-6972-OUT,T-2808-OUT;n:type:ShaderForge.SFN_Vector3,id:2592,x:34366,y:33406,varname:node_2592,prsc:2,v1:0,v2:0,v3:1;n:type:ShaderForge.SFN_Tex2dAsset,id:1511,x:33602,y:33295,ptovrint:False,ptlb:Normal Texture,ptin:_NormalTexture,varname:node_1511,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:True,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9575,x:33863,y:33585,varname:node_9575,prsc:2,ntxv:0,isnm:False|UVIN-8973-OUT,TEX-1511-TEX;n:type:ShaderForge.SFN_TexCoord,id:1981,x:32895,y:33493,varname:node_1981,prsc:2,uv:0;n:type:ShaderForge.SFN_Rotator,id:3506,x:33165,y:33496,varname:node_3506,prsc:2|UVIN-1981-UVOUT,ANG-8109-OUT;n:type:ShaderForge.SFN_Vector1,id:8109,x:32895,y:33639,varname:node_8109,prsc:2,v1:1.5708;n:type:ShaderForge.SFN_Subtract,id:6972,x:34071,y:33517,varname:node_6972,prsc:2|A-470-RGB,B-9575-RGB;n:type:ShaderForge.SFN_ValueProperty,id:8103,x:34783,y:33576,ptovrint:False,ptlb:Light Wrapping,ptin:_LightWrapping,varname:node_8103,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:2;n:type:ShaderForge.SFN_ObjectScale,id:1443,x:32717,y:34049,varname:node_1443,prsc:2,rcp:False;n:type:ShaderForge.SFN_ComponentMask,id:3806,x:32895,y:34049,varname:node_3806,prsc:2,cc1:0,cc2:2,cc3:-1,cc4:-1|IN-1443-XYZ;n:type:ShaderForge.SFN_ValueProperty,id:6926,x:32717,y:34232,ptovrint:False,ptlb:Normal Tiling,ptin:_NormalTiling,varname:node_6926,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1;n:type:ShaderForge.SFN_Multiply,id:7753,x:33103,y:34132,varname:node_7753,prsc:2|A-3806-OUT,B-6926-OUT;n:type:ShaderForge.SFN_Multiply,id:8041,x:33602,y:33455,varname:node_8041,prsc:2|A-1797-OUT,B-3558-OUT;n:type:ShaderForge.SFN_Multiply,id:8973,x:33602,y:33585,varname:node_8973,prsc:2|A-4985-OUT,B-3558-OUT;n:type:ShaderForge.SFN_Add,id:1797,x:33368,y:33666,varname:node_1797,prsc:2|A-3506-UVOUT,B-1163-OUT;n:type:ShaderForge.SFN_Add,id:4985,x:33368,y:33802,varname:node_4985,prsc:2|A-1981-UVOUT,B-1163-OUT;n:type:ShaderForge.SFN_Append,id:8275,x:32895,y:33699,varname:node_8275,prsc:2|A-734-OUT,B-3935-OUT;n:type:ShaderForge.SFN_Time,id:6304,x:32717,y:33826,varname:node_6304,prsc:2;n:type:ShaderForge.SFN_Multiply,id:1163,x:33064,y:33765,varname:node_1163,prsc:2|A-8275-OUT,B-5349-OUT;n:type:ShaderForge.SFN_Divide,id:5349,x:32895,y:33826,varname:node_5349,prsc:2|A-6304-TSL,B-6186-OUT;n:type:ShaderForge.SFN_Vector1,id:6186,x:32717,y:33982,varname:node_6186,prsc:2,v1:100;n:type:ShaderForge.SFN_ValueProperty,id:2063,x:32533,y:33630,ptovrint:False,ptlb:Wave Speed,ptin:_WaveSpeed,varname:node_2063,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:40;n:type:ShaderForge.SFN_Divide,id:734,x:32717,y:33630,varname:node_734,prsc:2|A-2063-OUT,B-3558-OUT;n:type:ShaderForge.SFN_Vector1,id:3935,x:32717,y:33753,varname:node_3935,prsc:2,v1:0;n:type:ShaderForge.SFN_Vector1,id:523,x:34091,y:33858,varname:node_523,prsc:2,v1:0.2;n:type:ShaderForge.SFN_Multiply,id:7976,x:34327,y:33814,varname:node_7976,prsc:2|A-2808-OUT,B-523-OUT;n:type:ShaderForge.SFN_ComponentMask,id:1185,x:34254,y:33561,varname:node_1185,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-6972-OUT;n:type:ShaderForge.SFN_Multiply,id:2312,x:34531,y:33667,varname:node_2312,prsc:2|A-1185-OUT,B-7976-OUT;n:type:ShaderForge.SFN_OneMinus,id:7470,x:32359,y:32379,varname:node_7470,prsc:2|IN-796-V;n:type:ShaderForge.SFN_Append,id:7938,x:32516,y:32365,varname:node_7938,prsc:2|A-796-U,B-7470-OUT;n:type:ShaderForge.SFN_ValueProperty,id:2877,x:34091,y:34009,ptovrint:False,ptlb:Depth Transparency,ptin:_DepthTransparency,varname:node_2877,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:1.5;n:type:ShaderForge.SFN_ValueProperty,id:7272,x:34327,y:34169,ptovrint:False,ptlb:Shore Fade,ptin:_ShoreFade,varname:node_7272,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.3;n:type:ShaderForge.SFN_ValueProperty,id:1933,x:34327,y:34267,ptovrint:False,ptlb:Shore Transparency,ptin:_ShoreTransparency,varname:node_1933,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,v1:0.04;n:type:ShaderForge.SFN_Power,id:4692,x:34529,y:34067,varname:node_4692,prsc:2|VAL-1214-OUT,EXP-7272-OUT;n:type:ShaderForge.SFN_DepthBlend,id:4946,x:34529,y:34233,varname:node_4946,prsc:2|DIST-1933-OUT;n:type:ShaderForge.SFN_Multiply,id:1886,x:34727,y:34155,varname:node_1886,prsc:2|A-4692-OUT,B-4946-OUT;n:type:ShaderForge.SFN_RemapRange,id:5462,x:32425,y:32137,varname:node_5462,prsc:2,frmn:-1,frmx:1,tomn:0,tomx:1|IN-5686-OUT;n:type:ShaderForge.SFN_Add,id:5686,x:32222,y:32137,varname:node_5686,prsc:2|A-796-UVOUT,B-4053-OUT;n:type:ShaderForge.SFN_Multiply,id:4053,x:32031,y:32137,varname:node_4053,prsc:2|A-2527-OUT,B-3306-OUT;n:type:ShaderForge.SFN_Append,id:2527,x:31848,y:32137,varname:node_2527,prsc:2|A-2569-R,B-2569-G;n:type:ShaderForge.SFN_ComponentMask,id:2569,x:31656,y:32137,varname:node_2569,prsc:2,cc1:0,cc2:1,cc3:-1,cc4:-1|IN-6972-OUT;n:type:ShaderForge.SFN_Slider,id:3306,x:31679,y:32324,ptovrint:False,ptlb:Distortion,ptin:_Distortion,varname:node_3306,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3,max:2;n:type:ShaderForge.SFN_Divide,id:3558,x:33308,y:34132,varname:node_3558,prsc:2|A-7753-OUT,B-3449-OUT;n:type:ShaderForge.SFN_Vector1,id:3449,x:33103,y:34283,varname:node_3449,prsc:2,v1:1000;n:type:ShaderForge.SFN_Divide,id:9910,x:31939,y:33657,varname:node_9910,prsc:2|A-2878-OUT,B-8482-OUT;n:type:ShaderForge.SFN_Vector1,id:8482,x:31714,y:33827,varname:node_8482,prsc:2,v1:1000;n:type:ShaderForge.SFN_Multiply,id:6291,x:34755,y:33762,varname:node_6291,prsc:2|A-2312-OUT,B-1886-OUT;proporder:1511-6926-5957-1253-1198-992-2877-7272-1933-7912-2696-3280-3306-8651-5329-5005-8103-2808-2063-668-7092-6334-3617-1887-5180-6064-5272;pass:END;sub:END;*/

Shader "AQUAS-Lite" {
    Properties {
        [NoScaleOffset]_NormalTexture ("Normal Texture", 2D) = "white" {}
        _NormalTiling ("Normal Tiling", Float ) = 1
        _MainColor ("Main Color", Color) = (0,0.4627451,1,1)
        _DeepWaterColor ("Deep Water Color", Color) = (0,0.3411765,0.6235294,1)
        _Fade ("Fade", Float ) = 1.45
        _Density ("Density", Range(0, 10)) = 1.74
        _DepthTransparency ("Depth Transparency", Float ) = 1.5
        _ShoreFade ("Shore Fade", Float ) = 0.3
        _ShoreTransparency ("Shore Transparency", Float ) = 0.04
        [MaterialToggle] _EnableReflections ("Enable Reflections", Float ) = 0.6
        [HideInInspector]_ReflectionTex ("Reflection Tex", 2D) = "white" {}
        _ReflectionIntensity ("Reflection Intensity", Range(0, 1)) = 0.6
        _Distortion ("Distortion", Range(0, 2)) = 0.3
        _Specular ("Specular", Float ) = 1
        _SpecularColor ("Specular Color", Color) = (0.6985294,0.7088019,0.7088019,1)
        _Gloss ("Gloss", Float ) = 0.7
        _LightWrapping ("Light Wrapping", Float ) = 2
        _Refraction ("Refraction", Range(0, 1)) = 0.67
        _WaveSpeed ("Wave Speed", Float ) = 40
        [NoScaleOffset]_FoamTexture ("Foam Texture", 2D) = "white" {}
        _FoamTiling ("Foam Tiling", Float ) = 3
        _FoamBlend ("Foam Blend", Float ) = 0.15
        _FoamVisibility ("Foam Visibility", Range(0, 1)) = 0.3
        _FoamIntensity ("Foam Intensity", Float ) = 10
        _FoamContrast ("Foam Contrast", Range(0, 0.5)) = 0.25
        _FoamColor ("Foam Color", Color) = (0.3382353,0.3456964,0.3456964,1)
        _FoamSpeed ("Foam Speed", Float ) = 120
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        GrabPass{ "Refraction" }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma exclude_renderers d3d9 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D Refraction;
            uniform sampler2D _CameraDepthTexture;
            uniform float4 _TimeEditor;
            uniform float _Fade;
            uniform float4 _MainColor;
            uniform float _Density;
            uniform float4 _DeepWaterColor;
            uniform float _ReflectionIntensity;
            uniform fixed _EnableReflections;
            uniform sampler2D _ReflectionTex; uniform float4 _ReflectionTex_ST;
            uniform float _FoamBlend;
            uniform float _FoamIntensity;
            uniform float4 _FoamColor;
            uniform float _FoamVisibility;
            uniform float _FoamContrast;
            uniform sampler2D _FoamTexture; uniform float4 _FoamTexture_ST;
            uniform float _FoamTiling;
            uniform float _FoamSpeed;
            uniform float _Specular;
            uniform float _Gloss;
            uniform float4 _SpecularColor;
            uniform float _Refraction;
            uniform sampler2D _NormalTexture; uniform float4 _NormalTexture_ST;
            uniform float _LightWrapping;
            uniform float _NormalTiling;
            uniform float _WaveSpeed;
            uniform float _DepthTransparency;
            uniform float _ShoreFade;
            uniform float _ShoreTransparency;
            uniform float _Distortion;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                float4 screenPos : TEXCOORD5;
                float4 projPos : TEXCOORD6;
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                o.projPos = ComputeScreenPos (o.pos);
                COMPUTE_EYEDEPTH(o.projPos.z);
                o.screenPos = o.pos;
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
                #if UNITY_UV_STARTS_AT_TOP
                    float grabSign = -_ProjectionParams.x;
                #else
                    float grabSign = _ProjectionParams.x;
                #endif
                i.screenPos = float4( i.screenPos.xy / i.screenPos.w, 0, 0 );
                i.screenPos.y *= _ProjectionParams.x;
                float sceneZ = max(0,LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)))) - _ProjectionParams.g);
                float partZ = max(0,i.projPos.z - _ProjectionParams.g);
                float node_3506_ang = 1.5708;
                float node_3506_spd = 1.0;
                float node_3506_cos = cos(node_3506_spd*node_3506_ang);
                float node_3506_sin = sin(node_3506_spd*node_3506_ang);
                float2 node_3506_piv = float2(0.5,0.5);
                float2 node_3506 = (mul(i.uv0-node_3506_piv,float2x2( node_3506_cos, -node_3506_sin, node_3506_sin, node_3506_cos))+node_3506_piv);
                float2 node_3558 = ((objScale.rb*_NormalTiling)/1000.0);
                float4 node_6304 = _Time + _TimeEditor;
                float3 node_1163 = (float3((_WaveSpeed/node_3558),0.0)*(node_6304.r/100.0));
                float2 node_8041 = ((node_3506+node_1163)*node_3558);
                float4 node_470 = tex2D(_NormalTexture,TRANSFORM_TEX(node_8041, _NormalTexture));
                float2 node_8973 = ((i.uv0+node_1163)*node_3558);
                float4 node_9575 = tex2D(_NormalTexture,TRANSFORM_TEX(node_8973, _NormalTexture));
                float3 node_6972 = (node_470.rgb-node_9575.rgb);
                float node_1886 = (pow(saturate((sceneZ-partZ)/_DepthTransparency),_ShoreFade)*saturate((sceneZ-partZ)/_ShoreTransparency));
                float2 sceneUVs = float2(1,grabSign)*i.screenPos.xy*0.5+0.5 + ((node_6972.rg*(_Refraction*0.2))*node_1886);
                float4 sceneColor = tex2D(Refraction, sceneUVs);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalLocal = lerp(float3(0,0,1),node_6972,_Refraction);
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _Gloss;
                float specPow = exp2( gloss * 10.0+1.0);
////// Specular:
                float NdotL = max(0, dot( normalDirection, lightDirection ));
                float3 specularColor = (_Specular*_SpecularColor.rgb);
                float3 directSpecular = (floor(attenuation) * _LightColor0.xyz) * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = dot( normalDirection, lightDirection );
                float3 w = float3(_LightWrapping,_LightWrapping,_LightWrapping)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = forwardLight * attenColor;
                float3 indirectDiffuse = float3(0,0,0);
                indirectDiffuse += UNITY_LIGHTMODEL_AMBIENT.rgb; // Ambient Light
                float3 node_6686 = float3(0,0,0);
                float node_3685 = 1.0;
                float3 node_2072 = saturate((_DeepWaterColor.rgb+(sceneColor.rgb*pow(saturate((node_3685 + ( ((sceneZ-partZ) - node_6686) * (0.5 - node_3685) ) / (((_MainColor.rgb*(10.0/_Density))+0.0) - node_6686))),_Fade))));
                float2 node_2569 = node_6972.rg;
                float2 node_5462 = ((i.screenPos.rg+(float2(node_2569.r,node_2569.g)*_Distortion))*0.5+0.5);
                float4 _ReflectionTex_var = tex2D(_ReflectionTex,TRANSFORM_TEX(node_5462, _ReflectionTex));
                float node_6960_ang = 1.5708;
                float node_6960_spd = 1.0;
                float node_6960_cos = cos(node_6960_spd*node_6960_ang);
                float node_6960_sin = sin(node_6960_spd*node_6960_ang);
                float2 node_6960_piv = float2(0.5,0.5);
                float2 node_6960 = (mul(i.uv0-node_6960_piv,float2x2( node_6960_cos, -node_6960_sin, node_6960_sin, node_6960_cos))+node_6960_piv);
                float2 node_9910 = ((objScale.rb*_FoamTiling)/1000.0);
                float4 node_6613 = _Time + _TimeEditor;
                float3 node_4993 = (float3((_FoamSpeed/node_9910),0.0)*(node_6613.r/100.0));
                float2 node_4290 = ((node_6960+node_4993)*node_9910);
                float4 node_8133 = tex2D(_FoamTexture,TRANSFORM_TEX(node_4290, _FoamTexture));
                float2 node_8960 = ((i.uv0+node_4993)*node_9910);
                float4 node_3613 = tex2D(_FoamTexture,TRANSFORM_TEX(node_8960, _FoamTexture));
                float node_9363 = 0.0;
                float3 node_5136 = ((saturate((sceneZ-partZ)/_FoamBlend)*-1.0+1.0)*(((node_9363 + ( (dot((node_8133.rgb-node_3613.rgb),float3(0.3,0.59,0.11)) - _FoamContrast) * (1.0 - node_9363) ) / ((1.0 - _FoamContrast) - _FoamContrast))*_FoamColor.rgb)*(_FoamIntensity*(-1.0))));
                float3 diffuseColor = lerp(lerp( node_2072, lerp(_ReflectionTex_var.rgb,node_2072,(1.0 - _ReflectionIntensity)), _EnableReflections ),(node_5136*node_5136),_FoamVisibility);
                float3 diffuse = (directDiffuse + indirectDiffuse) * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse + specular;
                fixed4 finalRGBA = fixed4(lerp(sceneColor.rgb, finalColor,node_1886),1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
