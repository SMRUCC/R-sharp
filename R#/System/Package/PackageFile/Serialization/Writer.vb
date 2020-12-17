﻿Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package.File.Expressions

Namespace System.Package.File

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' expression: [ExpressionTypes, i32][dataSize, i32][TypeCodes, byte][expressionData, bytes]
    '''              4                     4              1                ...
    ''' </remarks>
    Public Class Writer : Implements IDisposable

        Dim buffer As BinaryWriter
        Dim disposedValue As Boolean

        Public Const Magic As String = "SMRUCC/R#"

        Public ReadOnly Property RSymbol As RSymbol
        Public ReadOnly Property RLiteral As RLiteral
        Public ReadOnly Property RBinary As RBinary
        Public ReadOnly Property RCallFunction As RCallFunction
        Public ReadOnly Property RImports As RRequire
        Public ReadOnly Property RUnary As RUnary
        Public ReadOnly Property RVector As RVector

        Sub New(buffer As Stream)
            Me.buffer = New BinaryWriter(buffer)

            Me.RSymbol = New RSymbol(Me)
            Me.RLiteral = New RLiteral(Me)
            Me.RBinary = New RBinary(Me)
            Me.RCallFunction = New RCallFunction(Me)
            Me.RImports = New RRequire(Me)
            Me.RUnary = New RUnary(Me)
            Me.RVector = New RVector(Me)
        End Sub

        Public Function GetBuffer(x As Expression) As Byte()
            Select Case x.GetType
                Case GetType(RSymbol) : Return RSymbol.GetBuffer(x)
                Case GetType(RLiteral) : Return RLiteral.GetBuffer(x)
                Case GetType(RBinary) : Return RBinary.GetBuffer(x)
                Case GetType(RCallFunction) : Return RCallFunction.GetBuffer(x)
                Case GetType(RRequire) : Return RImports.GetBuffer(x)
                Case GetType(RUnary) : Return RUnary.GetBuffer(x)
                Case GetType(RVector) : Return RVector.GetBuffer(x)
                Case Else
                    Throw New NotImplementedException(x.GetType.FullName)
            End Select
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>
        ''' 函数返回表达式的长度
        ''' </returns>
        Public Function Write(x As Expression) As Integer
            Dim buffer As Byte() = getBuffer(x)
            Call Me.buffer.Write(buffer)
            Return buffer.Length
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call buffer.Flush()
                    Call buffer.Close()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace