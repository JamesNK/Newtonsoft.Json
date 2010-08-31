# psake
# Copyright (c) 2010 James Kovacs
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

#-- Private Module Variables (Listed here for quick reference)
[system.collections.stack]$script:context

#-- Public Module Variables -- The psake hashtable variable is initialized in the invoke-psake function
$script:psake = @{}
$script:psake.use_exit_on_error = $false    # determines if psake uses the "exit()" function when an exception occurs
$script:psake.log_error = $false            # determines if the exception details are written to a file
$script:psake.build_success = $false        # indicates that the current build was successful
$script:psake.version = "4.00"              # contains the current version of psake
$script:psake.build_script_file = $null     # contains a System.IO.FileInfo for the current build file
$script:psake.framework_version = ""        # contains the framework version # for the current build
$script:psake.default_build_file_name = 'default.ps1'

Export-ModuleMember -Variable "psake"

#-- Private Module Functions
function ExecuteTask
{
  param([string]$taskName)

  Assert (![string]::IsNullOrEmpty($taskName)) "Task name should not be null or empty string"

  $taskKey = $taskName.ToLower()

    Assert ($script:context.Peek().tasks.Contains($taskKey)) "task [$taskName] does not exist"

    if ($script:context.Peek().executedTasks.Contains($taskKey))
  {
    return
  }

  Assert (!$script:context.Peek().callStack.Contains($taskKey)) "Error: Circular reference found for task, $taskName"

  $script:context.Peek().callStack.Push($taskKey)

  $task = $script:context.Peek().tasks.$taskKey

  $taskName = $task.Name

  $precondition_is_valid = if ($task.Precondition -ne $null) {& $task.Precondition} else {$true}

  if (!$precondition_is_valid)
  {
    "Precondition was false not executing $name"
  }
  else
  {
    if ($taskKey -ne 'default')
    {
      $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

      if ( ($task.PreAction -ne $null) -or ($task.PostAction -ne $null) )
      {
        Assert ($task.Action -ne $null) "Error: Action parameter must be specified when using PreAction or PostAction parameters"
      }

      if ($task.Action -ne $null)
      {
        try
        {
          foreach($childTask in $task.DependsOn)
          {
            ExecuteTask $childTask
          }

          $script:context.Peek().currentTaskName = $taskName

          if ($script:context.Peek().taskSetupScriptBlock -ne $null)
          {
            & $script:context.Peek().taskSetupScriptBlock
          }

          if ($task.PreAction -ne $null)
          {
            & $task.PreAction
          }

          $script:context.Peek().formatTaskNameString -f $taskName
          & $task.Action

          if ($task.PostAction -ne $null)
          {
            & $task.PostAction
          }

          if ($script:context.Peek().taskTearDownScriptBlock -ne $null)
          {
            & $script:context.Peek().taskTearDownScriptBlock
          }
        }
        catch
        {
          if ($task.ContinueOnError)
          {
            "-"*70
            "Error in Task [$taskName] $_"
            "-"*70
          }
          else
          {
            throw $_
          }
        }
      } # if ($task.Action -ne $null)
      else
      {
        #no Action was specified but we still execute all the dependencies
        foreach($childTask in $task.DependsOn)
        {
          ExecuteTask $childTask
        }
      }
      $stopwatch.stop()
      $task.Duration = $stopwatch.Elapsed
    } # if ($name.ToLower() -ne 'default')
    else
    {
      foreach($childTask in $task.DependsOn)
      {
        ExecuteTask $childTask
      }
    }

    if ($task.Postcondition -ne $null)
    {
      Assert (& $task.Postcondition) "Error: Postcondition failed for $taskName"
    }
  }

  $poppedTaskKey = $script:context.Peek().callStack.Pop()

  Assert ($poppedTaskKey -eq $taskKey) "Error: CallStack was corrupt. Expected $taskKey, but got $poppedTaskKey."

  $script:context.Peek().executedTasks.Push($taskKey)
}

