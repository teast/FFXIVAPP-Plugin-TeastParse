#!/bin/sh

BIN=../FFXIVAPP.Plugin.TeastParse/bin/Debug/netstandard2.1
DEST=../../ffxivapp/FFXIVAPP.Client/bin/Debug/netcoreapp3.0/Plugins/FFXIVAPP.Plugin.TeastParse


./install.sh $BIN/FFXIVAPP.Plugin.TeastParse.dll $DEST || exit 1
./install.sh $BIN/FFXIVAPP.Plugin.TeastParse.pdb $DEST || exit 2
./install.sh $BIN/PluginInfo.xml $DEST || exit 3
./install.sh ../FFXIVAPP.Plugin.TeastParse/Logo.png $DEST || exit 4
