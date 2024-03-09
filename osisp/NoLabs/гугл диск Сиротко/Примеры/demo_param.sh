echo '$0 =' "$0"
echo '$# =' "$#"
while [ $# -gt 0 ] ; do
	echo $1
	shift
done

