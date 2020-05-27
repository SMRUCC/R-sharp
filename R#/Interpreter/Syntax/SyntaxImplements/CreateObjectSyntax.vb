﻿#Region "Microsoft.VisualBasic::515684b9d472d168e46616608b2b1c98, R#\Interpreter\Syntax\SyntaxImplements\CreateObjectSyntax.vb"

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

    '     Module CreateObjectSyntax
    ' 
    '         Function: CreateNewObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module CreateObjectSyntax

        Public Function CreateNewObject(keyword As Token, tokens As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim type As String = tokens(Scan0).text
            Dim stackFrame As StackFrame = opts.GetStackTrace(keyword, $"[{type}].cor")
            Dim args = tokens.Skip(2).Take(tokens.Length - 3).ToArray
            Dim parameters As New List(Of Expression)

            For Each a As SyntaxResult In args.getInvokeParameters(opts)
                If a.isException Then
                    Return a
                Else
                    parameters.Add(a.expression)
                End If
            Next

            Return New CreateObject(type, parameters.ToArray, stackFrame)
        End Function
    End Module
End Namespace