#!/bin/bash
    prefix=log
    suffix=$(date +%s)  # The "+%s" option to 'date' is GNU-specific.
    filename=$prefix.$suffix
    cp /var/oserver/oserverlog.log /var/oserver/logfiles/$filename.log
    filename=$prefix.$suffix
    cp /var/oserver/elog.xml /var/oserver/logfiles/$filename.xml
	DIR="/home/ftpaccount/serverupdate"
	if [ "$(ls -A $DIR)" ]; then
		chmod -R 777 /home/ftpaccount/serverupdate
		sudo cp -r /home/ftpaccount/serverupdate/* /var/oserver/ ;
		sudo rm -r /home/ftpaccount/serverupdate/* ;
		chmod -R 777 /var/oserver
	fi

chmod 777 emailalert.sh
./var/oserver/emailalert.sh	
exit 0

