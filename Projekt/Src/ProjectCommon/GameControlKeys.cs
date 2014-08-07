// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.EntitySystem;

namespace ProjectCommon
{
    public enum  GameControlKeys
    {
        ///////////////////////////////////////////
        //Moving

        [DefaultKeyboardMouseValue(EKeys.W)]
        [DefaultKeyboardMouseValue(EKeys.Up)]
        //[DefaultJoystickValue( JoystickAxes.Y, JoystickAxisFilters.GreaterZero )]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickY, JoystickAxisFilters.GreaterZero)]
        [DefaultJoystickValue(JoystickButtons.Button1)]
        Forward,

        [DefaultKeyboardMouseValue(EKeys.S)]
        [DefaultKeyboardMouseValue(EKeys.Down)]
        //[DefaultJoystickValue( JoystickAxes.Y, JoystickAxisFilters.LessZero )]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickY, JoystickAxisFilters.LessZero)]
        [DefaultJoystickValue(JoystickButtons.Button2)]
        Backward,

        [DefaultKeyboardMouseValue(EKeys.A)]
        [DefaultKeyboardMouseValue(EKeys.Left)]
        //[DefaultJoystickValue( JoystickAxes.X, JoystickAxisFilters.LessZero )]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickX, JoystickAxisFilters.LessZero)]
        [DefaultJoystickValue(JoystickButtons.Button4)]
        Left,

        [DefaultKeyboardMouseValue(EKeys.D)]
        [DefaultKeyboardMouseValue(EKeys.Right)]
        //[DefaultJoystickValue( JoystickAxes.X, JoystickAxisFilters.GreaterZero )]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftThumbstickX, JoystickAxisFilters.GreaterZero)]
        [DefaultJoystickValue(JoystickButtons.Button3)]
        Right,

        ///////////////////////////////////////////
        //Looking

        [DefaultJoystickValue(JoystickAxes.Rz, JoystickAxisFilters.GreaterZero)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickY, JoystickAxisFilters.GreaterZero)]
        //MouseMove (in the PlayerIntellect)
        [DefaultJoystickValue(JoystickAxes.Y, JoystickAxisFilters.GreaterZero)]
        LookUp,

        [DefaultJoystickValue(JoystickAxes.Rz, JoystickAxisFilters.LessZero)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickY, JoystickAxisFilters.LessZero)]
        //MouseMove (in the PlayerIntellect)
        [DefaultJoystickValue(JoystickAxes.Y, JoystickAxisFilters.LessZero)]
        LookDown,

        [DefaultJoystickValue(JoystickAxes.Z, JoystickAxisFilters.LessZero)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickX, JoystickAxisFilters.LessZero)]
        //MouseMove (in the PlayerIntellect)
        [DefaultJoystickValue(JoystickAxes.X, JoystickAxisFilters.LessZero)]
        LookLeft,

        [DefaultJoystickValue(JoystickAxes.Z, JoystickAxisFilters.GreaterZero)]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightThumbstickX, JoystickAxisFilters.GreaterZero)]
        //MouseMove (in the PlayerIntellect)
        [DefaultJoystickValue(JoystickAxes.X, JoystickAxisFilters.GreaterZero)]
        LookRight,

        ///////////////////////////////////////////
        //Actions

        [DefaultKeyboardMouseValue(EMouseButtons.Left)]
        //[DefaultJoystickValue( JoystickButtons.Button1 )]
        [DefaultJoystickValue(JoystickAxes.XBox360_RightTrigger, JoystickAxisFilters.GreaterZero)]
        [DefaultJoystickValue(JoystickButtons.Button13)]
        Fire1,

        [DefaultKeyboardMouseValue(EMouseButtons.Right)]
        //[DefaultJoystickValue( JoystickButtons.Button2 )]
        [DefaultJoystickValue(JoystickAxes.XBox360_LeftTrigger, JoystickAxisFilters.GreaterZero)]
        [DefaultJoystickValue(JoystickButtons.Button12)]
        Fire2,

        [DefaultKeyboardMouseValue(EKeys.Space)]
        //[DefaultJoystickValue( JoystickButtons.Button3 )]
        [DefaultJoystickValue(JoystickButtons.XBox360_A)]
        [DefaultJoystickValue(JoystickButtons.Button6)]
        Jump,

        [DefaultKeyboardMouseValue(EKeys.C)]
        //[DefaultJoystickValue( JoystickButtons.Button6 )]
        [DefaultJoystickValue(JoystickButtons.XBox360_B)]
        Crouching,

        [DefaultKeyboardMouseValue(EKeys.R)]
        //[DefaultJoystickValue( JoystickButtons.Button4 )]
        [DefaultJoystickValue(JoystickButtons.XBox360_LeftShoulder)]
        Reload,

        [DefaultKeyboardMouseValue(EKeys.E)]
        //[DefaultJoystickValue( JoystickButtons.Button5 )]
        [DefaultJoystickValue(JoystickButtons.XBox360_RightShoulder)]
        [DefaultJoystickValue(JoystickButtons.Button5)]
        Use,

        //[DefaultJoystickValue( JoystickPOVs.POV1, JoystickPOVDirections.West )]
        [DefaultJoystickValue(JoystickButtons.Button7)]
        PreviousWeapon,

        //[DefaultJoystickValue( JoystickPOVs.POV1, JoystickPOVDirections.East )]
        [DefaultJoystickValue(JoystickButtons.Button8)]
        NextWeapon,

        [DefaultJoystickValue(JoystickButtons.Button11)]
        Inventory,

        [DefaultJoystickValue(JoystickButtons.Button9)]
        Light,

        [DefaultJoystickValue(JoystickButtons.Button10)]
        WeaponStatus,

        [DefaultKeyboardMouseValue(EKeys.D1)]
        Weapon1,
        [DefaultKeyboardMouseValue(EKeys.D2)]
        Weapon2,
        [DefaultKeyboardMouseValue(EKeys.D3)]
        Weapon3,
        [DefaultKeyboardMouseValue(EKeys.D4)]
        Weapon4,
        [DefaultKeyboardMouseValue(EKeys.D5)]
        Weapon5,
        [DefaultKeyboardMouseValue(EKeys.D6)]
        Weapon6,
        [DefaultKeyboardMouseValue(EKeys.D7)]
        Weapon7,
        [DefaultKeyboardMouseValue(EKeys.D8)]
        Weapon8,
        [DefaultKeyboardMouseValue(EKeys.D9)]
        Weapon9,

        [DefaultKeyboardMouseValue(EKeys.Shift)]
        Run,

        //Vehicle
        [DefaultKeyboardMouseValue(EKeys.Z)]
        [DefaultJoystickValue(JoystickPOVs.POV1, JoystickPOVDirections.North)]
        VehicleGearUp,
        [DefaultKeyboardMouseValue(EKeys.X)]
        [DefaultJoystickValue(JoystickPOVs.POV1, JoystickPOVDirections.South)]
        VehicleGearDown,
        [DefaultKeyboardMouseValue(EKeys.Space)]
        VehicleHandbrake,
    }
}
