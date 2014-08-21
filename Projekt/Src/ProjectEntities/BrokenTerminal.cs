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
            }

            get { return terminal; }
        }
        //*********************************


        protected override void OnRepair()
        {
            base.OnRepair();

            if (terminal == null)
                return;

            terminal.Position = Position + new Engine.MathEx.Vec3(-1.007327f, 0.0f, 0.3f);
            terminal.Rotation = Rotation;

            if (!this.Died)
                this.Die();
        }


    }
}
