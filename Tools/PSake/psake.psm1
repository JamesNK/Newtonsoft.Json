# psake
# Copyright (c) 2012 James Kovacs
# Permission is hereby granted, free of charge, to any person obtaining a copy
# of this software and associated documentation files (the "Software"), to deal
# in the Software without restriction, including without limitation the rights
# to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
# copies of the Software, and to permit persons to whom the Software is
# furnished to do so, subject to the following conditions:
#
# The above copyright notice and this permission notice shall be included in
# all copies or substantial portions of the Software.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
# IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
# FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
# AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
# LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
# OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
# THE SOFTWARE.

#Requires -Version 2.0

#-- Public Module Functions --#

# .ExternalHelp  psake.psm1-help.xml
function Invoke-Task
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)] [string]$taskName
    )

    Assert $taskName ($msgs.error_invalid_task_name)

    $taskKey = $taskName.ToLower()

    if ($currentContext.aliases.Contains($taskKey)) {
        $taskName = $currentContext.aliases.$taskKey.Name
        $taskKey = $taskName.ToLower()
    }

    $currentContext = $psake.context.Peek()

    Assert ($currentContext.tasks.Contains($taskKey)) ($msgs.error_task_name_does_not_exist -f $taskName)

    if ($currentContext.executedTasks.Contains($taskKey))  { return }

    Assert (!$currentContext.callStack.Contains($taskKey)) ($msgs.error_circular_reference -f $taskName)

    $currentContext.callStack.Push($taskKey)

    $task = $currentContext.tasks.$taskKey

    $precondition_is_valid = & $task.Precondition

    if (!$precondition_is_valid) {
        WriteColoredOutput ($msgs.precondition_was_false -f $taskName) -foregroundcolor Cyan
    } else {
        if ($taskKey -ne 'default') {

            if ($task.PreAction -or $task.PostAction) {
                Assert ($task.Action -ne $null) ($msgs.error_missing_action_parameter -f $taskName)
            }

            if ($task.Action) {
                try {
                    foreach($childTask in $task.DependsOn) {
                        Invoke-Task $childTask
                    }

                    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                    $currentContext.currentTaskName = $taskName

                    & $currentContext.taskSetupScriptBlock

                    if ($task.PreAction) {
                        & $task.PreAction
                    }

                    if ($currentContext.config.taskNameFormat -is [ScriptBlock]) {
                        & $currentContext.config.taskNameFormat $taskName
                    } else {
                        WriteColoredOutput ($currentContext.config.taskNameFormat -f $taskName) -foregroundcolor Cyan
                    }

                    foreach ($variable in $task.requiredVariables) {
                        Assert ((test-path "variable:$variable") -and ((get-variable $variable).Value -ne $null)) ($msgs.required_variable_not_set -f $variable, $taskName)
                    }

                    & $task.Action

                    if ($task.PostAction) {
                        & $task.PostAction
                    }

                    & $currentContext.taskTearDownScriptBlock
                    $task.Duration = $stopwatch.Elapsed
                } catch {
                    if ($task.ContinueOnError) {
                        "-"*70
                        WriteColoredOutput ($msgs.continue_on_error -f $taskName,$_) -foregroundcolor Yellow
                        "-"*70
                        $task.Duration = $stopwatch.Elapsed
                    }  else {
                        throw $_
                    }
                }
            } else {
                # no action was specified but we still execute all the dependencies
                foreach($childTask in $task.DependsOn) {
                    Invoke-Task $childTask
                }
            }
        } else {
            foreach($childTask in $task.DependsOn) {
                Invoke-Task $childTask
            }
        }

        Assert (& $task.Postcondition) ($msgs.postcondition_failed -f $taskName)
    }

    $poppedTaskKey = $currentContext.callStack.Pop()
    Assert ($poppedTaskKey -eq $taskKey) ($msgs.error_corrupt_callstack -f $taskKey,$poppedTaskKey)

    $currentContext.executedTasks.Push($taskKey)
}

