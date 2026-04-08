using Godot;
using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using System.Threading.Tasks;

namespace MCDISP {
	[GlobalClass]
	public partial class MidiTest : Node {
		[Export] public Track TargetTrack;
		[Export] public float NoteDuration = 0.5f;

		// private async Task Test ( ) {
		// 	for ( int i = 0; i <= 127; i++ ) {
		// 		TargetTrack.SetNoteState ( i, true );
		// 		await ToSignal ( GetTree ( ).CreateTimer ( NoteDuration ), SceneTreeTimer.SignalName.Timeout );
		// 		TargetTrack.SetNoteState ( i, false );
		// 	}
		// }

		private async Task Test ( ) {
			float cycleTime = 2.0f;

			while ( true ) {
				float elapsed = 0f;

				while ( elapsed < cycleTime ) {
					float t = elapsed / cycleTime;
					float sineWave = Mathf.Sin ( t * Mathf.Tau );
					int panValue = (int)( ( sineWave + 1f ) * 63.5f );

					TargetTrack.SetPanpot ( panValue );

					await ToSignal ( GetTree ( ), SceneTree.SignalName.ProcessFrame );
					elapsed += (float)GetProcessDeltaTime ( );
				}
			}
		}

		// public override void _Ready ( ) {
		// 	if ( TargetTrack == null ) {
		// 		TargetTrack = GetNode<Track> ( "../Track1" );
		// 	}
		// 	_ = Test ( );
		// 	TargetTrack.SetNoteState ( 60, true );
		// }
	}
}