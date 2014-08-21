using Engine.EntitySystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class BrokenTerminalType : RepairableType { }

    public class BrokenTerminal : Repairable
    {
        BrokenTerminalType _type = null; public new BrokenTerminalType Type { get { return _type; } }

        [FieldSerialize]
        private Terminal terminal;


        //***************************
        //*******Getter-Setter*******
        //***************************
        public Terminal Terminal
        {
            set
            {
                terminal = value;
                if (terminal != null)
                    terminal.Visible = false;
            }

            get { return terminal; }
        }
        //*********************************


        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            if (terminal != null)
                terminal.Visible = false;
        }

        protected override void OnRepair()
        {
            base.OnRepair();


            terminal.Visible = true;

            terminal.Position = Position + new Engine.MathEx.Vec3(-1.007327f, 0.0f, -0.3f);
            terminal.Rotation = Rotation;

            if (!this.Died)
                this.Die();
        }


    }
}