function Configure-BuildEnvironment
{
  if ($framework.Length -ne 3 -and $framework.Length -ne 6) {
    throw "Error: Invalid .NET Framework version, $framework, specified"
  }
  $versionPart = $framework.Substring(0,3)
  $bitnessPart = $framework.Substring(3)
  $versions = $null
  switch ($versionPart)
  {
    '1.0' { $versions = @('v1.0.3705')  }
    '1.1' { $versions = @('v1.1.4322')  }
    '2.0' { $versions = @('v2.0.50727') }
    '3.0' { $versions = @('v2.0.50727') }
    '3.5' { $versions = @('v3.5','v2.0.50727') }
    '4.0' { $versions = @('v4.0.30319') }
    default { throw "Error: Unknown .NET Framework version, $versionPart, specified in $framework" }
  }

  $bitness = 'Framework'
  if($versionPart -ne '1.0' -and $versionPart -ne '1.1') {
    switch ($bitnessPart)
    {
      'x86' { $bitness = 'Framework' }
      'x64' { $bitness = 'Framework64' }
      $null {
        $ptrSize = [System.IntPtr]::Size
        switch ($ptrSize)
        {
          4 { $bitness = 'Framework' }
          8 { $bitness = 'Framework64' }
          default { throw "Error: Unknown pointer size ($ptrSize) returned from System.IntPtr." }
        }
      }
      default { throw "Error: Unknown .NET Framework bitness, $bitnessPart, specified in $framework" }
    }
  }
  $frameworkDirs = $versions | foreach { "$env:windir\Microsoft.NET\$bitness\$_\" }

  $frameworkDirs | foreach { Assert (test-path $_) "Error: No .NET Framework installation directory found at $_" }

  $env:path = [string]::Join(';', $frameworkDirs) + ";$env:path"
  #if any error occurs in a PS function then "stop" processing immediately
  # this does not effect any external programs that return a non-zero exit code
  $global:ErrorActionPreference = "Stop"
}

function Cleanup-Environment
{
  $env:path = $script:context.Peek().originalEnvPath
  Set-Location $script:context.Peek().originalDirectory
  $global:ErrorActionPreference = $script:context.Peek().originalErrorActionPreference
}

#borrowed from Jeffrey Snover http://blogs.msdn.com/powershell/archive/2006/12/07/resolve-error.aspx
function Resolve-Error($ErrorRecord=$Error[0])
{
  "ErrorRecord"
  $ErrorRecord | Format-List * -Force | Out-String -Stream | ? {$_}
  ""
  "ErrorRecord.InvocationInfo"
  $ErrorRecord.InvocationInfo | Format-List * | Out-String -Stream | ? {$_}
  ""
  "Exception"
  $Exception = $ErrorRecord.Exception
  for ($i = 0; $Exception; $i++, ($Exception = $Exception.InnerException))
  {
    "$i" * 70
    $Exception | Format-List * -Force | Out-String -Stream | ? {$_}
    ""
  }
}

function Write-Documentation
{
  $list = New-Object System.Collections.ArrayList
  foreach($key in $script:context.Peek().tasks.Keys)
  {
    if($key -eq "default")
    {
      continue
    }
    $task = $script:context.Peek().tasks.$key
    $content = "" | Select-Object Name, Description
    $content.Name = $task.Name
    $content.Description = $task.Description
    $index = $list.Add($content)
  }

  $list | Sort 'Name' | Format-Table -Auto
}

function Write-TaskTimeSummary
{
  "-"*70
  "Build Time Report"
  "-"*70
  $list = @()
  while ($script:context.Peek().executedTasks.Count -gt 0)
  {
    $taskKey = $script:context.Peek().executedTasks.Pop()
    $task = $script:context.Peek().tasks.$taskKey
    if($taskKey -eq "default")
    {
      continue
    }
    $list += "" | Select-Object @{Name="Name";Expression={$task.Name}}, @{Name="Duration";Expression={$task.Duration}}
  }
  [Array]::Reverse($list)
  $list += "" | Select-Object @{Name="Name";Expression={"Total:"}}, @{Name="Duration";Expression={$stopwatch.Elapsed}}
  $list | Format-Table -Auto | Out-String -Stream | ? {$_}  # using "Out-String -Stream" to filter out the blank line that Format-Table prepends
}

#-- Public Module Functions
function Exec
{
<#
.SYNOPSIS
Helper function for executing command-line programs.

.DESCRIPTION
This is a helper function that runs a scriptblock and checks the PS variable $lastexitcode to see if an error occcured.
If an error is detected then an exception is thrown.  This function allows you to run command-line programs without
having to explicitly check fthe $lastexitcode variable.

.PARAMETER cmd
The scriptblock to execute.  This scriptblock will typically contain the command-line invocation.
Required

.PARAMETER errorMessage
The error message used for the exception that is thrown.
Optional

.EXAMPLE
exec { svn info $repository_trunk } "Error executing SVN. Please verify SVN command-line client is installed"

This example calls the svn command-line client.

.LINK
Assert
Invoke-psake
Task
Properties
Include
FormatTaskName
TaskSetup
TaskTearDown
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]

  param(
      [Parameter(Position=0,Mandatory=1)][scriptblock]$cmd,
    [Parameter(Position=1,Mandatory=0)][string]$errorMessage = "Error executing command: " + $cmd
  )
     & $cmd
     if ($lastexitcode -ne 0)
     {
          throw $errorMessage
     }
}