# .ExternalHelp  psake.psm1-help.xml
function Exec
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
        [Parameter(Position=1,Mandatory=0)][string]$errorMessage = ($msgs.error_bad_command -f $cmd),
        [Parameter(Position=2,Mandatory=0)][int]$maxRetries = 0,
        [Parameter(Position=3,Mandatory=0)][string]$retryTriggerErrorPattern = $null
    )

    $tryCount = 1

    do {
        try {
            $global:lastexitcode = 0
            & $cmd
            if ($lastexitcode -ne 0) {
                throw ("Exec: " + $errorMessage)
            }
            break
        }
        catch [Exception]
        {
            if ($tryCount -gt $maxRetries) {
                throw $_
            }

            if ($retryTriggerErrorPattern -ne $null) {
                $isMatch = [regex]::IsMatch($_.Exception.Message, $retryTriggerErrorPattern)

                if ($isMatch -eq $false) {
                    throw $_
                }
            }

            Write-Host "Try $tryCount failed, retrying again in 1 second..."

            $tryCount++

            [System.Threading.Thread]::Sleep([System.TimeSpan]::FromSeconds(1))
        }
    }
    while ($true)
}

# .ExternalHelp  psake.psm1-help.xml
function Assert
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)]$conditionToCheck,
        [Parameter(Position=1,Mandatory=1)]$failureMessage
    )
    if (!$conditionToCheck) {
        throw ("Assert: " + $failureMessage)
    }
}

# .ExternalHelp  psake.psm1-help.xml
function Task
{
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][string]$name = $null,
        [Parameter(Position=1,Mandatory=0)][scriptblock]$action = $null,
        [Parameter(Position=2,Mandatory=0)][scriptblock]$preaction = $null,
        [Parameter(Position=3,Mandatory=0)][scriptblock]$postaction = $null,
        [Parameter(Position=4,Mandatory=0)][scriptblock]$precondition = {$true},
        [Parameter(Position=5,Mandatory=0)][scriptblock]$postcondition = {$true},
        [Parameter(Position=6,Mandatory=0)][switch]$continueOnError = $false,
        [Parameter(Position=7,Mandatory=0)][string[]]$depends = @(),
        [Parameter(Position=8,Mandatory=0)][string[]]$requiredVariables = @(),
        [Parameter(Position=9,Mandatory=0)][string]$description = $null,
        [Parameter(Position=10,Mandatory=0)][string]$alias = $null,
        [Parameter(Position=11,Mandatory=0)][string]$maxRetries = 0,
        [Parameter(Position=12,Mandatory=0)][string]$retryTriggerErrorPattern = $null
    )
    if ($name -eq 'default') {
        Assert (!$action) ($msgs.error_default_task_cannot_have_action)
    }

    $newTask = @{
        Name = $name
        DependsOn = $depends
        PreAction = $preaction
        Action = $action
        PostAction = $postaction
        Precondition = $precondition
        Postcondition = $postcondition
        ContinueOnError = $continueOnError
        Description = $description
        Duration = [System.TimeSpan]::Zero
        RequiredVariables = $requiredVariables
        Alias = $alias
        MaxRetries = $maxRetries
        RetryTriggerErrorPattern = $retryTriggerErrorPattern
    }

    $taskKey = $name.ToLower()

    $currentContext = $psake.context.Peek()

    Assert (!$currentContext.tasks.ContainsKey($taskKey)) ($msgs.error_duplicate_task_name -f $name)

    $currentContext.tasks.$taskKey = $newTask

    if($alias)
    {
        $aliasKey = $alias.ToLower()

        Assert (!$currentContext.aliases.ContainsKey($aliasKey)) ($msgs.error_duplicate_alias_name -f $alias)

        $currentContext.aliases.$aliasKey = $newTask
    }
}

# .ExternalHelp  psake.psm1-help.xml
function Properties {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$properties
    )
    $psake.context.Peek().properties += $properties
}

# .ExternalHelp  psake.psm1-help.xml
function Include {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][string]$fileNamePathToInclude
    )
    Assert (test-path $fileNamePathToInclude -pathType Leaf) ($msgs.error_invalid_include_path -f $fileNamePathToInclude)
    $psake.context.Peek().includes.Enqueue((Resolve-Path $fileNamePathToInclude));
}

# .ExternalHelp  psake.psm1-help.xml
function FormatTaskName {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)]$format
    )
    $psake.context.Peek().config.taskNameFormat = $format
}

