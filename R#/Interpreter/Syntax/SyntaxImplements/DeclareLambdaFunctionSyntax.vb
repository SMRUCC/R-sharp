﻿#Region "Microsoft.VisualBasic::74476886073282f082d2ca261d22fb9a, R#\Interpreter\Syntax\SyntaxImplements\DeclareLambdaFunctionSyntax.vb"

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

    '     Module DeclareLambdaFunctionSyntax
    ' 
    '         Function: DeclareLambdaFunction
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module DeclareLambdaFunctionSyntax

        ''' <summary>
        ''' 只允许拥有一个参数，并且只允许出现一行代码
        ''' </summary>
        ''' <param name="tokens"></param>
        Public Function DeclareLambdaFunction(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            With tokens.ToArray
                Dim name = .IteratesALL _
                           .Select(Function(t) t.text) _
                           .JoinBy(" ") _
                           .DoCall(Function(exp)
                                       Return "[lambda: " & exp & "]"
                                   End Function)
                Dim parameter As SyntaxResult = SyntaxImplements.DeclareNewVariable(tokens(Scan0))
                Dim closure As SyntaxResult = .Skip(2) _
                                              .IteratesALL _
                                              .DoCall(Function(code)
                                                          Return Expression.CreateExpression(code, opts)
                                                      End Function)

                If parameter.isException Then
                    Return parameter
                ElseIf closure.isException Then
                    Return closure
                Else
                    Return New SyntaxResult(New DeclareLambdaFunction(name, parameter.expression, closure.expression))
                End If
            End With
        End Function
    End Module
End Namespace