﻿#Region "Microsoft.VisualBasic::ea99acbf41287b7fbdfecac205899e6c, R#\Runtime\Internal\objects\utils\TableFormatter.vb"

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

'     Class TableFormatter
' 
'         Function: GetTable
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Object.Utils

    Public Class TableFormatter

        Private Shared Function GetTypeArray(df As dataframe, colNames As String()) As String()
            Return colNames _
                .Select(Function(name)
                            Dim arrayType As Type = df(name).GetType
                            Dim type As RType = RType.GetRSharpType(arrayType)

                            Return $"<{type}>"
                        End Function) _
                .ToArray
        End Function

        Private Shared Function GetFormatters(df As dataframe, colNames As String(), env As GlobalEnvironment, isPrintContent As Boolean) As IStringBuilder()
            Dim elementTypes As Type() = colNames _
                .Select(Function(key)
                            Return df.columns(key).GetType.GetElementType
                        End Function) _
                .ToArray
            Dim formatters As IStringBuilder() = elementTypes _
                .Select(Function(type)
                            Return printer.ToString(type, env, isPrintContent)
                        End Function) _
                .ToArray

            Return formatters
        End Function

        ''' <summary>
        ''' Each element in a return result array is a row in table matrix
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetTable(df As dataframe, env As GlobalEnvironment,
                                        Optional printContent As Boolean = True,
                                        Optional showRowNames As Boolean = True) As String()()

            Dim table As String()() = New String(df.nrows + If(printContent, 1, 0))() {}
            Dim rIndex As Integer
            Dim colNames$() = df.columns.Keys.ToArray
            Dim col As Array
            Dim row As String() = {""}.Join(colNames)
            Dim rownames As String() = df.getRowNames()
            Dim typeRow As String() = GetTypeArray(df, colNames)
            Dim formatters As IStringBuilder() = GetFormatters(df, colNames, env, printContent)

            If showRowNames Then
                table(Scan0) = row.ToArray

                If printContent Then
                    table(1) = {"<mode>"}.JoinIterates(typeRow).ToArray
                End If
            Else
                table(Scan0) = row.Skip(1).ToArray

                If printContent Then
                    table(1) = typeRow
                End If
            End If

            For i As Integer = 2 To table.Length - 1
                rIndex = i - 2
                row(Scan0) = rownames(rIndex)

                For j As Integer = 0 To df.columns.Count - 1
                    col = df.columns(colNames(j))

                    If col.Length = 1 Then
                        row(j + 1) = formatters(j)(col.GetValue(Scan0))
                    Else
                        row(j + 1) = formatters(j)(col.GetValue(rIndex))
                    End If
                Next

                If showRowNames Then
                    table(i) = row.ToArray
                Else
                    table(i) = row.Skip(1).ToArray
                End If
            Next

            Return table
        End Function
    End Class
End Namespace
