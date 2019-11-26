#!/bin/bash
[ ! -f airports.dat ] && wget https://raw.githubusercontent.com/jpatokal/openflights/master/data/airports.dat
echo "namespace Skyscanner.Model"
echo "{"
echo "#pragma warning disable CA1720"
echo "    public enum Airport"
echo "    {"
for airport in $(cat airports.dat | awk -F ',' '{print$5}' | grep -o '"[A-Z][A-Z][A-Z]"' | grep -o '[A-Z][A-Z][A-Z]' | sort); do
echo "        $airport,"
done
echo "    }"
echo "#pragma warning restore CA1720"
echo "}"

