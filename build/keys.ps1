# Note: these values may only change during major release

If ($Version.Contains('-')) {

	# Use the development keys
	$Keys = @{
		'netstandard1.1' = '71b21a4aa82ae3b3'
		'net452' = '71b21a4aa82ae3b3'
	}

} Else {

	# Use the final release keys
	$Keys = @{
		'netstandard1.1' = '2d9feb668c6a0e40'
		'net452' = '2d9feb668c6a0e40'
	}

}

function Resolve-FullPath() {
	param([string]$Path)
	[System.IO.Path]::GetFullPath((Join-Path (pwd) $Path))
}
