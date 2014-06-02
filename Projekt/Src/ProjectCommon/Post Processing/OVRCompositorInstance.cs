using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing.Design;
using System.ComponentModel;
using Engine;
using Engine.Renderer;
using Engine.MathEx;

namespace ProjectCommon
{
	[CompositorName( "OVR" )]
	public class OVRCompositorParameters : CompositorParameters
	{
		//float intensity = 1;

       


        //[DefaultValue( 1.0f )]
        //[Editor( typeof( SingleValueEditor ), typeof( UITypeEditor ) )]
        //[EditorLimitsRange( 0, 1 )]
        //public float Intensity
        //{
        //    get { return intensity; }
        //    set
        //    {
        //        if( value < 0 )
        //            value = 0;
        //        if( value > 1 )
        //            value = 1;
        //        intensity = value;
        //    }
        //}
	}

	/// <summary>
	/// Represents work with the OVR post effect.
	/// </summary>
    [CompositorName("OVR")]
	public class OVRCompositorInstance : CompositorInstance
	{
		//float intensity = 1;

        public Vec4 lensCenter;
        public Vec4 screenScenter;
        public Vec4 scale;
        public Vec4 scaleIn;
        public Vec4 hmdWarpParam;
        public float projectionCorrection;

        OVRCompositorInstance()
        {
            lensCenter = new Vec4(0.5f, 0.5f, 0.0f, 0.0f);
            screenScenter = new Vec4(0.0f, 0.0f, 0.0f, 0.0f);
            scale = new Vec4(0.3f, 0.35f, 0.0f, 0.0f);
            scaleIn = new Vec4(1.0f, 1.0f, 0.0f, 0.0f);
            hmdWarpParam = new Vec4(1.0f, 0.22f, 0.24f, 0.0f);
        }

		//

        //[EditorLimitsRange( 0, 1 )]
        //public float Intensity
        //{
        //    get { return intensity; }
        //    set
        //    {
        //        if( value < 0 )
        //            value = 0;
        //        if( value > 1 )
        //            value = 1;
        //        intensity = value;
        //    }
        //}

		protected override void OnMaterialRender( uint passId, Material material, ref bool skipPass )
		{
			base.OnMaterialRender( passId, material, ref skipPass );

			if( passId == 556 )
			{
				GpuProgramParameters parameters = material.Techniques[ 0 ].Passes[ 0 ].FragmentProgramParameters;
				//parameters.SetNamedConstant( "intensity", intensity );

                parameters.SetNamedConstant("LensCenter", lensCenter);
                parameters.SetNamedConstant("ScreenCenter", screenScenter);
                parameters.SetNamedConstant("Scale", scale);
                parameters.SetNamedConstant("ScaleIn", scaleIn);
                parameters.SetNamedConstant("HmdWarpParam", hmdWarpParam);
                parameters.SetNamedConstant("ProjectionCorrection", projectionCorrection);
                
			}
		}

		protected override void OnUpdateParameters( CompositorParameters parameters )
		{
			base.OnUpdateParameters( parameters );

			//OVRCompositorParameters p = (OVRCompositorParameters)parameters;
			//Intensity = p.Intensity;
		}

	}
}
