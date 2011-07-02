#!/bin/bash
    prefix=log
    suffix=$(date +%s)  # The "+%s" option to 'date' is GNU-specific.
    filename=$prefix.$suffix
    cp /var/oserver2/oserverlog.log /var/oserver2/logfiles/$filename.log
    filename=$prefix.$suffix
    cp /var/oserver2/elog.xml /var/oserver2/logfiles/$filename.xml
	DIR="/home/ftpaccount/serverupdate2"
	if [ "$(ls -A $DIR)" ]; then
		chmod -R 777 /home/ftpaccount/serverupdate2
		sudo cp -r /home/ftpaccount/serverupdate2/* /var/oserver2/ ;
		sudo rm -r /home/ftpaccount/serverupdate2/* ;
		chmod -R 777 /var/oserver2.0
	fi

chmod 777 emailalert.sh
./var/oserver2/emailalert.sh	
exit 0

