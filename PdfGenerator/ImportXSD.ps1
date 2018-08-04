$versions =  @('Community', 'Professional', 'Enterprise')



foreach ($v in $versions) {
	$path = "C:\Program Files (x86)\Microsoft Visual Studio\2017\$v\Common7\Tools\VsDevCmd.bat"

    if(Test-Path $path) {
		& $path
		xsd XMLImport.xsd /classes /namespace:Serilizer
		break
	}
}

