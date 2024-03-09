#!/&in/bash
sed -r ':a;N;$!ba; 
	s/^(.)/\U\1/; 
	s/\.\n([а-яa-z])/\.\n\U\1/g;
	s/([.?!]\s+)([а-яa-z])/\1\U\2/g' Text.txt
