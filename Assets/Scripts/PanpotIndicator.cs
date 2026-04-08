using Godot;
using System;

namespace MCDISP {
	public partial class PanpotIndicator : ColorRect {
		private float _startX;

		public void UpdatePosition ( float midiValue ) {
			midiValue = Mathf.Clamp ( midiValue, 0, 127f );

			float offset = midiValue < 64
				? ( midiValue - 64f ) / 64f * 16f
				: ( midiValue - 64f ) / 63f * 15f;

			Position = new Vector2 ( _startX + offset, Position.Y );
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready ( ) {
			_startX = Position.X;
		}
	}
}
