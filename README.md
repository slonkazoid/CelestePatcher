# CelestePatcher

Patches [Celeste](https://mattmakesgames.itch.io/celeste) to run without Steam.

While only tested on Arch Linux, it should work on other operating systems too. The instructions in this document are
mainly for Linux.

This project is intended for legitimate use, not for piracy.

# Prerequisites

- .NET SDK 7.0

```sh
# Arch Linux
sudo pacman -S --needed dotnet-sdk
```

# Building

```sh
git clone https://gitlab.com/slonkazoid/CelestePatcher
cd CelestePatcher
dotnet build -m -p:Configuration=Release # Build in Release mode, parallelized
```

### Rider:

1. Open the solution in Rider.
2. (Optional) Select 'Release' as the configuration.
3. Click 'Build Solution'.

# Usage

After building, run the `CelestePatcher/bin/Release/net7.0/CelestePatcher` file from inside the game directory.

```sh
cd ~/.local/share/Steam/steamapps/common/Celeste # Game installation directory
~/CelestePatcher/CelestePatcher/bin/Release/net7.0/CelestePatcher # Path to executable
mv Celeste.exe{,.bak} # Rename original Celeste.exe to Celeste.exe.bak
mv Celeste.exe{.patched,} # Rename the patched executable to Celeste.exe
rm lib{,64}/lib{Csteamworks,steam_api}.so Steamworks.NET.dll # Remove unnecessary files
```

# Contributing

Contributions are welcome.

You can use the [Issues](https://gitlab.com/slonkazoid/CelestePatcher/issues) tab to create a bug report.

# Disclaimer

While this project can be used to pirate Celeste, please do not use it to do so.  
Celeste's an indie game and it's worth the $20. It even goes on 75% sale on Steam.

I am not responsible for any misuse of this code.

# License

This project is licensed under the GNU General Public License v3.0. See the [LICENSE](LICENSE) file for more
information.