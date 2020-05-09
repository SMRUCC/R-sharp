﻿#Region "Microsoft.VisualBasic::d52c37b40afba1db089277bd399ac472, R#\Runtime\Interop\RInteropAttributes\RInteropAttribute.vb"

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

'     Class RInteropAttribute
' 
' 
' 
'     Class RInitializeAttribute
' 
' 
' 
'     Class RByRefValueAssignAttribute
' 
' 
' 
'     Class RParameterNameAliasAttribute
' 
'         Properties: [alias]
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: ToString
' 
'     Class RApiReturnAttribute
' 
'         Properties: returnTypes
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: GetActualReturnType, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class RInteropAttribute : Inherits Attribute
    End Class

    ''' <summary>
    ''' 如果使用sub new初始化的话，则在导入程序包的时候sub new里面的代码是不会被自动调用的
    ''' 对sub new构造函数的调用只在发生实际的api调用的时候才会发生
    ''' 所以才在这里使用这个属性来标记一些需要在导入程序包的时候自动化运行的代码来进行一些初始化操作
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RInitializeAttribute : Inherits RInteropAttribute

    End Class

    ''' <summary>
    ''' 这个参数是接受``a(x) &lt;- y``操作之中的``y``结果值的
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RByRefValueAssignAttribute : Inherits RInteropAttribute
    End Class

    <AttributeUsage(AttributeTargets.Parameter Or AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class RNameAliasAttribute : Inherits RInteropAttribute

        Public ReadOnly Property [alias] As String

        Sub New([alias] As String)
            Me.alias = [alias]
        End Sub

        Public Overrides Function ToString() As String
            Return [alias]
        End Function
    End Class

    ''' <summary>
    ''' For make compatibale with return value and exception message or R# object wrapper
    ''' The .NET api is usually declare as returns object value, then we could use this
    ''' attribute to let user known the actual returns type of the target api function
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RApiReturnAttribute : Inherits RInteropAttribute

        Public ReadOnly Property returnTypes As Type()

        Sub New(ParamArray type As Type())
            returnTypes = type
        End Sub

        Public Overrides Function ToString() As String
            Return $"fun() -> {returnTypes.Select(Function(type) type.Name).JoinBy("|")}"
        End Function

        Public Shared Function GetActualReturnType(api As MethodInfo) As Type()
            Dim tag As RApiReturnAttribute = api.GetCustomAttribute(Of RApiReturnAttribute)

            If tag Is Nothing Then
                Return {api.ReturnType}
            Else
                Return tag.returnTypes
            End If
        End Function
    End Class

    Public Class RBasePackageAttribute : Inherits PackageAttribute

        Public Sub New([Namespace] As String)
            Call MyBase.New([Namespace])
        End Sub
    End Class

    Public Class RTypeExportAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' type name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String
        Public ReadOnly Property model As Type

        Sub New(name As String, target As Type)
            Me.name = name
            Me.model = target
        End Sub

        Public Overrides Function ToString() As String
            Return $"imports '{name}' = {model.FullName}"
        End Function

    End Class
End Namespace
