using Godot;
using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System.Linq;

namespace MCDISP {
	public partial class Main : Node {
		[Export] public Palette Palette;
		[Export] public ColorRect BG;

		private Label _titleLabel;
		private Window _window;
		private bool _isEnlarged = false;

		private string _defaultPortName = "SC-VA";
		private OutputDevice _midiOut;
		private Playback _playback;
		private List<Track> _tracks = [];
		private bool _ignoreEvents = false; // For making piano visualisations stay after pausing
		private bool _isFinished = false;
		private uint _sessionId = 0;

		private void PlayMidi ( string filePath ) {
			_sessionId++;
			_ignoreEvents = false;
			_isFinished = false;
			if ( _playback != null ) {
				_playback.EventPlayed -= OnEventPlayed;
				_playback.Finished -= OnPlaybackFinished;
				_playback.Dispose ( );
				_playback = null;
			}

			ResetAudioState ( );
			ResetVisuals ( );

			GC.Collect ( );
			GC.WaitForPendingFinalizers ( );
			GC.Collect ( );

			try {
				var file = MidiFile.Read ( filePath, new ReadingSettings {
					NotEnoughBytesPolicy = NotEnoughBytesPolicy.Ignore,
					InvalidChunkSizePolicy = InvalidChunkSizePolicy.Ignore,
					NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore,
					InvalidMetaEventParameterValuePolicy = InvalidMetaEventParameterValuePolicy.SnapToLimits
				} );
				var title = GetMidiTitle ( file, filePath );
				_titleLabel.Text = title;
				_window.Title = $"MCDISP  -  Now playing: {title}";
				_playback = file.GetPlayback ( _midiOut );
				_playback.EventPlayed += OnEventPlayed;
				_playback.Finished += OnPlaybackFinished;
				_playback.Start ( );
			} catch ( Exception ex ) {
				_titleLabel.Text = "Loading error!";
				_window.Title = $"MCDISP  -  Loading error!";
				GD.PrintErr ( ex );
			}
		}

		private static string GetMidiTitle ( MidiFile midiFile, string filePath ) {
			foreach ( var trackChunk in midiFile.GetTrackChunks ( ) ) {
				foreach ( var midiEvent in trackChunk.Events ) {
					if ( midiEvent is SequenceTrackNameEvent trackName && !string.IsNullOrWhiteSpace ( trackName.Text ) )
						return trackName.Text;
				}
			}
			return System.IO.Path.GetFileName ( filePath );
		}

		private void OnFilesDropped ( string[] files ) {
			PlayMidi ( files[0] );
		}

		private void OnEventPlayed ( object sender, MidiEventPlayedEventArgs e ) {
			if ( _ignoreEvents ) return;
			uint sessionId = _sessionId;

			// KEY-ON/OFF
			if ( e.Event is NoteOnEvent noteOn ) {
				CallDeferred ( "UpdateTrackVisual", sessionId, (int)noteOn.Channel, (int)noteOn.NoteNumber, noteOn.Velocity > 0 );
				CallDeferred ( "UpdateTrackVelocity", sessionId, (int)noteOn.Channel, (int)noteOn.Velocity );
			} else if ( e.Event is NoteOffEvent noteOff ) {
				CallDeferred ( "UpdateTrackVisual", sessionId, (int)noteOff.Channel, (int)noteOff.NoteNumber, false );
			}

			// Pitch-bend
			if ( e.Event is PitchBendEvent pitchBend ) {
				CallDeferred ( "UpdateTrackPitch", sessionId, (int)pitchBend.Channel, (int)pitchBend.PitchValue );
			}

			// CC
			if ( e.Event is ControlChangeEvent cc ) {
				int channel = cc.Channel;
				int value = cc.ControlValue;

				switch ( cc.ControlNumber ) {
					case 11:
						CallDeferred ( "UpdateTrackExpression", sessionId, channel, value );
						break;
					case 7:
						CallDeferred ( "UpdateTrackVolume", sessionId, channel, value );
						break;
					case 10:
						CallDeferred ( "UpdateTrackPanpot", sessionId, channel, value );
						break;
					case 91:
						CallDeferred ( "UpdateTrackReverb", sessionId, channel, value );
						break;
					case 93:
						CallDeferred ( "UpdateTrackChorus", sessionId, channel, value );
						break;
				}
			}
		}

		private void OnPlaybackFinished ( object sender, EventArgs e ) {
			_isFinished = true;
			// CallDeferred ( "ResetVisuals" );
		}

