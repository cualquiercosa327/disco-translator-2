# disco-translator-2
Disco Translator 2 is a software suite for creating language packs for [Disco Elysium](https://zaumstudio.com/). It consists of a BepInEx plugin for interfacing between the game and the file system, as well as Transltool, a Python script for splitting the extracted strings into human-readable `.transl` files and resource management in general.

Disco Translator 2 is currently being used to translate the game into Polish: [disco-elysium-polish](https://github.com/Lachcim/disco-elysium-polish). Come help the effort!

## Installing and using Disco Translator 2

### I want to use a translation

1. Download and install [BepInEx](https://github.com/BepInEx/BepInEx/releases), a plugin framework for Unity games
2. Run the game once so that BepInEx can set up its file structure
3. Download and install [Disco Translator 2](https://github.com/Lachcim/disco-translator-2/releases) by placing it in the `plugins` directory
4. Run the game once again
5. Place your `.transl` file in `Disco Elysium\BepInEx\plugins\DiscoTranslator2`


### I want to be a translator

1. Install Python 3 if you haven't already
2. Install Disco Translator 2 as described above
3. Open `Disco Elysium\BepInEx\config\pl.mssnt.DiscoTranslator2.cfg` in your text editor of choice
4. Set the translation path to your workplace, presumably your git repo
5. Set the database extraction path to somewhere you can easily access
6. Run the game once to extract the database
7. Use Transltool to dump the database:
```
python transltool.py --dump database.json mydumpdir
```
8. Move `.transl` files from the dump output directory to your workplace and commit them to your repo once translated.
9. The plugin will detect file changes and update the strings as you save them. For troubleshooting, you can configure BepInEx to show you the debug console. Errors and missing translations will be displayed there.
