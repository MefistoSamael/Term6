cd /home
mkdir g853501 g853502 g853503 g853504 g853505
useradd g853501 -d /home/g853501 -g users
useradd g853502 -d /home/g853502 -g users
useradd g853503 -d /home/g853503 -g users
useradd g853504 -d /home/g853504 -g users
useradd g853505 -d /home/g853505 -g users
passwd g853501
passwd g853502
passwd g853503
passwd g853504
passwd g853505
chgrp users g85350?
chown g853501 g853501
chown g853502 g853502
chown g853503 g853503
chown g853504 g853504
chown g853505 g853505
chmod 710 g85350?
