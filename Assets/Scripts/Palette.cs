using Godot;
using System;
using System.Runtime.CompilerServices;

namespace MCDISP {
    [GlobalClass]
    public partial class Palette : Resource {
        [Export]
        public Color[] Colors = new Color[16];

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public Color GetColorByIndex ( int index ) {
            return Colors[index];
        }
    }
}