using Godot;
using System;

namespace MCDISP {
	public partial class PitchIndicator : ColorRect {
		private float _startX;

		public void UpdatePosition ( float midiValue ) {
			midiValue = Mathf.Clamp ( midiValue, 0, 16383f );

			float offset = ( midiValue - 8192f ) / 8192f * 144;

			Position = new Vector2 ( _startX + offset, Position.Y );
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready ( ) {
			_startX = Position.X;
		}
	}
}
