rm ./.git/refs/tags/Build_*
rm ./.git/refs/tags/Dev-*
rm ./.git/refs/tags/Test-*
rm ./.git/refs/tags/PreProd-*
rm ./.git/refs/tags/Prod-Release-*

echo "Now do a 'git fetch --tags' to get your tags back"
echo ""

read  -n 1 -p "Press Enter to finish:" unused
