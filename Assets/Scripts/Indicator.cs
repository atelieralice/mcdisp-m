using Godot;
using System;

namespace MCDISP {
	[GlobalClass]
	public partial class Indicator : ColorRect {
		private float _startX;

		public void UpdatePosition ( float midiValue ) {
			midiValue = Mathf.Clamp ( midiValue, 0, 127f );

			float offset = midiValue / 127f * 31;

			Position = new Vector2 ( _startX + offset, Position.Y );
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready ( ) {
			_startX = Position.X;
		}
	}
}