# .ExternalHelp  psake.psm1-help.xml
function TaskSetup {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$setup
    )
    $psake.context.Peek().taskSetupScriptBlock = $setup
}

# .ExternalHelp  psake.psm1-help.xml
function TaskTearDown {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][scriptblock]$teardown
    )
    $psake.context.Peek().taskTearDownScriptBlock = $teardown
}

# .ExternalHelp  psake.psm1-help.xml
function Framework {
    [CmdletBinding()]
    param(
        [Parameter(Position=0,Mandatory=1)][string]$framework
    )
    $psake.context.Peek().config.framework = $framework
    ConfigureBuildEnvironment
}

# .ExternalHelp  psake.psm1-help.xml
function Invoke-psake {
    [CmdletBinding()]
    param(
        [Parameter(Position = 0, Mandatory = 0)][string] $buildFile,
        [Parameter(Position = 1, Mandatory = 0)][string[]] $taskList = @(),
        [Parameter(Position = 2, Mandatory = 0)][string] $framework,
        [Parameter(Position = 3, Mandatory = 0)][switch] $docs = $false,
        [Parameter(Position = 4, Mandatory = 0)][hashtable] $parameters = @{},
        [Parameter(Position = 5, Mandatory = 0)][hashtable] $properties = @{},
        [Parameter(Position = 6, Mandatory = 0)][alias("init")][scriptblock] $initialization = {},
        [Parameter(Position = 7, Mandatory = 0)][switch] $nologo = $false,
        [Parameter(Position = 8, Mandatory = 0)][switch] $detailedDocs = $false
    )
    try {
        if (-not $nologo) {
            "psake version {0}`nCopyright (c) 2010-2015 James Kovacs, Damian Hickey & Contributors`n" -f $psake.version
        }

        if (!$buildFile) {
          $buildFile = $psake.config_default.buildFileName
        }
        elseif (!(test-path $buildFile -pathType Leaf) -and (test-path $psake.config_default.buildFileName -pathType Leaf)) {
            # If the $config.buildFileName file exists and the given "buildfile" isn 't found assume that the given
            # $buildFile is actually the target Tasks to execute in the $config.buildFileName script.
            $taskList = $buildFile.Split(', ')
            $buildFile = $psake.config_default.buildFileName
        }

        # Execute the build file to set up the tasks and defaults
        Assert (test-path $buildFile -pathType Leaf) ($msgs.error_build_file_not_found -f $buildFile)

        $psake.build_script_file = get-item $buildFile
        $psake.build_script_dir = $psake.build_script_file.DirectoryName
        $psake.build_success = $false

        $psake.context.push(@{
            "taskSetupScriptBlock" = {};
            "taskTearDownScriptBlock" = {};
            "executedTasks" = new-object System.Collections.Stack;
            "callStack" = new-object System.Collections.Stack;
            "originalEnvPath" = $env:path;
            "originalDirectory" = get-location;
            "originalErrorActionPreference" = $global:ErrorActionPreference;
            "tasks" = @{};
            "aliases" = @{};
            "properties" = @();
            "includes" = new-object System.Collections.Queue;
            "config" = CreateConfigurationForNewContext $buildFile $framework
        })

        LoadConfiguration $psake.build_script_dir

        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

        set-location $psake.build_script_dir

        LoadModules

        $frameworkOldValue = $framework
        . $psake.build_script_file.FullName

        $currentContext = $psake.context.Peek()

        if ($framework -ne $frameworkOldValue) {
            writecoloredoutput $msgs.warning_deprecated_framework_variable -foregroundcolor Yellow
            $currentContext.config.framework = $framework
        }

        ConfigureBuildEnvironment

        while ($currentContext.includes.Count -gt 0) {
            $includeFilename = $currentContext.includes.Dequeue()
            . $includeFilename
        }

        if ($docs -or $detailedDocs) {
            WriteDocumentation($detailedDocs)
            CleanupEnvironment
            return
        }

        foreach ($key in $parameters.keys) {
            if (test-path "variable:\$key") {
                set-item -path "variable:\$key" -value $parameters.$key -WhatIf:$false -Confirm:$false | out-null
            } else {
                new-item -path "variable:\$key" -value $parameters.$key -WhatIf:$false -Confirm:$false | out-null
            }
        }

        # The initial dot (.) indicates that variables initialized/modified in the propertyBlock are available in the parent scope.
        foreach ($propertyBlock in $currentContext.properties) {
            . $propertyBlock
        }

        foreach ($key in $properties.keys) {
            if (test-path "variable:\$key") {
                set-item -path "variable:\$key" -value $properties.$key -WhatIf:$false -Confirm:$false | out-null
            }
        }

        # Simple dot sourcing will not work. We have to force the script block into our
        # module's scope in order to initialize variables properly.
        . $MyInvocation.MyCommand.Module $initialization

        # Execute the list of tasks or the default task
        if ($taskList) {
            foreach ($task in $taskList) {
                invoke-task $task
            }
        } elseif ($currentContext.tasks.default) {
            invoke-task default
        } else {
            throw $msgs.error_no_default_task
        }

        WriteColoredOutput ("`n" + $msgs.build_success + "`n") -foregroundcolor Green

        WriteTaskTimeSummary $stopwatch.Elapsed

        $psake.build_success = $true
    } catch {
        $currentConfig = GetCurrentConfigurationOrDefault
        if ($currentConfig.verboseError) {
            $error_message = "{0}: An Error Occurred. See Error Details Below: `n" -f (Get-Date)
            $error_message += ("-" * 70) + "`n"
            $error_message += "Error: {0}`n" -f (ResolveError $_ -Short)
            $error_message += ("-" * 70) + "`n"
            $error_message += ResolveError $_
            $error_message += ("-" * 70) + "`n"
            $error_message += "Script Variables" + "`n"
            $error_message += ("-" * 70) + "`n"
            $error_message += get-variable -scope script | format-table | out-string
        } else {
            # ($_ | Out-String) gets error messages with source information included.
            $error_message = "Error: {0}: `n{1}" -f (Get-Date), (ResolveError $_ -Short)
        }

        $psake.build_success = $false

        # if we are running in a nested scope (i.e. running a psake script from a psake script) then we need to re-throw the exception
        # so that the parent script will fail otherwise the parent script will report a successful build
        $inNestedScope = ($psake.context.count -gt 1)
        if ( $inNestedScope ) {
            throw $_
        } else {
            if (!$psake.run_by_psake_build_tester) {
                WriteColoredOutput $error_message -foregroundcolor Red
            }
        }
    } finally {
        CleanupEnvironment
    }
}

