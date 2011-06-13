#!/bin/bash
function fappend {
    echo "$2">>$1;
}
YYYYMMDD=`date +%Y%m%d`

# CHANGE THESE
TOEMAIL="7015807400@vtext.com";
FREMAIL="root";
SUBJECT="Server2.0 stopped.";
MSGBODY="Server2.0 stopped for some reason";

# DON'T CHANGE ANYTHING BELOW
TMP="/tmp/tmpfil_123"$RANDOM;

rm -rf $TMP;
fappend $TMP "From: $FREMAIL";
fappend $TMP "To: $TOEMAIL";
fappend $TMP "Reply-To: $FREMAIL";
fappend $TMP "Subject: $SUBJECT";
fappend $TMP "";
fappend $TMP "$MSGBODY";
fappend $TMP "";
fappend $TMP "";
cat $TMP|sendmail -t;
rm $TMP;



