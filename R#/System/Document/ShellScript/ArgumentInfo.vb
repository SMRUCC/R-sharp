﻿#Region "Microsoft.VisualBasic::6e6eca7503e6f4e88ca6ad34923b4613, R#\System\Document\ShellScript\ArgumentInfo.vb"

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

    '     Class ArgumentInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [GetTypeCode]
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language.Default
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development.CommandLine

    Friend Class ArgumentInfo

        Friend attrs As New Dictionary(Of String, String())

        ''' <summary>
        ''' argument value type in the commandline input.
        ''' </summary>
        ReadOnly type As TypeCodes?

        Default Public ReadOnly Property Item(name As String) As String
            Get
                Return attrs.TryGetValue("info").JoinBy(";" & vbCrLf)
            End Get
        End Property

        Sub New(type As TypeCodes)
            If (Not type = TypeCodes.generic) AndAlso (Not type = TypeCodes.NA) Then
                Me.type = type
            Else
                Me.type = Nothing
            End If
        End Sub

        Public Function [GetTypeCode]() As String
            Static stringType As [Default](Of String) = "string"

            If attrs.ContainsKey("type") Then
                Return attrs("type").FirstOrDefault Or stringType
            End If

            If type Is Nothing Then
                Return "string"
            Else
                Return CType(type, TypeCodes).ToString
            End If
        End Function

    End Class
End Namespace
