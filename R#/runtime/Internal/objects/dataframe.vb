﻿#Region "Microsoft.VisualBasic::a06e4b5de63a3c36786b51d8d760dbac, R#\Runtime\Internal\objects\dataframe.vb"

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

    '     Class dataframe
    ' 
    '         Properties: columns, ncols, nrows, rownames
    ' 
    '         Function: GetByRowIndex, GetColumnVector, GetTable, projectByColumn, sliceByRow
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Internal.Object

    Public Class dataframe

        ''' <summary>
        ''' 长度为1或者长度为n
        ''' </summary>
        ''' <returns></returns>
        Public Property columns As Dictionary(Of String, Array)
        Public Property rownames As String()

        Public ReadOnly Property nrows As Integer
            Get
                Return Aggregate col In columns.Values Let len = col.Length Into Max(len)
            End Get
        End Property

        Public ReadOnly Property ncols As Integer
            Get
                Return columns.Count
            End Get
        End Property

        Public Function GetColumnVector(columnName As String) As Array
            Dim n As Integer = nrows
            Dim col As Array = columns.TryGetValue(columnName)

            If col Is Nothing Then
                Return Nothing
            ElseIf col.Length = n Then
                Return col
            Else
                Return VectorExtensions _
                    .Replicate(col.GetValue(Scan0), n) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' ``data[, selector]``
        ''' </summary>
        ''' <param name="selector"></param>
        ''' <returns></returns>
        Public Function projectByColumn(selector As Array) As dataframe
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' ``data[selector, ]``
        ''' </summary>
        ''' <param name="selector"></param>
        ''' <returns></returns>
        Public Function sliceByRow(selector As Array) As dataframe
            Throw New NotImplementedException
        End Function

        Public Function GetByRowIndex(index As Integer()) As dataframe
            Dim subsetRowNumbers As String() = index _
                .Select(Function(i, j)
                            Return rownames.ElementAtOrDefault(i, j)
                        End Function) _
                .ToArray
            Dim subsetData As Dictionary(Of String, Array) = columns _
                .ToDictionary(Function(c) c.Key,
                              Function(c)
                                  If c.Value.Length = 1 Then
                                      ' single value
                                      Return DirectCast(c.Value.getvalue(Scan0), Array)
                                  End If

                                  Dim vec = index _
                                    .Select(Function(i)
                                                Return c.Value.GetValue(i)
                                            End Function) _
                                    .ToArray

                                  Return DirectCast(vec, Array)
                              End Function)

            Return New dataframe With {
                .rownames = subsetRowNumbers,
                .columns = subsetData
            }
        End Function

        ''' <summary>
        ''' Each element in a return result array is a row in table matrix
        ''' </summary>
        ''' <returns></returns>
        Public Function GetTable(env As GlobalEnvironment) As String()()
            Dim table As String()() = New String(nrows)() {}
            Dim row As String()
            Dim rIndex As Integer
            Dim colNames$() = columns.Keys.ToArray
            Dim col As Array

            row = {""}.Join(colNames)
            table(Scan0) = row.ToArray

            If rownames.IsNullOrEmpty Then
                rownames = table _
                    .Sequence(offSet:=1) _
                    .Select(Function(r) $"[{r}, ]") _
                    .ToArray
            End If

            Dim elementTypes As Type() = colNames _
                .Select(Function(key) columns(key).GetType.GetElementType) _
                .ToArray
            Dim formatters As IStringBuilder() = elementTypes _
                .Select(Function(type) printer.ToString(type, env)) _
                .ToArray

            For i As Integer = 1 To table.Length - 1
                rIndex = i - 1
                row(Scan0) = rownames(rIndex)

                For j As Integer = 0 To columns.Count - 1
                    col = columns(colNames(j))

                    If col.Length = 1 Then
                        row(j + 1) = formatters(j)(col.GetValue(Scan0))
                    Else
                        row(j + 1) = formatters(j)(col.GetValue(rIndex))
                    End If
                Next

                table(i) = row.ToArray
            Next

            Return table
        End Function

    End Class
End Namespace