#-- Private Module Functions --#
function WriteColoredOutput {
    param(
        [string] $message,
        [System.ConsoleColor] $foregroundcolor
    )

    $currentConfig = GetCurrentConfigurationOrDefault
    if ($currentConfig.coloredOutput -eq $true) {
        if (($Host.UI -ne $null) -and ($Host.UI.RawUI -ne $null) -and ($Host.UI.RawUI.ForegroundColor -ne $null)) {
            $previousColor = $Host.UI.RawUI.ForegroundColor
            $Host.UI.RawUI.ForegroundColor = $foregroundcolor
        }
    }

    $message

    if ($previousColor -ne $null) {
        $Host.UI.RawUI.ForegroundColor = $previousColor
    }
}

function LoadModules {
    $currentConfig = $psake.context.peek().config
    if ($currentConfig.modules) {

        $scope = $currentConfig.moduleScope

        $global = [string]::Equals($scope, "global", [StringComparison]::CurrentCultureIgnoreCase)

        $currentConfig.modules | foreach {
            resolve-path $_ | foreach {
                "Loading module: $_"
                $module = import-module $_ -passthru -DisableNameChecking -global:$global
                if (!$module) {
                    throw ($msgs.error_loading_module -f $_.Name)
                }
            }
        }
        ""
    }
}

function LoadConfiguration {
    param(
        [string] $configdir = $PSScriptRoot
    )

    $psakeConfigFilePath = (join-path $configdir "psake-config.ps1")

    if (test-path $psakeConfigFilePath -pathType Leaf) {
        try {
            $config = GetCurrentConfigurationOrDefault
            . $psakeConfigFilePath
        } catch {
            throw "Error Loading Configuration from psake-config.ps1: " + $_
        }
    }
}

