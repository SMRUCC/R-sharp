﻿#Region "Microsoft.VisualBasic::50e056e58f14743034ba1024a35ba627, studio\RData\ParserXDR.vb"

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

    ' Class ParserXDR
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: parse_double, parse_int, parse_string
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.Xdr

''' <summary>
''' Parser used when the integers and doubles are in XDR format.
''' </summary>
Public Class ParserXDR : Inherits Reader

    ReadOnly data As BinaryDataReader
    ReadOnly xdr_parser As Unpacker

    Sub New(data As BinaryDataReader, Optional position As Integer = 0, Optional expand_altrep As Boolean = True)
        Call MyBase.New(expand_altrep)

        Me.data = data
        Me.data.Position = position
        Me.xdr_parser = New Unpacker(data)
    End Sub

    Public Overrides Function parse_int() As Integer
        Dim result = xdr_parser.unpack_int()
        Return result
    End Function

    Public Overrides Function parse_double() As Double
        Dim result = xdr_parser.unpack_double()
        Return result
    End Function

    Public Overrides Function parse_string(length As Integer) As Byte()
        Dim result As Byte() = data.ReadBytes(length)
        Return result
    End Function
End Class
