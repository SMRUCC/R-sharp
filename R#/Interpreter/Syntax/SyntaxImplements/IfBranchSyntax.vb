﻿#Region "Microsoft.VisualBasic::6d948e6f5c958b208531976b1cddc264, R#\Interpreter\Syntax\SyntaxImplements\IfBranchSyntax.vb"

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

    '     Module IfBranchSyntax
    ' 
    '         Function: ElseClosure, ElseIfClosure, IfClosure, (+2 Overloads) IIfExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser.SyntaxImplements

    Module IfBranchSyntax

        <Extension>
        Public Function IIfExpression(tokens As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim [select] = tokens.Split(Function(t) t.Length = 1 AndAlso t(Scan0) = (TokenType.keyword, "else")).ToArray
            Dim [else] = Expression.CreateExpression([select](1).IteratesALL, opts)
            Dim sourceMap = opts.GetStackTrace(tokens(Scan0)(Scan0), "iif")

            tokens = [select](Scan0).IteratesALL.SplitByTopLevelDelimiter(TokenType.close, False, ")", Nothing)

            Dim test = Expression.CreateExpression(tokens(Scan0).Skip(2), opts)
            Dim [if] = Expression.CreateExpression(tokens.Skip(2).IteratesALL, opts)

            If [else].isException Then
                Return [else]
            ElseIf test.isException Then
                Return test
            ElseIf [if].isException Then
                Return [if]
            End If

            Return New IIfExpression(
                iftest:=test.expression,
                trueResult:=[if].expression,
                falseResult:=[else].expression,
                stackFrame:=sourceMap
            )
        End Function

        Public Function IIfExpression(test As Token(), ifelse As List(Of Token()), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim ifTest = Expression.CreateExpression(test, opts)
            Dim trueResult = Expression.CreateExpression(ifelse(Scan0), opts)
            Dim falseResult = Expression.CreateExpression(ifelse(2), opts)
            Dim sourceMap = opts.GetStackTrace(test(Scan0), "iif")

            If ifTest.isException Then
                Return ifTest
            ElseIf trueResult.isException Then
                Return trueResult
            ElseIf falseResult.isException Then
                Return falseResult
            Else
                Return New IIfExpression(
                    iftest:=ifTest.expression,
                    trueResult:=trueResult.expression,
                    falseResult:=falseResult.expression,
                    stackFrame:=sourceMap
                )
            End If
        End Function

        Public Function IfClosure(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim blocks = tokens.SplitByTopLevelDelimiter(TokenType.close)
            Dim ifTest = Expression.CreateExpression(blocks(Scan0).Skip(1), opts)

            If ifTest.isException Then
                Return ifTest
            End If

            Dim closureInternal As SyntaxResult = blocks(2) _
                .Skip(1) _
                .DoCall(Function(code)
                            Return SyntaxImplements.ClosureExpression(code, opts)
                        End Function)

            If closureInternal.isException Then
                Return closureInternal
            End If

            Dim stackframe As StackFrame = opts.GetStackTrace(blocks(Scan0)(Scan0), "if_closure")

            If blocks = 6 Then
                ' if () {} else {}
                Dim elseBlock = blocks(4)
                Dim elseClosure As SyntaxResult = SyntaxImplements.ClosureExpression(elseBlock.Skip(2), opts)

                If elseClosure.isException Then
                    Return elseClosure
                End If

                Return New IIfExpression(
                    iftest:=ifTest.expression,
                    trueResult:=closureInternal.expression,
                    falseResult:=elseClosure.expression,
                    stackFrame:=stackframe
                )
            Else
                Dim [if] As New IfBranch(
                    ifTest:=ifTest.expression,
                    trueClosure:=DirectCast(closureInternal.expression, ClosureExpression),
                    stackframe:=stackframe
                )

                Return New SyntaxResult([if])
            End If
        End Function

        Public Function ElseIfClosure(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim [if] As SyntaxResult = IfClosure(tokens, opts)

            If [if].isException Then
                Return [if]
            Else
                With DirectCast([if].expression, IfBranch)
                    Return New ElseIfBranch(.ifTest, .trueClosure.body, .stackFrame)
                End With
            End If
        End Function

        Public Function ElseClosure(code As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim syntaxResult As SyntaxResult = code _
                .Skip(1) _
                .Take(code.Length - 2) _
                .DoCall(Function(tokens)
                            Return SyntaxImplements.ClosureExpression(tokens, opts)
                        End Function)
            Dim stackframe As StackFrame = opts.GetStackTrace(code(Scan0), name:="else_false")

            If syntaxResult.isException Then
                Return syntaxResult
            Else
                Return New ElseBranch(syntaxResult.expression, stackframe)
            End If
        End Function
    End Module
End Namespace
