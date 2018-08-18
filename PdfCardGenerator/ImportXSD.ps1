$versions =  @('Community', 'Professional', 'Enterprise')



foreach ($v in $versions) {
	$path = "C:\Program Files (x86)\Microsoft Visual Studio\2017\$v\Common7\Tools\VsDevCmd.bat"

    if(Test-Path $path) {
		cmd /c """$path""&xsd XMLImport.xsd /classes /namespace:Serilizer"
		$output = 'XMLImport.cs'
		(Get-Content $output).replace('public', 'internal') | Set-Content $output
		break
	}
}

