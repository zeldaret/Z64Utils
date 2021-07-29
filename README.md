# Z64Utils Configuration Files

This repository contains the configuration files to be used with [Z64Utils](https://github.com/Random06457/Z64Utils) since version [v2.1.0](https://github.com/Random06457/Z64Utils/releases/tag/v2.1.0).

# Version Format

`versions/*.json`

The version files are stored in JSON format and contain information to identify ROM versions and provide information that cannot be extracted directly from ROMs.


### ROM Identification / Information Fields

`version_name`: The version name. e.g. `"Majora's Mask Japan 1.0"`

`version_game`: The game version. This can be either `oot` (Ocarina of Time) or `mm` (Majora's Mask).

`compression_method`: The compression method. This can be either `yaz0` or `zlib`.

`identifier.build_team`: The build team. e.g. `zelda@srd44`.

`identifier.build_date`: The build date. e.g. `00-03-31 02:22:11`.

`memory.code`: Code VRAM address.

`memory.actor_table`: Actor overlay table VRAM address.

`memory.gamestate_table`: Gamestate overlay table VRAM address.

`memory.effect_table`: Effect SS2 overlay table VRAM address.

`memory.kaleido_mgr_table`: Kaleido Manager (pause menu) overlay table VRAM address.

`memory.fbdemo_table`: FB Demo (transition effect) overlay table VRAM address. This field is specific to Majora's Mask.

`memory.map_mark_data_table`: Map Mark Data overlay table VRAM address. This field is specific to Ocarina of Time.

### File Fields

A version file should at least contain a `code` file entry in order to be considered valid.

`files[].vrom`: File VROM address.

`files[].name`: File name.

`files[].type`: File type. (Current possible values: `Unknow`, `Code`, `Object`, `Room`, `Scene`)