function Assert
{
<#
.SYNOPSIS
Helper function for "Design by Contract" assertion checking.

.DESCRIPTION
This is a helper function that makes the code less noisy by eliminating many of the "if" statements
that are normally required to verify assumptions in the code.

.PARAMETER conditionToCheck
The boolean condition to evaluate
Required

.PARAMETER failureMessage
The error message used for the exception if the conditionToCheck parameter is false
Required

.EXAMPLE
Assert $false "This always throws an exception"

This example always throws an exception

.EXAMPLE
Assert ( ($i % 2) -eq 0 ) "%i is not an even number"

This exmaple may throw an exception if $i is not an even number

.LINK
Invoke-psake
Task
Properties
Include
FormatTaskName
TaskSetup
TaskTearDown

.NOTES
It might be necessary to wrap the condition with paranthesis to force PS to evaluate the condition
so that a boolean value is calculated and passed into the 'conditionToCheck' parameter.

Example:
    Assert 1 -eq 2 "1 doesn't equal 2"

PS will pass 1 into the condtionToCheck variable and PS will look for a parameter called "eq" and
throw an exception with the following message "A parameter cannot be found that matches parameter name 'eq'"

The solution is to wrap the condition in () so that PS will evaluate it first.

    Assert (1 -eq 2) "1 doesn't equal 2"
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]

  param(
    [Parameter(Position=0,Mandatory=1)]$conditionToCheck,
    [Parameter(Position=1,Mandatory=1)]$failureMessage
  )
  if (!$conditionToCheck) { throw $failureMessage }
}

function Task
{
<#
.SYNOPSIS
Defines a build task to be executed by psake

.DESCRIPTION
This function creates a 'task' object that will be used by the psake engine to execute a build task.
Note: There must be at least one task called 'default' in the build script

.PARAMETER Name
The name of the task
Required

.PARAMETER Action
A scriptblock containing the statements to execute
Optional

.PARAMETER PreAction
A scriptblock to be executed before the 'Action' scriptblock.
Note: This parameter is ignored if the 'Action' scriptblock is not defined.
Optional

.PARAMETER PostAction
A scriptblock to be executed after the 'Action' scriptblock.
Note: This parameter is ignored if the 'Action' scriptblock is not defined.
Optional

.PARAMETER Precondition
A scriptblock that is executed to determine if the task is executed or skipped.
This scriptblock should return $true or $false
Optional

.PARAMETER Postcondition
A scriptblock that is executed to determine if the task completed its job correctly.
An exception is thrown if the scriptblock returns $false.
Optional

.PARAMETER ContinueOnError
If this switch parameter is set then the task will not cause the build to fail when an exception is thrown

.PARAMETER Depends
An array of tasks that this task depends on.  They will be executed before the current task is executed.

.PARAMETER Description
A description of the task.

.EXAMPLE
A sample build script is shown below:

task default -depends Test

task Test -depends Compile, Clean {
  "This is a test"
}

task Compile -depends Clean {
  "Compile"
}

task Clean {
  "Clean"
}

The 'default' task is required and should not contain an 'Action' parameter.
It uses the 'depends' parameter to specify that 'Test' is a dependency

The 'Test' task uses the 'depends' parameter to specify that 'Compile' and 'Clean' are dependencies
The 'Compile' task depends on the 'Clean' task.

Note:
The 'Action' parameter is defaulted to the script block following the 'Clean' task.

The equivalent 'Test' task is shown below:

task Test -depends Compile, Clean -Action {
  $testMessage
}

The output for the above sample build script is shown below:
Executing task, Clean...
Clean
Executing task, Compile...
Compile
Executing task, Test...
This is a test

Build Succeeded!

----------------------------------------------------------------------
Build Time Report
----------------------------------------------------------------------
Name    Duration
----    --------
Clean   00:00:00.0065614
Compile 00:00:00.0133268
Test    00:00:00.0225964
Total:  00:00:00.0782496

.LINK
Invoke-psake
Properties
Include
FormatTaskName
TaskSetup
TaskTearDown
Assert
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]
  param(
    [Parameter(Position=0,Mandatory=1)]
    [string]$name = $null,
    [Parameter(Position=1,Mandatory=0)]
    [scriptblock]$action = $null,
    [Parameter(Position=2,Mandatory=0)]
    [scriptblock]$preaction = $null,
    [Parameter(Position=3,Mandatory=0)]
    [scriptblock]$postaction = $null,
    [Parameter(Position=4,Mandatory=0)]
    [scriptblock]$precondition = $null,
    [Parameter(Position=5,Mandatory=0)]
    [scriptblock]$postcondition = $null,
    [Parameter(Position=6,Mandatory=0)]
    [switch]$continueOnError = $false,
    [Parameter(Position=7,Mandatory=0)]
    [string[]]$depends = @(),
    [Parameter(Position=8,Mandatory=0)]
    [string]$description = $null
    )

  if ($name.ToLower() -eq 'default')
  {
    Assert ($action -eq $null) "Error: 'default' task cannot specify an action"
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
    Duration = 0
  }

  $taskKey = $name.ToLower()

  Assert (!$script:context.Peek().tasks.ContainsKey($taskKey)) "Error: Task, $name, has already been defined."

  $script:context.Peek().tasks.$taskKey = $newTask
}

