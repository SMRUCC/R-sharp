﻿Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports RDotNET.Extensions.RBase.Vectors

Namespace RBase.MathExtension

    <PackageNamespace("R.Math.Vector", Url:="", Publisher:="", Category:=APICategories.UtilityTools, Description:="")>
    Public Module VectorMath

        ''' <summary>
        ''' abs(x) computes the absolute value of x, sqrt(x) computes the (principal) square root of x, √{x}.
        ''' </summary>
        ''' <param name="x">a numeric or complex vector or array.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Sqrt")>
        Public Function Sqrt(x As Vector) As Vector
            Return New Vector((From n In x.Elements Select Math.Sqrt(n)).ToArray)
        End Function

        ''' <summary>
        ''' log computes logarithms, by default natural logarithms, log10 computes common (i.e., base 10) logarithms, 
        ''' and log2 computes binary (i.e., base 2) logarithms. 
        ''' The general form log(x, base) computes logarithms with base base.
        ''' log1p(x) computes log(1+x) accurately also for |x| &lt;&lt; 1.
        ''' exp computes the exponential function.
        ''' expm1(x) computes exp(x) - 1 accurately also for |x| &lt;&lt; 1.
        ''' </summary>
        ''' <param name="x">a numeric or complex vector.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Exp")>
        Public Function Exp(x As Vector) As Vector
            Return New Vector((From n As Double In x.Elements Select Math.Exp(n)).ToArray)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x">a numeric or complex vector.</param>
        ''' <returns></returns>
        ''' <param name="base">a positive or complex number: the base with respect to which logarithms are computed. Defaults to e=exp(1).</param>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Log")>
        Public Function Log(x As Vector, Optional base As Double = Math.E) As Vector
            Return New Vector((From n As Double In x.Elements Select Math.Log(n, base)).ToArray)
        End Function

        <ExportAPI("Max")>
        Public Function Max(x As Vector) As Double
            Return x.Elements.Max
        End Function

        <ExportAPI("Min")>
        Public Function Min(x As Vector) As Double
            Return x.Elements.Min
        End Function

        ''' <summary>
        ''' Rounding of Numbers
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Trunc")>
        Public Function Trunc(x As Vector) As Vector
            Return x
        End Function

        <ExportAPI("Abs")>
        Public Function Abs(x As Vector) As Vector
            Return New Vector((From d As Double In x.Elements Select Math.Abs(d)).ToArray)
        End Function

        ''' <summary>
        ''' Bessel Functions of integer and fractional order, of first and second kind, J(nu) and Y(nu), 
        ''' and Modified Bessel functions (of first and third kind), I(nu) and K(nu).
        ''' </summary>
        ''' <returns>
        ''' Numeric vector with the (scaled, if expon.scaled = TRUE) values of the corresponding Bessel function.
        ''' The length of the result is the maximum of the lengths of the parameters. All parameters are recycled to that length.
        ''' </returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Besseli")>
        Public Function BesselI(x As Vector, nu As Vector,
                            <Parameter("expon.scaled", "logical; if TRUE, the results are exponentially scaled in order to avoid overflow (I(nu)) or underflow (K(nu)), respectively.")>
                            Optional ExponScaled As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("Floor")>
        Public Function floor(x As Vector) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("Round")>
        Public Function round(x As Vector) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("Sinh")>
        Public Function Sinh(x As Vector) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("Sign")>
        Public Function Sign(x As Vector) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("pchisq")>
        Public Function pchisq(q As Vector, df As Vector, Optional ncp As Integer = 0, Optional lowertail As Boolean = True, Optional logp As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function

        <ExportAPI("Sum")>
        Public Function Sum(x As Vector, Optional NaRM As Boolean = False) As Vector
            Return New Vector({x.Elements.Sum})
        End Function

        <ExportAPI("Sum")>
        Public Function Sum(x As BooleanVector, Optional NaRM As Boolean = False) As Vector
            Dim data = (From b As Boolean In x.Elements Select If(b, 1, 0)).ToArray
            Return New Vector({data.Sum})
        End Function

        ''' <summary>
        ''' Sorting or Ordering Vectors
        ''' Sort (or order) a vector or factor (partially) into ascending or descending order. For ordering along more than one variable, e.g., for sorting data frames, see order.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Sort")>
        Public Function Sort(x As Vector, Optional decreasing As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function

        ''' <summary>
        ''' order returns a permutation which rearranges its first argument into ascending or descending order, breaking ties by further arguments. sort.list is the same, using only one argument.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Order")>
        Public Function Order(x As Vector, Optional nalast As Boolean = True, Optional decreasing As Boolean = False) As Vector
            Throw New NotImplementedException
        End Function

    End Module
End Namespace
