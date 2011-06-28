#!/bin/bash
    prefix=log
    suffix=$(date +%s)  # The "+%s" option to 'date' is GNU-specific.
    filename=$prefix.$suffix
    cp /var/oserver2.0/oserverlog.log /var/oserver2.0/logfiles/$filename.log
    filename=$prefix.$suffix
    cp /var/oserver2.0/elog.xml /var/oserver2.0/logfiles/$filename.xml
	DIR="/home/ftpaccount/serverupdate"
	if [ "$(ls -A $DIR)" ]; then
		chmod -R 777 /home/ftpaccount/serverupdate
		sudo cp -r /home/ftpaccount/serverupdate/* /var/oserver2.0/ ;
		sudo rm -r /home/ftpaccount/serverupdate/* ;
		chmod -R 777 /var/oserver2.0
	fi

chmod 777 emailalert.sh
./var/oserver2.0/emailalert.sh	
exit 0