function Properties
{
<#
.SYNOPSIS
Define a scriptblock that contains assignments to variables that will be available to all tasks in the build script

.DESCRIPTION
A build script may declare a "Properies" function which allows you to define
variables that will be available to all the "Task" functions in the build script.

.PARAMETER properties
The script block containing all the variable assignment statements
Required

.EXAMPLE
A sample build script is shown below:

Properties {
  $build_dir = "c:\build"
  $connection_string = "datasource=localhost;initial catalog=northwind;integrated security=sspi"
}

Task default -depends Test

Task Test -depends Compile, Clean {
}

Task Compile -depends Clean {
}

Task Clean {
}

.LINK
Invoke-psake
Task
Include
FormatTaskName
TaskSetup
TaskTearDown
Assert

.NOTES
You can have more than 1 "Properties" function defined in the script
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]
  param(
  [Parameter(Position=0,Mandatory=1)]
  [scriptblock]$properties
  )
  $script:context.Peek().properties += $properties
}

function Include
{
<#
.SYNOPSIS
Include the functions or code of another powershell script file into the current build script's scope

.DESCRIPTION
A build script may declare an "includes" function which allows you to define
a file containing powershell code to be included and added to the scope of
the currently running build script.

.PARAMETER fileNamePathToInclude
A string containing the path and name of the powershell file to include
Required

.EXAMPLE
A sample build script is shown below:

Include ".\build_utils.ps1"

Task default -depends Test

Task Test -depends Compile, Clean {
}

Task Compile -depends Clean {
}

Task Clean {
}


.LINK
Invoke-psake
Task
Properties
FormatTaskName
TaskSetup
TaskTearDown
Assert

.NOTES
You can have more than 1 "Include" function defined in the script
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]
  param(
  [Parameter(Position=0,Mandatory=1)]
  [string]$fileNamePathToInclude
  )
  Assert (test-path $fileNamePathToInclude) "Error: Unable to include $fileNamePathToInclude. File not found."
  $script:context.Peek().includes.Enqueue((Resolve-Path $fileNamePathToInclude));
}

function FormatTaskName
{
<#
.SYNOPSIS
Allows you to define a format mask that will be used when psake displays
the task name

.DESCRIPTION
Allows you to define a format mask that will be used when psake displays
the task name.  The default is "Executing task, {0}..."

.PARAMETER format
A string containing the format mask to use, it should contain a placeholder ({0})
that will be used to substitute the task name.
Required

.EXAMPLE
A sample build script is shown below:

FormatTaskName "[Task: {0}]"

Task default -depends Test

Task Test -depends Compile, Clean {
}

Task Compile -depends Clean {
}

Task Clean {
}

You should get the following output:
------------------------------------

[Task: Clean]
[Task: Compile]
[Task: Test]

Build Succeeded

----------------------------------------------------------------------
Build Time Report
----------------------------------------------------------------------
Name    Duration
----    --------
Clean   00:00:00.0043477
Compile 00:00:00.0102130
Test    00:00:00.0182858
Total:  00:00:00.0698071

.LINK
Invoke-psake
Include
Task
Properties
TaskSetup
TaskTearDown
Assert
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]
  param(
  [Parameter(Position=0,Mandatory=1)]
  [string]$format
  )
  $script:context.Peek().formatTaskNameString = $format
}

