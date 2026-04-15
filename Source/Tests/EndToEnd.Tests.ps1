if ($PSVersionTable.PSVersion.Major -lt 6) {
    Write-Host "ERROR: This script requires PowerShell 6.0+ but the current version is: $($PSVersionTable.PSVersion)" -ForegroundColor Red
    exit 1
}

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$binary = Get-ChildItem -r "$root\AnsVaultCmd.exe" | select -First 1 | foreach FullName
$plainTextFile = Join-Path $root 'Source\Tests\TestFiles\text-plaintext.txt'
$plainText = Get-Content -Raw $plainTextFile
$cipherTextFile = Join-Path $root 'Source\Tests\TestFiles\text-ciphertext.txt'
$tempFile = Join-Path $env:TEMP 'AnsVaultCmd-Output.txt'

Describe 'Program Options' {

    It 'Shows the usage message and exits with a non-zero code when no arguments are provided' {
        $stdout = & "$binary" | Out-String
        $LastExitCode | Should Be 1
        $stdout | Should Match 'USAGE'
    }

    It 'Shows the usage message and exits with a zero code when help is requested' {
        $stdout = & "$binary" '--help' | Out-String
        $LastExitCode | Should Be 0
        $stdout | Should Match 'USAGE'

        $stdout = & "$binary" '-h' | Out-String
        $LastExitCode | Should Be 0
        $stdout | Should Match 'USAGE'
    }
}

Describe 'Program Version' {

    It 'Shows the version number by itself when the version is requested' {
        $stdout = & "$binary" '--version' | Out-String -NoNewline
        $LastExitCode | Should Be 0
        $stdout | measure | foreach Count | Should Be 1
        $stdout | Should Match '^\d+\.\d+\.\d+\.\d+$'
    }
}

Describe 'Decryption' {

    AfterEach {
        Remove-Item $tempFile -ErrorAction SilentlyContinue
    }

    It 'Raises an error when stdin is assumed and no password is provided' {
        $stdin = Get-Content -Raw $cipherTextFile
        $stdout = $stdin | & "$binary" 2>&1 | Out-String
        $LastExitCode | Should Be 1
        $stdout | select -First 1 | Should Match 'ERROR: No vault password specified'
    }

    It 'Decrypts the contents of stdin to stdout by default using the password provided' {
        $stdin = Get-Content -Raw $cipherTextFile
        $stdout = $stdin | & "$binary" '--password' 'password' | Out-String -NoNewline
        $LastExitCode | Should Be 0
        $stdout | Should BeExactly $plainText
    }

    It 'Decrypts the contents of the input file to stdout using the password provided' {
        $stdout = & "$binary" '--password' 'password' '--infile' $cipherTextFile | Out-String -NoNewline
        $LastExitCode | Should Be 0
        $stdout | Should BeExactly $plainText
    }

    It 'Decrypts the contents of stdin to the output file using the password provided' {
        $stdin = Get-Content -Raw $cipherTextFile
        $stdout = $stdin | & "$binary" '--password' 'password' '--outfile' $tempFile | Out-String -NoNewline
        $LastExitCode | Should Be 0
        $stdout | Should BeNullOrEmpty
        $tempFile | Should Contain $plainText
    }

    It 'Decrypts the contents of the input file to the output file using the password provided' {
        $stdout = & "$binary" '--password' 'password' '--infile' $cipherTextFile '--outfile' $tempFile | Out-String -NoNewline
        $LastExitCode | Should Be 0
        $stdout | Should BeNullOrEmpty
        $tempFile | Should Contain $plainText
    }

    It 'Ignores the contents of stdin when an input file has been specified' {
        $stdin = 'unencrypted text'
        $stdout = $stdin | & "$binary" '--password' 'password' '--infile' $cipherTextFile '--outfile' $tempFile | Out-String -NoNewline
        $LastExitCode | Should Be 0
        $stdout | Should BeNullOrEmpty
        $tempFile | Should Contain $plainText
    }
}
