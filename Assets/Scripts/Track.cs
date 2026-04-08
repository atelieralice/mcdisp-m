using Godot;
using MCDISP;
using System;
using System.Collections.Generic;

namespace MCDISP {
	public partial class Track : Control {
		[Export] public Palette Palette;
		[Export] public Texture2D PianoKeys;
		[Export] public PitchIndicator PitchIndicator;
		[Export] public ProgressBar VelocityBar;
		[Export] public Indicator VelocityIndicator;
		[Export] public ProgressBar ExpressBar;
		[Export] public Indicator ExpressIndicator;
		[Export] public ProgressBar VolumeBar;
		[Export] public Indicator VolumeIndicator;
		[Export] public PanpotIndicator PanpotIndicator;
		[Export] public ProgressBar ReverbBar;
		[Export] public Indicator ReverbIndicator;
		[Export] public ProgressBar ChorusBar;
		[Export] public Indicator ChorusIndicator;

		public HashSet<int> _pressedKeys = [];
		private enum KeyType { Left, Middle, Right, Black }
		private const int KEY_WIDTH_W = 3;
		private const int KEY_WIDTH_B = 2;
		private Color _keyColor;

		private Tween _tween;

		private static KeyType GetKeyType ( int note ) {
			note = note % 12;
			return note switch {
				0 or 5 => KeyType.Left,
				2 or 7 or 9 => KeyType.Middle,
				4 or 11 => KeyType.Right,
				1 or 3 or 6 or 8 or 10 => KeyType.Black,
				_ => KeyType.Middle
			};
		}

		private static int GetWhiteKeyIndex ( int note ) {
			int octave = note / 12;
			int posInOctave = note % 12;
			int baseIndex = octave * 7;

			int offset = posInOctave switch {
				0 or 1 => 0,  // C, C#
				2 or 3 => 1,  // D, D#
				4 => 2,       // E
				5 or 6 => 3,  // F, F#
				7 or 8 => 4,  // G, G#
				9 or 10 => 5, // A, A#
				11 => 6,      // B
				_ => 0
			};
			return baseIndex + offset;
		}

		public void SetNoteState ( int note, bool pressed ) {
			if ( pressed ) {
				_pressedKeys.Add ( note );
			} else {
				_pressedKeys.Remove ( note );
			}
			QueueRedraw ( );
		}
		public void SetPitchBend ( int pitchBendValue ) => PitchIndicator.UpdatePosition ( pitchBendValue );
		public void SetVelocity ( int velocityValue ) {
			VelocityIndicator.Visible = velocityValue != 0;
			VelocityIndicator.UpdatePosition ( velocityValue );
			VelocityBar.Value = velocityValue;
			_tween?.Kill ( );
			_tween = CreateTween ( );
			_tween.TweenProperty ( VelocityBar, "value", 0, 5f )
				.SetTrans ( Tween.TransitionType.Expo )
				.SetEase ( Tween.EaseType.Out );
		}
		public void SetExpression ( int expressionValue ) => ExpressBar.Value = expressionValue;
		public void SetVolume ( int volumeValue ) => VolumeBar.Value = volumeValue;
		public void SetPanpot ( int panpotValue ) => PanpotIndicator.UpdatePosition ( panpotValue );
		public void SetReverb ( int reverbValue ) => ReverbBar.Value = reverbValue;
		public void SetChorus ( int chorusValue ) => ChorusBar.Value = chorusValue;

		public override void _Draw ( ) {
			// Blit key textures from atlas to screen
			foreach ( int note in _pressedKeys ) {
				KeyType keyType = GetKeyType ( note );
				if ( keyType == KeyType.Black ) continue;
				int whiteKeyIndex = GetWhiteKeyIndex ( note );

				// Get atlas position
				int srcX = (int)keyType * KEY_WIDTH_W;
				Rect2 srcRect = new Rect2 ( srcX, 0, KEY_WIDTH_W, PianoKeys.GetHeight ( ) );

				// Get screen position
				int dstX = whiteKeyIndex * ( KEY_WIDTH_W + 1 ); // 1px border between keys
				Rect2 dstRect = new Rect2 ( dstX, 1, KEY_WIDTH_W, PianoKeys.GetHeight ( ) );

				DrawTextureRectRegion ( PianoKeys, dstRect, srcRect, _keyColor );
			}
			foreach ( int note in _pressedKeys ) {
				KeyType keyType = GetKeyType ( note );
				if ( keyType != KeyType.Black ) continue;
				int whiteKeyIndex = GetWhiteKeyIndex ( note );

				// Get atlas position
				int srcX = (int)keyType * KEY_WIDTH_W;
				Rect2 srcRect = new Rect2 ( srcX, 0, KEY_WIDTH_B, PianoKeys.GetHeight ( ) );

				// Get screen position
				int dstX = whiteKeyIndex * ( KEY_WIDTH_W + 1 ); // 1px border between keys
				Rect2 dstRect = new Rect2 ( dstX + 2, 1, KEY_WIDTH_B, PianoKeys.GetHeight ( ) );

				DrawTextureRectRegion ( PianoKeys, dstRect, srcRect, _keyColor );
			}
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready ( ) {
			_keyColor = Palette.GetColorByIndex ( 7 );
			PitchIndicator.SelfModulate = Palette.GetColorByIndex ( 6 );
			VelocityBar.SelfModulate = Palette.GetColorByIndex ( 6 );
			VelocityIndicator.SelfModulate = Palette.GetColorByIndex ( 6 );
			ExpressBar.SelfModulate = Palette.GetColorByIndex ( 6 );
			ExpressIndicator.SelfModulate = Palette.GetColorByIndex ( 6 );
			VolumeBar.SelfModulate = Palette.GetColorByIndex ( 6 );
			VolumeIndicator.SelfModulate = Palette.GetColorByIndex ( 6 );
			PanpotIndicator.SelfModulate = Palette.GetColorByIndex ( 6 );
			ReverbBar.SelfModulate = Palette.GetColorByIndex ( 5 );
			ReverbIndicator.SelfModulate = Palette.GetColorByIndex ( 5 );
			ChorusBar.SelfModulate = Palette.GetColorByIndex ( 5 );
			ChorusIndicator.SelfModulate = Palette.GetColorByIndex ( 5 );
		}
	}
}