function TaskSetup
{
<#
.SYNOPSIS
Adds a scriptblock that will be executed before each task

.DESCRIPTION
This function will accept a scriptblock that will be executed before each
task in the build script.

.PARAMETER include
A scriptblock to execute
Required

.EXAMPLE
A sample build script is shown below:

Task default -depends Test

Task Test -depends Compile, Clean {
}

Task Compile -depends Clean {
}

Task Clean {
}

TaskSetup {
  "Running 'TaskSetup' for task $script:context.Peek().currentTaskName"
}

You should get the following output:
------------------------------------

Running 'TaskSetup' for task Clean
Executing task, Clean...
Running 'TaskSetup' for task Compile
Executing task, Compile...
Running 'TaskSetup' for task Test
Executing task, Test...

Build Succeeded

----------------------------------------------------------------------
Build Time Report
----------------------------------------------------------------------
Name    Duration
----    --------
Clean   00:00:00.0054018
Compile 00:00:00.0123085
Test    00:00:00.0236915
Total:  00:00:00.0739437

.LINK
Invoke-psake
Include
Task
Properties
FormatTaskName
TaskTearDown
Assert
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]
  param(
  [Parameter(Position=0,Mandatory=1)]
  [scriptblock]$setup
  )
  $script:context.Peek().taskSetupScriptBlock = $setup
}

function TaskTearDown
{
<#
.SYNOPSIS
Adds a scriptblock that will be executed after each task

.DESCRIPTION
This function will accept a scriptblock that will be executed after each
task in the build script.

.PARAMETER include
A scriptblock to execute
Required

.EXAMPLE
A sample build script is shown below:

Task default -depends Test

Task Test -depends Compile, Clean {
}

Task Compile -depends Clean {
}

Task Clean {
}

TaskTearDown {
  "Running 'TaskTearDown' for task $script:context.Peek().currentTaskName"
}

You should get the following output:
------------------------------------

Executing task, Clean...
Running 'TaskTearDown' for task Clean
Executing task, Compile...
Running 'TaskTearDown' for task Compile
Executing task, Test...
Running 'TaskTearDown' for task Test

Build Succeeded

----------------------------------------------------------------------
Build Time Report
----------------------------------------------------------------------
Name    Duration
----    --------
Clean   00:00:00.0064555
Compile 00:00:00.0218902
Test    00:00:00.0309151
Total:  00:00:00.0858301

.LINK
Invoke-psake
Include
Task
Properties
FormatTaskName
TaskSetup
Assert
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]
  param(
  [Parameter(Position=0,Mandatory=1)]
  [scriptblock]$teardown)
  $script:context.Peek().taskTearDownScriptBlock = $teardown
}

