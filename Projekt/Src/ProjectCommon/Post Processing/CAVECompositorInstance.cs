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
	[CompositorName( "CAVE" )]
	public class CAVECompositorParameters : CompositorParameters
	{
        
	}

	/// <summary>
	/// Represents work with the CAVE post effect.
	/// </summary>
    [CompositorName("CAVE")]
	public class CAVECompositorInstance : CompositorInstance
	{
        Vec4 _tctl;
        Vec4 _tctr;
        Vec4 _tcbl;
        Vec4 _tcbr;

        public Vec2 Tctl
        {
            get { return new Vec2(_tctl.X, _tctl.Y); }
            set { _tctl = new Vec4(value.X, value.Y, 0.0f, 1.0f); }
        }
       
        public Vec2 Tctr
        {
            get { return new Vec2(_tctr.X, _tctr.Y); }
            set { _tctr = new Vec4(value.X, value.Y, 0.0f, 1.0f); }
        }
        
        public Vec2 Tcbl
        {
            get { return new Vec2(_tcbl.X, _tcbl.Y); }
            set { _tcbl = new Vec4(value.X, value.Y, 0.0f, 1.0f); }
        }
        
        public Vec2 Tcbr
        {
            get { return new Vec2(_tcbr.X, _tcbr.Y); }
            set { _tcbr = new Vec4(value.X, value.Y, 0.0f, 1.0f); }
        }

        CAVECompositorInstance()
        {
            _tctl = new Vec4(0.0f, 0.0f, 0.0f, 1.0f);
            _tctr = new Vec4(1.0f, 0.0f, 0.0f, 1.0f);
            _tcbl = new Vec4(0.0f, 1.0f, 0.0f, 1.0f);
            _tcbr = new Vec4(1.0f, 1.0f, 0.0f, 1.0f);
        }
        
		protected override void OnMaterialRender( uint passId, Material material, ref bool skipPass )
		{
			base.OnMaterialRender( passId, material, ref skipPass );

			if( passId == 557 )
			{
				GpuProgramParameters parameters = material.Techniques[ 0 ].Passes[ 0 ].FragmentProgramParameters;

                parameters.SetNamedConstant("tctl", _tctl);
                parameters.SetNamedConstant("tctr", _tctr);
                parameters.SetNamedConstant("tcbl", _tcbl);
                parameters.SetNamedConstant("tcbr", _tcbr);
                
			}
		}

		protected override void OnUpdateParameters( CompositorParameters parameters )
		{
			base.OnUpdateParameters( parameters );
		}

	}
}
