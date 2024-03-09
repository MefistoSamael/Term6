#functions demonstration

Sqr ()
{
	expr $1 \* $1
}

Add () {
	expr $1 + $2
#	ps >&2
#	sleep 3
}

Mul ()
(
	expr $1 \* $2
#	ps >&2
#	sleep 3
)

Pow ()
{
	result=1 ; i=$2
	while [ $i -ge 1 ] ; do
		result=`expr $result \* $1`
		i=`expr $i - 1`
	done
	echo $result
}

Min ()
{
	if [ "$1" -lt "$2" ]
	then
		echo $1
	else
		echo $2
	fi
}

Max ()
{
	if [ $1 -gt $2 ]
	then
		echo $1
	else
		echo $2
	fi
}
#ps ; sleep 3
x=2; y=3
echo "x+y =" `Add $x $y`
echo "x*y =" `Mul $x $y`
echo "x^y = " `Pow $x $y`
echo "min(x,y) =" `Min $x $y`
echo "max(x,y) =" `Max $x $y`