function GetCurrentConfigurationOrDefault() {
    if ($psake.context.count -gt 0) {
        return $psake.context.peek().config
    } else {
        return $psake.config_default
    }
}

function CreateConfigurationForNewContext {
    param(
        [string] $buildFile,
        [string] $framework
    )

    $previousConfig = GetCurrentConfigurationOrDefault

    $config = new-object psobject -property @{
        buildFileName = $previousConfig.buildFileName;
        framework = $previousConfig.framework;
        taskNameFormat = $previousConfig.taskNameFormat;
        verboseError = $previousConfig.verboseError;
        coloredOutput = $previousConfig.coloredOutput;
        modules = $previousConfig.modules;
        moduleScope =  $previousConfig.moduleScope;
    }

    if ($framework) {
        $config.framework = $framework;
    }

    if ($buildFile) {
        $config.buildFileName = $buildFile;
    }

    return $config
}

function ConfigureBuildEnvironment {
    $framework = $psake.context.peek().config.framework
    if ($framework -cmatch '^((?:\d+\.\d+)(?:\.\d+){0,1})(x86|x64){0,1}$') {
        $versionPart = $matches[1]
        $bitnessPart = $matches[2]
    } else {
        throw ($msgs.error_invalid_framework -f $framework)
    }
    $versions = $null
    $buildToolsVersions = $null
    switch ($versionPart) {
        '1.0' {
            $versions = @('v1.0.3705')
        }
        '1.1' {
            $versions = @('v1.1.4322')
        }
        '2.0' {
            $versions = @('v2.0.50727')
        }
        '3.0' {
            $versions = @('v2.0.50727')
        }
        '3.5' {
            $versions = @('v3.5', 'v2.0.50727')
        }
        '4.0' {
            $versions = @('v4.0.30319')
        }
        {($_ -eq '4.5.1') -or ($_ -eq '4.5.2')} {
            $versions = @('v4.0.30319')
            $buildToolsVersions = @('14.0', '12.0')
        }
        '4.6' {
            $versions = @('v4.0.30319')
            $buildToolsVersions = @('14.0')
        }

        default {
            throw ($msgs.error_unknown_framework -f $versionPart, $framework)
        }
    }

    $bitness = 'Framework'
    if ($versionPart -ne '1.0' -and $versionPart -ne '1.1') {
        switch ($bitnessPart) {
            'x86' {
                $bitness = 'Framework'
                $buildToolsKey = 'MSBuildToolsPath32'
            }
            'x64' {
                $bitness = 'Framework64'
                $buildToolsKey = 'MSBuildToolsPath'
            }
            { [string]::IsNullOrEmpty($_) } {
                $ptrSize = [System.IntPtr]::Size
                switch ($ptrSize) {
                    4 {
                        $bitness = 'Framework'
                        $buildToolsKey = 'MSBuildToolsPath32'
                    }
                    8 {
                        $bitness = 'Framework64'
                        $buildToolsKey = 'MSBuildToolsPath'
                    }
                    default {
                        throw ($msgs.error_unknown_pointersize -f $ptrSize)
                    }
                }
            }
            default {
                throw ($msgs.error_unknown_bitnesspart -f $bitnessPart, $framework)
            }
        }
    }
    $frameworkDirs = @()
    if ($buildToolsVersions -ne $null) {
        foreach($ver in $buildToolsVersions) {
            if (Test-Path "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\$ver") {
                $frameworkDirs += (Get-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\$ver" -Name $buildToolsKey).$buildToolsKey
            }
        }
    }
    $frameworkDirs = $frameworkDirs + @($versions | foreach { "$env:windir\Microsoft.NET\$bitness\$_\" })

    for ($i = 0; $i -lt $frameworkDirs.Count; $i++) {
        $dir = $frameworkDirs[$i]
        if ($dir -Match "\$\(Registry:HKEY_LOCAL_MACHINE(.*?)@(.*)\)") {
            $key = "HKLM:" + $matches[1]
            $name = $matches[2]
            $dir = (Get-ItemProperty -Path $key -Name $name).$name
            $frameworkDirs[$i] = $dir
        }
    }

    $frameworkDirs | foreach { Assert (test-path $_ -pathType Container) ($msgs.error_no_framework_install_dir_found -f $_)}

    $env:path = ($frameworkDirs -join ";") + ";$env:path"
    # if any error occurs in a PS function then "stop" processing immediately
    # this does not effect any external programs that return a non-zero exit code
    $global:ErrorActionPreference = "Stop"
}

function CleanupEnvironment {
    if ($psake.context.Count -gt 0) {
        $currentContext = $psake.context.Peek()
        $env:path = $currentContext.originalEnvPath
        Set-Location $currentContext.originalDirectory
        $global:ErrorActionPreference = $currentContext.originalErrorActionPreference
        [void] $psake.context.Pop()
    }
}

function SelectObjectWithDefault
{
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)]
        [PSObject]
        $InputObject,
        [string]
        $Name,
        $Value
    )

    process {
        if ($_ -eq $null) { $Value }
        elseif ($_ | Get-Member -Name $Name) {
          $_.$Name
        }
        elseif (($_ -is [Hashtable]) -and ($_.Keys -contains $Name)) {
          $_.$Name
        }
        else { $Value }
    }
}

