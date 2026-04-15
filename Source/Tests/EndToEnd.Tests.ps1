if ($PSVersionTable.PSVersion.Major -lt 6) {
    Write-Error "This script requires PowerShell 6.0+ but the current version is: $($PSVersionTable.PSVersion)"
    exit 1
}

$root = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$program = Get-ChildItem -r "$root\AnsVaultCmd.exe" |
            where { $_ -like '*\bin\*' } |
            sort -Desc LastWriteTime |
            select -First 1 |
            foreach FullName

$plainTextFile = Join-Path $root 'Source\Tests\TestFiles\text-plaintext.txt'
$plainText = Get-Content -Raw $plainTextFile
$cipherTextFile = Join-Path $root 'Source\Tests\TestFiles\text-ciphertext.txt'
$tempFile = Join-Path $env:TEMP 'AnsVaultCmd-Output.txt'

Describe 'Program Usage' {

    It 'Shows the usage message and exits with a non-zero code when no arguments are provided' {

        $stdout = & "$program" | Out-String

        $LastExitCode | Should Be 1
        $stdout | Should Match 'USAGE'
    }

    $HelpFlags = @(
        @{ helpFlag = '--help' }
        @{ helpFlag = '-h' }
    )
    It 'Shows the usage message and exits with a zero code when help is requested with <helpFlag>' -TestCases $HelpFlags {
        param($helpFlag)

        $stdout = & "$program" $helpFlag | Out-String

        $LastExitCode | Should Be 0
        $stdout | Should Match 'USAGE'
    }
}

Describe 'Program Version' {

    It 'Shows the version number by itself when the version is requested' {

        $stdout = & "$program" '--version' | Out-String -NoNewline

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

        $stdout = $stdin | & "$program" 2>&1 | Out-String

        $LastExitCode | Should Be 1
        $stdout | select -First 1 | Should Match 'ERROR: No vault password specified'
    }

    It 'Decrypts the contents of stdin to stdout by default using the password provided' {
        $stdin = Get-Content -Raw $cipherTextFile

        $stdout = $stdin | & "$program" '--password' 'password' | Out-String -NoNewline

        $LastExitCode | Should Be 0
        $stdout | Should BeExactly $plainText
    }

    It 'Decrypts the contents of the input file to stdout using the password provided' {

        $stdout = & "$program" '--password' 'password' '--infile' $cipherTextFile | Out-String -NoNewline

        $LastExitCode | Should Be 0
        $stdout | Should BeExactly $plainText
    }

    It 'Decrypts the contents of stdin to the output file using the password provided' {
        $stdin = Get-Content -Raw $cipherTextFile

        $stdout = $stdin | & "$program" '--password' 'password' '--outfile' $tempFile | Out-String -NoNewline

        $LastExitCode | Should Be 0
        $stdout | Should BeNullOrEmpty
        $tempFile | Should Contain $plainText
    }

    It 'Decrypts the contents of the input file to the output file using the password provided' {

        $stdout = & "$program" '--password' 'password' '--infile' $cipherTextFile '--outfile' $tempFile | Out-String -NoNewline

        $LastExitCode | Should Be 0
        $stdout | Should BeNullOrEmpty
        $tempFile | Should Contain $plainText
    }

    It 'Ignores the contents of stdin when an input file has been specified' {
        $stdin = 'unencrypted text'

        $stdout = $stdin | & "$program" '--password' 'password' '--infile' $cipherTextFile '--outfile' $tempFile | Out-String -NoNewline

        $LastExitCode | Should Be 0
        $stdout | Should BeNullOrEmpty
        $tempFile | Should Contain $plainText
    }
}
