# Custom Radio Music Mod for Kebab Chefs

This is a mod for [Kebab Chefs](https://store.steampowered.com/app/1001270) that allows you to play custom music on the Radio.

![banner](https://github.com/Quriz/KC-CustomRadioMusic/assets/75581292/cbdfbace-dc11-41b9-ab3c-d59a6837a8cd)

## Features

- Play custom music from YouTube, SoundCloud and [many more](https://github.com/yt-dlp/yt-dlp/blob/master/supportedsites.md).
- Create a playlist
- Works in multiplayer

## Installation
1. Make sure you have MelonLoader installed for Kebab Chefs.
2. Download the [latest release](https://github.com/Quriz/KC-CustomRadioMusic/releases/latest) and put the DLLs inside the `Mods` folder.

## Usage

1. Open the radio.
2. Paste a video/music link, e.g. a YouTube or SoundCloud link.
3. Wait a bit until they are downloaded.
4. Enjoy the music!

## How to build

1. In `KC_CustomRadio/KC_CustomRadio.csproj` change `<GameDir>` to the path of your game.
2. Copy the DLL dependencies to the `Mods` folder. You can use the ones from the [releases](https://github.com/Quriz/KC-CustomRadioMusic/releases/latest).
3. Build the project! The mod's DLL will automatically be copied to the `Mods` folder.
