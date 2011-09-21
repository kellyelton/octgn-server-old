<?php 
	//start session and make sure IE follows certain guidelines, comment out if you wish
	session_start();
	echo '<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0//EN">';
	echo "Site is being updated...";
	die();
		//initialize global variables
	global $esd;	global $debug_file;
	include 'funs.php';
	//open debug file
	//$debug_file_path = 'temp/debug.php';
	$debug_file_path = '/home/skylabso/temp/debug.php';
	$debug_file = fopen($debug_file_path, 'w');
	if(!$debug_file)
		echo "Debug error";
	fwrite($debug_file,"<?xml version='1.0' standalone='yes'?>");
	fclose($debug_file);
	//*******************
	//set some variables
	//*******************
	global $dsep;
	//$dsep = "\\";	
	$dsep = "/";
	//$esd['rvars']['localsiteroot'] = "D:\\wamp\\www\\skylabs";
	$esd['rvars']['localsiteroot'] = "/home/skylabso/public_html";
	$esd['rvars']['tempdir'] = "/home/skylabso/public_html/temp";	//$esd['rvars']['tempdir'] = "d:\\wamp\\www\\skylabs\\temp";
	
	//includes
	include 'be/comfun.php';
	include 'be/parseesd.php';
	//Load_Config('defsf/conf.php');	
	Load_Config('defsf/confonline.php');
	load_plugins();   	//load plugins
	Start_Site();
	dwrite(print_r($esd, "true"), "esd");
	dwrite(print_r($_SESSION, "true"), "Session");	
	close_css();
	//fclose($debug_file);
?>