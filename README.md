# MCDISP-M

https://github.com/user-attachments/assets/0270b340-cdaa-450d-8262-8c66638cb83f

## Introduction

MCDISP-M is a modern port of **MCDISP.X**, a sequenced music player and display utility originally written for the Sharp X68000.
Built around a color palette system, it blits textures from an atlas to screen space to draw piano visuals without Node overhead.

Developed as a passion project when my internet connection was unusably unstable ❤️

**This is merely a showcase and not a public release.**

## Controls

Drag and drop a `.mid` file onto the window to start playback.

| Key | Action |
|-----|--------|
| `Space` | Pause / Resume |
| `Escape` | Stop |
| `F1` | Restart from beginning |
| `F9` | Toggle round/sharp corners (Windows 11 only) |
| `F10` | Toggle borderless mode |
| `F11` | Toggle 2× window scale |
| `F12` | Toggle always-on-top |

## Known Limitations

- **Some MIDI files do not play.** This is related to DryWetMIDI and I couldn't find a workaround.
- **Non-Latin text is not rendered correctly.** Track or song titles using non-Latin characters are displayed as quotation marks.
- **Port selection is hardcoded.** The app checks if a port named `SC-VA` exists and uses it. If not, it falls back to the first available MIDI output port.

## Running the Project

> **DryWetMIDI native libraries are required.** Place `Melanchall_DryWetMidi_Native32.dll`, `Melanchall_DryWetMidi_Native64.dll`, and `Melanchall_DryWetMidi_Native64.dylib` in the project root before building or running from the editor.

- Open the project in [Godot Editor (4.6.2)](https://godotengine.org/download/archive/4.6.2-stable/) with .NET support and press Play.
- Or grab the precompiled build from [Releases](https://github.com/atelieralice/mcdisp-m/releases).

Optional build check (requires .NET 10 SDK):

```sh
dotnet build
```

### Build Size

The Windows binary uses **Native AOT** and a custom export template I compiled from source, stripping out the 3D engine and non-OpenGL rendering backends to reduce the final build size:

| Component | Before | After |
|-----------|--------|-------|
| .NET libraries | 83 MB | 22 MB |
| Engine code | 102 MB | 32 MB |

## Credits / Third-Party

- **DryWetMIDI** by Melanchall – MIDI file parsing, playback, and device output ([NuGet](https://www.nuget.org/packages/Melanchall.DryWetMidi))
- **sharp-x68000.otf** – Font based on the Sharp X68000 character ROM, by [DamienG](https://fontstruct.com/fontstructions/show/2585848/sharp-x68000)
- **x12y16pxMaruMonica.ttf** – Pixel font by [hicc](https://hicchicc.github.io/00ff/)
- Bitmap graphics directly taken from the original MCDISP.X by (C) CUL.

## Disclaimer

MCDISP-M is an unofficial personal project inspired by MCDISP.X for Sharp X68000. It is not affiliated with or endorsed by Sharp Corporation, CUL., or any other third-party brands.
