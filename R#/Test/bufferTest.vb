﻿#Region "Microsoft.VisualBasic::cd00d22db1292a45cd80d79ecf4f22a4, R#\Test\bufferTest.vb"

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

    ' Module bufferTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Serialize

Module bufferTest

    Dim R As New RInterpreter With {.debug = False}

    Sub Main()
        Dim vec As New vector({1, 2, 3, 4, 5}, RType.GetRSharpType(GetType(Integer)))

        vec.setNames({"a", "b", "c", "d", "e"}, R.globalEnvir)
        vec.unit = New unit With {.name = "abc"}

        Dim serial As vectorBuffer = vectorBuffer.CreateBuffer(vec, R.globalEnvir)
        Dim bytes = serial.Serialize

        Dim temp = "./test_vector.dat"

        Call bytes.FlushStream(temp)

        Dim vec2 = vectorBuffer.CreateBuffer(temp.Open).GetVector


        Dim message As Message = R.Evaluate("stop(['123456666','babala']);")

        Dim msgSerial As messageBuffer = New messageBuffer(message)
        bytes = msgSerial.Serialize

        temp = "./test_message.dat"
        Call bytes.FlushStream(temp)

        Dim msgNew = messageBuffer.CreateBuffer(temp.Open)


        Pause()
    End Sub
End Module

