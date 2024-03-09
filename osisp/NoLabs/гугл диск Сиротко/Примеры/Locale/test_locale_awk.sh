export LC_TIME="ru_RU.UTF-8"
echo "Use \"Rusian Rusian\" datetime locale settings: $LC_TIME"
awk 'BEGIN { print strftime() }'
export LC_TIME="en_UK"
echo "Use \"United Kingdom English\" datetime locale settings: $LC_TIME"
awk 'BEGIN { print strftime() }'
export LC_TIME="en_GB"
echo "Use \"Great Britain English\" datetime locale settings: $LC_TIME"
awk 'BEGIN { print strftime() }'
export LC_TIME="en_US"
echo "Use \"United States English\" datetime locale settings: $LC_TIME"
awk 'BEGIN { print strftime() }'
export LC_TIME="fr_FR"
echo "Use \"France France\" datetime locale settings: $LC_TIME"
awk 'BEGIN { print strftime() }'