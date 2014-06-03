using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProjectEntities
{

    public class ProgressRepairableType : RepairableType
    {
        [FieldSerialize]
        private int progressPerPress = 1;

        

        [FieldSerialize]
        private int progressRequired = 10;


        [Description("Progress that gets added each time when used.")]
        public int ProgressPerPress
        {
            get { return progressPerPress; }
            set { progressPerPress = value; }
        }

        [Description("Progress at which object is repaired.")]
        public int ProgressRequired
        {
            get { return progressRequired; }
            set { progressRequired = value; }
        }


    }

    public class ProgressRepairable : Repairable
    {
        ProgressRepairableType _type = null; public new ProgressRepairableType Type { get { return _type; } }
        private int progress = 0;

        public override void Press()
        {
            SoundPlay3D(Type.SoundUsing, .5f, false);

            progress += Type.ProgressPerPress;

            if (progress >= Type.ProgressRequired)
                Repaired = true;
        }
        

    }
}
