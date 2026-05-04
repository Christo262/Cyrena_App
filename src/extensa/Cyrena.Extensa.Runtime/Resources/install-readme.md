# Installations

Place zipped extensions in this folder for Cyréna to install on next startup.

## ZIP Structure
my.extension.zip
├── [application files]
└── extension.json

`extension.json` is the extension manifest and must point to the entry assembly.

## Requirements

- The ZIP filename must exactly match the `id` field in `extension.json`.
- On startup, Cyréna will unzip the archive and move its contents to the extensions directory under a folder named after the extension id.