# borrowed from Jeffrey Snover http://blogs.msdn.com/powershell/archive/2006/12/07/resolve-error.aspx
# modified to better handle SQL errors
function ResolveError
{
    [CmdletBinding()]
    param(
        [Parameter(ValueFromPipeline=$true)]
        $ErrorRecord=$Error[0],
        [Switch]
        $Short
    )

    process {
        if ($_ -eq $null) { $_ = $ErrorRecord }
        $ex = $_.Exception

        if (-not $Short) {
            $error_message = "`nErrorRecord:{0}ErrorRecord.InvocationInfo:{1}Exception:`n{2}"
            $formatted_errorRecord = $_ | format-list * -force | out-string
            $formatted_invocationInfo = $_.InvocationInfo | format-list * -force | out-string
            $formatted_exception = ''

            $i = 0
            while ($ex -ne $null) {
                $i++
                $formatted_exception += ("$i" * 70) + "`n" +
                    ($ex | format-list * -force | out-string) + "`n"
                $ex = $ex | SelectObjectWithDefault -Name 'InnerException' -Value $null
            }

            return $error_message -f $formatted_errorRecord, $formatted_invocationInfo, $formatted_exception
        }

        $lastException = @()
        while ($ex -ne $null) {
            $lastMessage = $ex | SelectObjectWithDefault -Name 'Message' -Value ''
            $lastException += ($lastMessage -replace "`n", '')
            if ($ex -is [Data.SqlClient.SqlException]) {
                $lastException += "(Line [$($ex.LineNumber)] " +
                    "Procedure [$($ex.Procedure)] Class [$($ex.Class)] " +
                    " Number [$($ex.Number)] State [$($ex.State)] )"
            }
            $ex = $ex | SelectObjectWithDefault -Name 'InnerException' -Value $null
        }
        $shortException = $lastException -join ' --> '

        $header = $null
        $current = $_
        $header = (($_.InvocationInfo |
            SelectObjectWithDefault -Name 'PositionMessage' -Value '') -replace "`n", ' '),
            ($_ | SelectObjectWithDefault -Name 'Message' -Value ''),
            ($_ | SelectObjectWithDefault -Name 'Exception' -Value '') |
                ? { -not [String]::IsNullOrEmpty($_) } |
                Select -First 1

        $delimiter = ''
        if ((-not [String]::IsNullOrEmpty($header)) -and
            (-not [String]::IsNullOrEmpty($shortException)))
            { $delimiter = ' [<<==>>] ' }

        return "$($header)$($delimiter)Exception: $($shortException)"
    }
}

function WriteDocumentation($showDetailed) {
    $currentContext = $psake.context.Peek()

    if ($currentContext.tasks.default) {
        $defaultTaskDependencies = $currentContext.tasks.default.DependsOn
    } else {
        $defaultTaskDependencies = @()
    }

    $docs = $currentContext.tasks.Keys | foreach-object {
        if ($_ -eq "default") {
            return
        }

        $task = $currentContext.tasks.$_
        new-object PSObject -property @{
            Name = $task.Name;
            Alias = $task.Alias;
            Description = $task.Description;
            "Depends On" = $task.DependsOn -join ", "
            Default = if ($defaultTaskDependencies -contains $task.Name) { $true }
        }
    }
    if ($showDetailed) {
        $docs | sort 'Name' | format-list -property Name,Alias,Description,"Depends On",Default
    } else {
        $docs | sort 'Name' | format-table -autoSize -wrap -property Name,Alias,"Depends On",Default,Description
    }

}

