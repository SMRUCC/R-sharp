Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService
Imports Microsoft.VisualBasic.ApplicationServices

' Microsoft VisualBasic CommandLine Code AutoGenerator
' assembly: ..\App\Rscript.exe

' 
'  // 
'  // R# scripting host
'  // 
'  // VERSION:   1.99.7661.24729
'  // ASSEMBLY:  Rscript, Version=1.99.7661.24729, Culture=neutral, PublicKeyToken=null
'  // COPYRIGHT: Copyright (c) SMRUCC genomics  2020
'  // GUID:      16d477b1-e7fb-41eb-9b61-7ea75c5d2939
'  // BUILT:     12/22/2020 1:44:18 PM
'  // 
' 
' 
'  < Rscript.CLI >
' 
' 
' SYNOPSIS
' Rscript command [/argument argument-value...] [/@set environment-variable=value...]
' 
' All of the command that available in this program has been list below:
' 
'  --build:     build R# package
'  --check:     Verify a packed R# package is damaged or not?
'  --slave:     Create a R# cluster node for run background or parallel task. This IPC command will
'               run a R# script file that specified by the ``/exec`` argument, and then post back the
'               result data json to the specific master listener.
' 
' 
' ----------------------------------------------------------------------------------------------------
' 
'    1. You can using "Rscript ??<commandName>" for getting more details command help.
'    2. Using command "Rscript /CLI.dev [---echo]" for CLI pipeline development.
'    3. Using command "Rscript /i" for enter interactive console mode.

Namespace RscriptCommandLine


    ''' <summary>
    ''' Rscript.CLI
    ''' </summary>
    '''
    Public Class Rscript : Inherits InteropService

        Public Const App$ = "Rscript.exe"

        Sub New(App$)
            MyBase._executableAssembly = App$
        End Sub

        ''' <summary>
        ''' Create an internal CLI pipeline invoker from a given environment path. 
        ''' </summary>
        ''' <param name="directory">A directory path that contains the target application</param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromEnvironment(directory As String) As Rscript
            Return New Rscript(App:=directory & "/" & Rscript.App)
        End Function

        ''' <summary>
        ''' ```bash
        ''' --build /src &lt;folder&gt; [/save &lt;Rpackage.zip&gt;]
        ''' ```
        ''' build R# package
        ''' </summary>
        '''
        ''' <param name="src"> A folder path that contains the R source files and meta data files of the target R package, 
        '''               a folder that exists in this folder path which is named &apos;R&apos; is required!
        ''' </param>
        Public Function Compile(src As String, Optional save As String = "") As Integer
            Dim cli = GetCompileCommandLine(src:=src, save:=save)
            Dim proc As IIORedirectAbstract = RunDotNetApp(cli)
            Return proc.Run()
        End Function
        Public Function GetCompileCommandLine(src As String, Optional save As String = "") As String
            Dim CLI As New StringBuilder("--build")
            Call CLI.Append(" ")
            Call CLI.Append("/src " & """" & src & """ ")
            If Not save.StringEmpty Then
                Call CLI.Append("/save " & """" & save & """ ")
            End If
            Call CLI.Append("/@set --internal_pipeline=TRUE ")


            Return CLI.ToString()
        End Function

        ''' <summary>
        ''' ```bash
        ''' --check --target &lt;package.zip&gt;
        ''' ```
        ''' Verify a packed R# package is damaged or not?
        ''' </summary>
        '''

        Public Function Check(target As String) As Integer
            Dim cli = GetCheckCommandLine(target:=target)
            Dim proc As IIORedirectAbstract = RunDotNetApp(cli)
            Return proc.Run()
        End Function
        Public Function GetCheckCommandLine(target As String) As String
            Dim CLI As New StringBuilder("--check")
            Call CLI.Append(" ")
            Call CLI.Append("--target " & """" & target & """ ")
            Call CLI.Append("/@set --internal_pipeline=TRUE ")


            Return CLI.ToString()
        End Function

        ''' <summary>
        ''' ```bash
        ''' --slave /exec &lt;script.R&gt; /args &lt;json_base64&gt; /request-id &lt;request_id&gt; /PORT=&lt;port_number&gt; [/timeout=&lt;timeout in ms, default=1000&gt; /retry=&lt;retry_times, default=5&gt; /MASTER=&lt;ip, default=localhost&gt; /entry=&lt;function_name, default=NULL&gt;]
        ''' ```
        ''' Create a R# cluster node for run background or parallel task. This IPC command will run a R# script file that specified by the ``/exec`` argument, and then post back the result data json to the specific master listener.
        ''' </summary>
        '''
        ''' <param name="exec"> a specific R# script for run
        ''' </param>
        ''' <param name="args"> The base64 text of the input arguments for running current R# script file, this is a json encoded text of the arguments. the json object should be a collection of [key =&gt; value[]] pairs.
        ''' </param>
        ''' <param name="entry"> the entry function name, by default is running the script from the begining to ends.
        ''' </param>
        ''' <param name="request_id"> the unique id for identify current slave progress in the master node when invoke post data callback.
        ''' </param>
        ''' <param name="MASTER"> the ip address of the master node, by default this parameter value is ``localhost``.
        ''' </param>
        ''' <param name="PORT"> the port number for master node listen to this callback post data.
        ''' </param>
        ''' <param name="retry"> How many times that this cluster node should retry to send callback data if the TCP request timeout.
        ''' </param>
        Public Function slaveMode(exec As String,
                                     args As String,
                                     request_id As String,
                                     PORT As String,
                                     Optional timeout As String = "1000",
                                     Optional retry As String = "5",
                                     Optional master As String = "localhost",
                                     Optional entry As String = "NULL") As Integer
            Dim cli = GetslaveModeCommandLine(exec:=exec,
                                         args:=args,
                                         request_id:=request_id,
                                         PORT:=PORT,
                                         timeout:=timeout,
                                         retry:=retry,
                                         master:=master,
                                         entry:=entry)
            Dim proc As IIORedirectAbstract = RunDotNetApp(cli)
            Return proc.Run()
        End Function
        Public Function GetslaveModeCommandLine(exec As String,
                                     args As String,
                                     request_id As String,
                                     PORT As String,
                                     Optional timeout As String = "1000",
                                     Optional retry As String = "5",
                                     Optional master As String = "localhost",
                                     Optional entry As String = "NULL") As String
            Dim CLI As New StringBuilder("--slave")
            Call CLI.Append(" ")
            Call CLI.Append("/exec " & """" & exec & """ ")
            Call CLI.Append("/args " & """" & args & """ ")
            Call CLI.Append("/request-id " & """" & request_id & """ ")
            Call CLI.Append("/PORT " & """" & PORT & """ ")
            If Not timeout.StringEmpty Then
                Call CLI.Append("/timeout " & """" & timeout & """ ")
            End If
            If Not retry.StringEmpty Then
                Call CLI.Append("/retry " & """" & retry & """ ")
            End If
            If Not master.StringEmpty Then
                Call CLI.Append("/master " & """" & master & """ ")
            End If
            If Not entry.StringEmpty Then
                Call CLI.Append("/entry " & """" & entry & """ ")
            End If
            Call CLI.Append("/@set --internal_pipeline=TRUE ")


            Return CLI.ToString()
        End Function
    End Class
End Namespace