		private void UpdateTrackVisual ( uint sessionId, int channel, int note, bool pressed ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetNoteState ( note, pressed );
			}
		}
		private void UpdateTrackPitch ( uint sessionId, int channel, int pitch ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetPitchBend ( pitch );
			}
		}
		private void UpdateTrackVelocity ( uint sessionId, int channel, int value ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetVelocity ( value );
			}
		}
		private void UpdateTrackExpression ( uint sessionId, int channel, int value ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetExpression ( value );
			}
		}
		private void UpdateTrackVolume ( uint sessionId, int channel, int value ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetVolume ( value );
			}
		}
		private void UpdateTrackPanpot ( uint sessionId, int channel, int value ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetPanpot ( value );
			}
		}
		private void UpdateTrackReverb ( uint sessionId, int channel, int value ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetReverb ( value );
			}
		}
		private void UpdateTrackChorus ( uint sessionId, int channel, int value ) {
			if ( sessionId != _sessionId ) return;
			if ( channel < _tracks.Count ) {
				_tracks[channel].SetChorus ( value );
			}
		}
		private void ResetVisuals ( ) {
			foreach ( var track in _tracks ) {
				track._pressedKeys.Clear ( );
				track.QueueRedraw ( );

				track.SetPitchBend ( 8192 );
				track.SetVelocity ( 0 );
				track.SetExpression ( 127 );
				track.SetVolume ( 100 );
				track.SetPanpot ( 64 );
				track.SetReverb ( 0 );
				track.SetChorus ( 0 );
			}
		}
		private void ResetAudioState ( ) {
			if ( _midiOut == null ) return;
			_midiOut.TurnAllNotesOff ( );

			for ( byte channel = 0; channel < 16; channel++ ) {
				var ch = (FourBitNumber)channel;
				// Pitch-bend
				_midiOut.SendEvent ( new PitchBendEvent ( (ushort)8192 ) { Channel = ch } );
				// Expression
				_midiOut.SendEvent ( new ControlChangeEvent ( (SevenBitNumber)11, (SevenBitNumber)127 ) { Channel = ch } );
				// Volume
				_midiOut.SendEvent ( new ControlChangeEvent ( (SevenBitNumber)7, (SevenBitNumber)100 ) { Channel = ch } );
				// Panpot
				_midiOut.SendEvent ( new ControlChangeEvent ( (SevenBitNumber)10, (SevenBitNumber)64 ) { Channel = ch } );
				// Reverb
				_midiOut.SendEvent ( new ControlChangeEvent ( (SevenBitNumber)91, (SevenBitNumber)0 ) { Channel = ch } );
				// Chorus
				_midiOut.SendEvent ( new ControlChangeEvent ( (SevenBitNumber)93, (SevenBitNumber)0 ) { Channel = ch } );
			}
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready ( ) {

			BG.SelfModulate = Palette.GetColorByIndex ( 0 );
			_titleLabel = GetNode<Label> ( "TitleLabel" );
			_window = GetWindow ( );

			var allDevices = OutputDevice.GetAll ( );
			_midiOut = allDevices.FirstOrDefault ( d => d.Name == _defaultPortName ) ?? allDevices.FirstOrDefault ( );
			if ( _midiOut == null ) {
				GD.PrintErr ( $"Port {_defaultPortName} couldn't be found. " );
			} else {
				GD.Print ( $"Using MIDI device: {_midiOut.Name}" );
				GetTree ( ).Root.FilesDropped += OnFilesDropped;
			}

			for ( int i = 1; i <= 16; i++ ) {
				var track = GetNode<Track> ( $"Track{i}" ); // TODO: Modify soon-to-be none label to MIDI
				_tracks.Add ( track );
			}
		}

		public override void _ExitTree ( ) {
			if ( _playback != null ) {
				_playback.Dispose ( );
				_playback = null;
			}

			if ( _midiOut != null ) {
				_midiOut.TurnAllNotesOff ( );
				_midiOut.Dispose ( );
				_midiOut = null;
			}
		}

		public override void _Input ( InputEvent @event ) {
			if ( @event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo ) {
				// Pause toggle
				if ( eventKey.Keycode == Key.Space ) {
					if ( _playback?.IsRunning == true ) {
						_ignoreEvents = true;
						_playback?.Stop ( );
					} else {
						_ignoreEvents = false;
						if ( _isFinished ) {
							_sessionId++;
							ResetAudioState ( );
							ResetVisuals ( );
							_playback?.MoveToStart ( );
							_isFinished = false;
						}
						_playback?.Start ( );
					}
				}
				// Stop
				if ( eventKey.Keycode == Key.Escape ) {
					_ignoreEvents = true;
					_sessionId++;
					_playback?.Stop ( );
					_playback?.MoveToStart ( );
					ResetAudioState ( );
					ResetVisuals ( );
				}
				// Restart
				if ( eventKey.Keycode == Key.F1 ) {
					_ignoreEvents = true;
					_sessionId++;
					ResetAudioState ( );
					ResetVisuals ( );
					_playback?.MoveToStart ( );
					_ignoreEvents = false;
					_playback?.Start ( );
				}

				if ( eventKey.Keycode == Key.F9 ) {
					DisplayServer.WindowSetFlag (
						DisplayServer.WindowFlags.SharpCorners,
						!DisplayServer.WindowGetFlag ( DisplayServer.WindowFlags.SharpCorners )
					);
				}
				if ( eventKey.Keycode == Key.F10 ) {
					_window.Borderless = !_window.Borderless;
					var targetSize = new Vector2I ( 768, 512 );
					_window.Size = _isEnlarged ? targetSize * 2 : targetSize;
				}
				if ( eventKey.Keycode == Key.F11 ) {
					_window.Size = _isEnlarged ? _window.Size / 2 : _window.Size * 2;
					_isEnlarged = !_isEnlarged;
				}
				if ( eventKey.Keycode == Key.F12 ) {
					_window.AlwaysOnTop = !_window.AlwaysOnTop;
				}
			}
		}
	}
}