function Invoke-psake
{
<#
.SYNOPSIS
Runs a psake build script.

.DESCRIPTION
This function runs a psake build script

.PARAMETER BuildFile
The psake build script to execute (default: default.ps1).

.PARAMETER TaskList
A comma-separated list of task names to execute

.PARAMETER Framework
The version of the .NET framework you want to build. You can append x86 or x64 to force a specific framework. If not specified, x86 or x64 will be detected based on the bitness of the PowerShell process.
Possible values: '1.0', '1.1', '2.0', '2.0x86', '2.0x64', '3.0', '3.0x86', '3.0x64', '3.5', '3.5x86', '3.5x64', '4.0', '4.0x86', '4.0x64'
Default = '3.5'

.PARAMETER Docs
Prints a list of tasks and their descriptions

.PARAMETER Parameters
A hashtable containing parameters to be passed into the current build script.  These parameters will be processed before the 'Properties' function of the script is processed.  This means you can access parameters from within the 'Properties' function!

.PARAMETER Properties
A hashtable containing properties to be passed into the current build script.  These properties will override matching properties that are found in the 'Properties' function of the script.

.EXAMPLE
Invoke-psake

Runs the 'default' task in the 'default.ps1' build script in the current directory

.EXAMPLE
Invoke-psake '.\build.ps1'

Runs the 'default' task in the '.build.ps1' build script

.EXAMPLE
Invoke-psake '.\build.ps1' Tests,Package

Runs the 'Tests' and 'Package' tasks in the '.build.ps1' build script

.EXAMPLE
Invoke-psake Tests

If you have your Tasks in the .\default.ps1. This example will run the 'Tests' tasks in the 'default.ps1' build script.

.EXAMPLE
Invoke-psake 'Tests, Package'

If you have your Tasks in the .\default.ps1. This example will run the 'Tests' and 'Package' tasks in the 'default.ps1' build script.
NOTE: the quotes around the list of tasks to execute.

.EXAMPLE
Invoke-psake '.\build.ps1' -docs

Prints a report of all the tasks and their descriptions and exits

.EXAMPLE
Invoke-psake .\parameters.ps1 -parameters @{"p1"="v1";"p2"="v2"}

Runs the build script called 'parameters.ps1' and passes in parameters 'p1' and 'p2' with values 'v1' and 'v2'

.EXAMPLE
Invoke-psake .\properties.ps1 -properties @{"x"="1";"y"="2"}

Runs the build script called 'properties.ps1' and passes in parameters 'x' and 'y' with values '1' and '2'

.OUTPUTS
    If there is an exception and '$psake.use_exit_on_error' -eq $true
  then runs exit(1) to set the DOS lastexitcode variable
  otherwise set the '$psake.build_success variable' to $true or $false depending
  on whether an exception was thrown

.NOTES
When the psake module is loaded a variabled called $psake is created it is a hashtable
containing some variables that can be used to configure psake:

$psake.use_exit_on_error = $false   # determines if psake uses the "exit()" function when an exception occurs
$psake.log_error = $false           # determines if the exception details are written to a file
$psake.build_success = $false       # indicates that the current build was successful
$psake.version = "4.00"             # contains the current version of psake
$psake.build_script_file = $null    # contains a System.IO.FileInfo for the current build file
$psake.framework_version = ""       # contains the framework version # for the current build

$psake.use_exit_on_error and $psake.log_error are boolean variables that can be set before you call Invoke-Psake.

You should see the following when you display the contents of the $psake variable right after importing psake

PS projects:\psake> Import-Module .\psake.psm1
PS projects:\psake> $psake

Name                           Value
----                           -----
version                        4.00
build_script_file
use_exit_on_error              False
build_success                  False
log_error                      False
framework_version

After a build is executed the following $psake values are updated (build_script_file, build_success, and framework_version)

PS projects:\psake> Invoke-psake .\examples\default.ps1
Executing task: Clean
Executed Clean!
Executing task: Compile
Executed Compile!
Executing task: Test
Executed Test!

Build Succeeded!

----------------------------------------------------------------------
Build Time Report
----------------------------------------------------------------------
Name    Duration
----    --------
Clean   00:00:00.0798486
Compile 00:00:00.0869948
Test    00:00:00.0958225
Total:  00:00:00.2712414

PS projects:\psake> $psake

Name                           Value
----                           -----
version                        4.00
build_script_file              C:\Users\Jorge\Documents\Projects\psake\examples\default.ps1
use_exit_on_error              False
build_success                  True
log_error                      False
framework_version              3.5

.LINK
Task
Include
Properties
FormatTaskName
TaskSetup
TaskTearDown
Assert
#>
[CmdletBinding(
    SupportsShouldProcess=$False,
    SupportsTransactions=$False,
    ConfirmImpact="None",
    DefaultParameterSetName="")]

  param(
    [Parameter(Position=0,Mandatory=0)]
    [string]$buildFile = $script:psake.default_build_file_name,
    [Parameter(Position=1,Mandatory=0)]
    [string[]]$taskList = @(),
    [Parameter(Position=2,Mandatory=0)]
    [string]$framework = '3.5',
    [Parameter(Position=3,Mandatory=0)]
    [switch]$docs = $false,
    [Parameter(Position=4,Mandatory=0)]
    [System.Collections.Hashtable]$parameters = @{},
    [Parameter(Position=5, Mandatory=0)]
    [System.Collections.Hashtable]$properties = @{}
  )

  Begin
  {
    $script:psake.build_success = $false
    $script:psake.framework_version = $framework

    if ($script:context -eq $null)
    {
      $script:context = New-Object System.Collections.Stack
    }

    $script:context.push(@{
                           "formatTaskNameString" = "Executing task: {0}";
                           "taskSetupScriptBlock" = $null;
                           "taskTearDownScriptBlock" = $null;
                           "executedTasks" = New-Object System.Collections.Stack;
                           "callStack" = New-Object System.Collections.Stack;
                           "originalEnvPath" = $env:path;
                           "originalDirectory" = Get-Location;
                           "originalErrorActionPreference" = $global:ErrorActionPreference;
                           "tasks" = @{};
                           "properties" = @();
                           "includes" = New-Object System.Collections.Queue;
    })
  }

  Process
  {
    try
    {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    <#
      If the default.ps1 file exists and the given "buildfile" isn't found assume that the given 
      $buildFile is actually the target Tasks to execute in the default.ps1 script.
    #>
    if((Test-Path $script:psake.default_build_file_name ) -and !(test-path $buildFile)) {
      $list = New-Object System.Collections.ArrayList
      foreach($t in $buildFile.Split(',')) {
        $t1 = $t.Trim()
        if($t1 -ne $null -or $t1 -ne "") {
          $list.Add($t1)
        }
      }
      $taskList = $list.ToArray()
      $buildFile = $script:psake.default_build_file_name
    }

    # Execute the build file to set up the tasks and defaults
    Assert (test-path $buildFile) "Error: Could not find the build file, $buildFile."

    $script:psake.build_script_file = dir $buildFile
    set-location $script:psake.build_script_file.Directory
    . $script:psake.build_script_file.FullName

    if ($docs)
    {
    Write-Documentation
    Cleanup-Environment
    return
    }

    Configure-BuildEnvironment

    # N.B. The initial dot (.) indicates that variables initialized/modified
    #      in the propertyBlock are available in the parent scope.
    while ($script:context.Peek().includes.Count -gt 0)
    {
      $includeBlock = $script:context.Peek().includes.Dequeue()
      . $includeBlock
    }

    foreach($key in $parameters.keys)
    {
      if (test-path "variable:\$key")
      {
        set-item -path "variable:\$key" -value $parameters.$key | out-null
      }
      else
      {
        new-item -path "variable:\$key" -value $parameters.$key | out-null
      }
    }

    foreach($propertyBlock in $script:context.Peek().properties)
    {
      . $propertyBlock
    }

    foreach($key in $properties.keys)
    {
      if (test-path "variable:\$key")
      {
        set-item -path "variable:\$key" -value $properties.$key | out-null
      }
    }

    # Execute the list of tasks or the default task
    if($taskList.Length -ne 0)
    {
      foreach($task in $taskList)
      {
        ExecuteTask $task
      }
    }
    elseif ($script:context.Peek().tasks.default -ne $null)
    {
      ExecuteTask default
    }
    else
    {
      throw 'Error: default task required'
    }

    $stopwatch.Stop()

    "`nBuild Succeeded!`n"

    Write-TaskTimeSummary

    $script:psake.build_success = $true
    }
    catch
    {
    #Append detailed exception and script variables to error log file
    if ($script:psake.log_error)
    {
      $errorLogFile = "psake-error-log-{0}.log" -f ([DateTime]::Now.ToString("yyyyMMdd"))
      "-" * 70 >> $errorLogFile
      "{0}: An Error Occurred. See Error Details Below: " -f [DateTime]::Now >>$errorLogFile
      "-" * 70 >> $errorLogFile
      Resolve-Error $_ >> $errorLogFile
      "-" * 70 >> $errorLogFile
      "Script Variables" >> $errorLogFile
      "-" * 70 >> $errorLogFile
      Get-Variable -scope script >> $errorLogFile
    }

    $buildFileName = Split-Path $buildFile -leaf
    if (test-path $buildFile) { $buildFileName = $script:psake.build_script_file.Name }
    Write-Host -foregroundcolor Red ($buildFileName + ":" + $_)

    if ($script:psake.use_exit_on_error)
    {
      exit(1)
    }
    else
    {
      $script:psake.build_success = $false
    }
    }
  } #Process

  End
  {
  # Clear out any global variables
  Cleanup-Environment
  [void]$script:context.Pop()
  }
}

Export-ModuleMember -Function "Invoke-psake","Task","Properties","Include","FormatTaskName","TaskSetup","TaskTearDown","Assert","Exec"