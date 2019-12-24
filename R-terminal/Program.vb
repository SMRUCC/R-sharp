﻿#Region "Microsoft.VisualBasic::161255cab10f5667cba9f304e42d9c18, R-terminal\Program.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module Program
    ' 
    '     Function: Main, RunScript
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Configuration

Module Program

    <DebuggerStepThrough>
    Public Function Main() As Integer
        Return GetType(CLI).RunCLI(
            args:=App.CommandLine,
            executeFile:=AddressOf RunScript,
            executeEmpty:=AddressOf Terminal.RunTerminal
        )
    End Function

    Private Function RunScript(filepath$, args As CommandLine) As Integer
        Dim R As RInterpreter = RInterpreter.FromEnvironmentConfiguration(ConfigFile.localConfigs)

        If args("--debug") Then
            R.debug = True
        End If

        Call Console.WriteLine(args.ToString)
        Call Console.WriteLine()

        Call R.LoadLibrary("base")
        Call R.LoadLibrary("utils")
        Call R.LoadLibrary("grDevices")

        Call Console.WriteLine()

        'For Each arg As NamedValue(Of String) In args.ToArgumentVector
        '    Call R.Add(CommandLine.TrimNamePrefix(arg.Name), arg.Value, TypeCodes.generic)
        'Next

        Dim result As Object = R.Source(filepath)

        If Not result Is Nothing AndAlso result.GetType Is GetType(Message) Then
            Return DirectCast(result, Message).level
        Else
            Return 0
        End If
    End Function
End Module
