﻿#Region "Microsoft.VisualBasic::5b94e75c3eab1a26aebcc4976f50dd01, R-terminal\CLI\CLI.vb"

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

' Module CLI
' 
' 
' 
' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Package

Module CLI

    <ExportAPI("--install.packages")>
    <Description("Install new packages.")>
    <Usage("--install.packages /module <*.dll> [--verbose]")>
    Public Function Install(args As CommandLine) As Integer
        Dim module$ = args <= "/module"
        Dim verboseMode As Boolean = args("--verbose")
        Dim config As New Options(ConfigFile.localConfigs)
        Dim localdb As LocalPackageDatabase = LocalPackageDatabase.Load(config.lib)

        If [module].StringEmpty Then
            Return "Missing '/module' argument!".PrintException
        End If

        Dim packageIndex = localdb.packages.ToDictionary(Function(pkg) pkg.namespace)

        For Each pkg As Package In PackageLoader.ParsePackages([module])
            With PackageLoaderEntry.FromLoaderInfo(pkg)
                ' 新的package信息会覆盖掉旧的package信息
                packageIndex(.namespace) = .ByRef
            End With

            Call $"load: {pkg.info.Namespace}".__INFO_ECHO
        Next

        localdb.packages = packageIndex.Values.ToArray
        localdb.system = GetType(LocalPackageDatabase).Assembly.FromAssembly

        Return localdb _
            .GetXml _
            .SaveTo(config.lib) _
            .CLICode
    End Function
End Module