function WriteTaskTimeSummary($invokePsakeDuration) {
    if ($psake.context.count -gt 0) {
        "-" * 70
        "Build Time Report"
        "-" * 70
        $list = @()
        $currentContext = $psake.context.Peek()
        while ($currentContext.executedTasks.Count -gt 0) {
            $taskKey = $currentContext.executedTasks.Pop()
            $task = $currentContext.tasks.$taskKey
            if ($taskKey -eq "default") {
                continue
            }
            $list += new-object PSObject -property @{
                Name = $task.Name;
                Duration = $task.Duration
            }
        }
        [Array]::Reverse($list)
        $list += new-object PSObject -property @{
            Name = "Total:";
            Duration = $invokePsakeDuration
        }
        # using "out-string | where-object" to filter out the blank line that format-table prepends
        $list | format-table -autoSize -property Name,Duration | out-string -stream | where-object { $_ }
    }
}

DATA msgs {
convertfrom-stringdata @'
    error_invalid_task_name = Task name should not be null or empty string.
    error_task_name_does_not_exist = Task {0} does not exist.
    error_circular_reference = Circular reference found for task {0}.
    error_missing_action_parameter = Action parameter must be specified when using PreAction or PostAction parameters for task {0}.
    error_corrupt_callstack = Call stack was corrupt. Expected {0}, but got {1}.
    error_invalid_framework = Invalid .NET Framework version, {0} specified.
    error_unknown_framework = Unknown .NET Framework version, {0} specified in {1}.
    error_unknown_pointersize = Unknown pointer size ({0}) returned from System.IntPtr.
    error_unknown_bitnesspart = Unknown .NET Framework bitness, {0}, specified in {1}.
    error_no_framework_install_dir_found = No .NET Framework installation directory found at {0}.
    error_bad_command = Error executing command {0}.
    error_default_task_cannot_have_action = 'default' task cannot specify an action.
    error_duplicate_task_name = Task {0} has already been defined.
    error_duplicate_alias_name = Alias {0} has already been defined.
    error_invalid_include_path = Unable to include {0}. File not found.
    error_build_file_not_found = Could not find the build file {0}.
    error_no_default_task = 'default' task required.
    error_loading_module = Error loading module {0}.
    warning_deprecated_framework_variable = Warning: Using global variable $framework to set .NET framework version used is deprecated. Instead use Framework function or configuration file psake-config.ps1.
    required_variable_not_set = Variable {0} must be set to run task {1}.
    postcondition_failed = Postcondition failed for task {0}.
    precondition_was_false = Precondition was false, not executing task {0}.
    continue_on_error = Error in task {0}. {1}
    build_success = Build Succeeded!
'@
}

import-localizeddata -bindingvariable msgs -erroraction silentlycontinue

$script:psake = @{}
$psake.version = "4.4.2" # contains the current version of psake
$psake.context = new-object system.collections.stack # holds onto the current state of all variables
$psake.run_by_psake_build_tester = $false # indicates that build is being run by psake-BuildTester
$psake.config_default = new-object psobject -property @{
    buildFileName = "default.ps1";
    framework = "4.0";
    taskNameFormat = "Executing {0}";
    verboseError = $false;
    coloredOutput = $true;
    modules = $null;
    moduleScope = "";
} # contains default configuration, can be overriden in psake-config.ps1 in directory with psake.psm1 or in directory with current build script

$psake.build_success = $false # indicates that the current build was successful
$psake.build_script_file = $null # contains a System.IO.FileInfo for the current build script
$psake.build_script_dir = "" # contains a string with fully-qualified path to current build script

LoadConfiguration

export-modulemember -function Invoke-psake, Invoke-Task, Task, Properties, Include, FormatTaskName, TaskSetup, TaskTearDown, Framework, Assert, Exec -variable psake
