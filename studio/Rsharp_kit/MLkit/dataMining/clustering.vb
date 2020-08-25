﻿#Region "Microsoft.VisualBasic::d798215b43ce424b392dff7294514147, studio\Rsharp_kit\MLkit\dataMining\clustering.vb"

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

    ' Module clustering
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: btreeClusterFUN, clusterResultDataFrame, clusterSummary, cmeansSummary, dbscan
    '               fuzzyCMeans, hclust, Kmeans, showHclust
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.DBSCAN
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# data clustering tools
''' </summary>
<Package("clustering", Category:=APICategories.ResearchTools, Publisher:="xie.guigang@live.com")>
Module clustering

    Sub New()
        Call REnv.Internal.generic.add("summary", GetType(EntityClusterModel()), AddressOf clusterSummary)

        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(EntityClusterModel()), AddressOf clusterResultDataFrame)
        Call REnv.Internal.Object.Converts.makeDataframe.addHandler(GetType(FuzzyCMeansEntity()), AddressOf cmeansSummary)

        Call REnv.Internal.ConsolePrinter.AttachConsoleFormatter(Of Cluster)(AddressOf showHclust)
    End Sub

    Private Function showHclust(cluster As Cluster) As String
        Return cluster.ToConsoleLine
    End Function

    Public Function clusterSummary(result As Object, args As list, env As Environment) As Object
        If TypeOf result Is EntityClusterModel() Then
            Return DirectCast(result, EntityClusterModel()) _
                .GroupBy(Function(d) d.Cluster) _
                .ToDictionary(Function(d) d.Key,
                              Function(cluster) As Object
                                  Return cluster.Select(Function(d) d.ID).ToArray
                              End Function) _
                .DoCall(Function(slots)
                            Return New list With {
                                .slots = slots
                            }
                        End Function)
        Else
            Throw New NotImplementedException
        End If
    End Function

    Public Function cmeansSummary(cmeans As FuzzyCMeansEntity(), args As list, env As Environment) As Rdataframe
        Dim summary As New Rdataframe With {
            .rownames = cmeans.Keys,
            .columns = New Dictionary(Of String, Array) From {
                {"cluster", cmeans.Select(Function(e) e.probablyMembership).ToArray}
            }
        }

        For Each i As Integer In cmeans(Scan0).memberships.Keys
            summary.columns.Add("cluster" & i, cmeans.Select(Function(e) e.memberships(i)).ToArray)
        Next

        Return summary
    End Function

    Public Function clusterResultDataFrame(data As EntityClusterModel(), args As list, env As Environment) As Rdataframe
        Dim table As File = data.ToCsvDoc
        Dim matrix As New Rdataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        For Each column As String() In table.Columns
            matrix.columns.Add(column(Scan0), column.Skip(1).ToArray)
        Next

        Return matrix
    End Function

    <ExportAPI("cmeans")>
    <RApiReturn(GetType(FuzzyCMeansEntity))>
    Public Function fuzzyCMeans(<RRawVectorArgument>
                                dataset As Object,
                                Optional centers% = 3,
                                Optional fuzzification# = 2,
                                Optional threshold# = 0.001,
                                Optional env As Environment = Nothing) As Object

        Dim data As pipeline = pipeline.TryCreatePipeline(Of DataSet)(dataset, env)

        If data.isError Then
            Return data.getError
        End If

        Dim raw = data.populates(Of DataSet)(env).ToArray
        Dim propertyNames As String() = raw.PropertyNames
        Dim entities As FuzzyCMeansEntity() = raw _
            .Select(Function(d)
                        Return New FuzzyCMeansEntity With {
                            .entityVector = d(propertyNames),
                            .uid = d.ID,
                            .memberships = New Dictionary(Of Integer, Double)
                        }
                    End Function) _
            .ToArray

        Call entities.FuzzyCMeans(
            numberOfClusters:=centers,
            fuzzificationParameter:=fuzzification,
            threshold:=threshold
        )

        Return entities
    End Function

    ''' <summary>
    ''' K-Means Clustering
    ''' </summary>
    ''' <param name="dataset">
    ''' numeric matrix of data, or an object that can be coerced 
    ''' to such a matrix (such as a numeric vector or a data 
    ''' frame with all numeric columns).
    ''' </param>
    ''' <param name="centers">
    ''' either the number of clusters, say k, or a set of initial 
    ''' (distinct) cluster centres. If a number, a random set of 
    ''' (distinct) rows in x is chosen as the initial centres.
    ''' </param>
    ''' <param name="parallel"></param>
    ''' <param name="debug"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("kmeans")>
    <RApiReturn(GetType(EntityClusterModel()))>
    Public Function Kmeans(<RRawVectorArgument>
                           dataset As Object,
                           Optional centers% = 3,
                           Optional parallel As Boolean = True,
                           Optional debug As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        Dim model As EntityClusterModel()

        If dataset Is Nothing Then
            Return Nothing
        ElseIf dataset.GetType.IsArray Then
            Select Case REnv.MeasureArrayElementType(dataset)
                Case GetType(DataSet)
                    model = DirectCast(REnv.asVector(Of DataSet)(dataset), DataSet()).ToKMeansModels
                Case GetType(EntityClusterModel)
                    model = REnv.asVector(Of EntityClusterModel)(dataset)
                Case Else
                    Return REnv.Internal.debug.stop(New InvalidProgramException(dataset.GetType.FullName), env)
            End Select
        Else
            Return REnv.Internal.debug.stop(New InvalidProgramException(dataset.GetType.FullName), env)
        End If

        Return model.Kmeans(centers, debug, parallel).ToArray
    End Function

    ''' <summary>
    ''' Hierarchical Clustering
    ''' 
    ''' Hierarchical cluster analysis on a set of dissimilarities and methods for analyzing it.
    ''' </summary>
    ''' <param name="d">a dissimilarity structure as produced by dist.</param>
    ''' <param name="method">
    ''' the agglomeration method to be used. This should be (an unambiguous abbreviation of) 
    ''' one of "ward.D", "ward.D2", "single", "complete", "average" (= UPGMA), "mcquitty" (= WPGMA), 
    ''' "median" (= WPGMC) or "centroid" (= UPGMC).
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' This function performs a hierarchical cluster analysis using a set of dissimilarities for 
    ''' the n objects being clustered. Initially, each object is assigned to its own cluster and 
    ''' then the algorithm proceeds iteratively, at each stage joining the two most similar clusters, 
    ''' continuing until there is just a single cluster. At each stage distances between clusters 
    ''' are recomputed by the Lance–Williams dissimilarity update formula according to the particular 
    ''' clustering method being used.
    '''
    ''' A number Of different clustering methods are provided. Ward's minimum variance method aims 
    ''' at finding compact, spherical clusters. The complete linkage method finds similar clusters. 
    ''' The single linkage method (which is closely related to the minimal spanning tree) adopts a 
    ''' ‘friends of friends’ clustering strategy. The other methods can be regarded as aiming for 
    ''' clusters with characteristics somewhere between the single and complete link methods. 
    ''' Note however, that methods "median" and "centroid" are not leading to a monotone distance 
    ''' measure, or equivalently the resulting dendrograms can have so called inversions or reversals 
    ''' which are hard to interpret, but note the trichotomies in Legendre and Legendre (2012).
    '''
    ''' Two different algorithms are found In the literature For Ward clustering. The one used by 
    ''' Option "ward.D" (equivalent To the only Ward Option "ward" In R versions &lt;= 3.0.3) does 
    ''' Not implement Ward's (1963) clustering criterion, whereas option "ward.D2" implements that 
    ''' criterion (Murtagh and Legendre 2014). With the latter, the dissimilarities are squared before 
    ''' cluster updating. Note that agnes(*, method="ward") corresponds to hclust(*, "ward.D2").
    '''
    ''' If members!= NULL, Then d Is taken To be a dissimilarity matrix between clusters instead 
    ''' Of dissimilarities between singletons And members gives the number Of observations per cluster. 
    ''' This way the hierarchical cluster algorithm can be 'started in the middle of the dendrogram’, 
    ''' e.g., in order to reconstruct the part of the tree above a cut (see examples). Dissimilarities 
    ''' between clusters can be efficiently computed (i.e., without hclust itself) only for a limited 
    ''' number of distance/linkage combinations, the simplest one being squared Euclidean distance 
    ''' and centroid linkage. In this case the dissimilarities between the clusters are the squared 
    ''' Euclidean distances between cluster means.
    '''
    ''' In hierarchical cluster displays, a decision Is needed at each merge to specify which subtree 
    ''' should go on the left And which on the right. Since, for n observations there are n-1 merges, 
    ''' there are 2^{(n-1)} possible orderings for the leaves in a cluster tree, Or dendrogram. The 
    ''' algorithm used in hclust Is to order the subtree so that the tighter cluster Is on the left 
    ''' (the last, i.e., most recent, merge of the left subtree Is at a lower value than the last 
    ''' merge of the right subtree). Single observations are the tightest clusters possible, And 
    ''' merges involving two observations place them in order by their observation sequence number.
    ''' </remarks>
    <ExportAPI("hclust")>
    <RApiReturn(GetType(Cluster))>
    Public Function hclust(d As DistanceMatrix,
                           Optional method$ = "complete",
                           Optional env As Environment = Nothing) As Object

        If d Is Nothing Then
            Return Internal.debug.stop(New NullReferenceException("the given distance matrix object can not be nothing!"), env)
        End If

        Dim alg As ClusteringAlgorithm = New DefaultClusteringAlgorithm
        Dim matrix As Double()() = d.PopulateRows _
            .Select(Function(a) a.ToArray) _
            .ToArray
        Dim cluster As Cluster = alg.performClustering(matrix, d.keys, New AverageLinkageStrategy)

        Return cluster
    End Function

    ''' <summary>
    ''' do btree clustering
    ''' </summary>
    ''' <param name="d"></param>
    ''' <param name="equals"></param>
    ''' <param name="gt"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("btree")>
    <RApiReturn(GetType(btreeCluster))>
    Public Function btreeClusterFUN(d As DistanceMatrix, Optional equals As Double = 0.9, Optional gt As Double = 0.7, Optional env As Environment = Nothing) As Object
        If d Is Nothing Then
            Return Internal.debug.stop(New NullReferenceException("the given distance matrix object can not be nothing!"), env)
        End If

        Dim compares As Comparison(Of String) =
            Function(x, y) As Integer
                Dim similarity As Double = d(x, y)

                If similarity >= equals Then
                    Return 0
                ElseIf similarity >= gt Then
                    Return 1
                Else
                    Return -1
                End If
            End Function
        Dim btree As New AVLTree(Of String, String)(compares, Function(str) str)

        For Each id As String In d.keys
            For Each id2 As String In d.keys.Where(Function(a) a <> id)
                Call btree.Add(id, id2, valueReplace:=False)
            Next
        Next

        Dim cluster As btreeCluster = btreeCluster.GetClusters(btree)

        Return cluster
    End Function

    ''' <summary>
    ''' ### DBSCAN density reachability and connectivity clustering
    ''' 
    ''' Generates a density based clustering of arbitrary shape as 
    ''' introduced in Ester et al. (1996).
    ''' 
    ''' Clusters require a minimum no of points (MinPts) within a maximum 
    ''' distance (eps) around one of its members (the seed). Any point 
    ''' within eps around any point which satisfies the seed condition 
    ''' is a cluster member (recursively). Some points may not belong to 
    ''' any clusters (noise).
    ''' </summary>
    ''' <param name="data">data matrix, data.frame, dissimilarity matrix 
    ''' or dist-object. Specify method="dist" if the data should be 
    ''' interpreted as dissimilarity matrix or object. Otherwise Euclidean 
    ''' distances will be used.</param>
    ''' <param name="eps">Reachability distance, see Ester et al. (1996).</param>
    ''' <param name="MinPts">Reachability minimum no. Of points, see Ester et al. (1996).</param>
    ''' <param name="scale">scale the data if TRUE.</param>
    ''' <param name="method">
    ''' "dist" treats data as distance matrix (relatively fast but memory 
    ''' expensive), "raw" treats data as raw data and avoids calculating a 
    ''' distance matrix (saves memory but may be slow), "hybrid" expects 
    ''' also raw data, but calculates partial distance matrices (very fast 
    ''' with moderate memory requirements).
    ''' </param>
    ''' <param name="seeds">FALSE to not include the isseed-vector in the dbscan-object.</param>
    ''' <param name="countmode">
    ''' NULL or vector of point numbers at which to report progress.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("dbscan")>
    Public Function dbscan(<RRawVectorArgument> data As Object,
                           eps As Double,
                           Optional MinPts As Integer = 5,
                           Optional scale As Boolean = False,
                           Optional method As dbScanMethods = dbScanMethods.raw,
                           Optional seeds As Boolean = True,
                           Optional countmode As Object = Nothing) As dbscanResult
        Dim x As DataSet()

        If data Is Nothing Then
            Return Nothing
        ElseIf TypeOf data Is Rdataframe Then
            With DirectCast(data, Rdataframe)
                x = .nrows _
                    .Sequence _
                    .Select(Function(i)
                                Dim id As String = .rownames.ElementAtOrDefault(i, i + 1)
                                Dim row As Dictionary(Of String, Object) = .getRowList(i, drop:=True)
                                Dim r As New DataSet With {
                                    .ID = id,
                                    .Properties = row.AsNumeric
                                }

                                Return r
                            End Function) _
                    .ToArray
            End With
        Else
            Throw New NotImplementedException
        End If

        Dim dist As Func(Of DataSet, DataSet, Double)

        Select Case method
            Case dbScanMethods.dist
                x = x.Euclidean.PopulateRowObjects(Of DataSet).ToArray
                dist = Function(a, b) a(b.ID)
            Case dbScanMethods.raw
                Dim all As String() = x.PropertyNames
                dist = Function(a, b) a.Vector.EuclideanDistance(b.Vector)
            Case dbScanMethods.hybrid
                Throw New NotImplementedException
            Case Else
                Throw New NotImplementedException
        End Select

        Dim isseed As Integer() = Nothing
        Dim result = New DbscanAlgorithm(Of DataSet)(dist).ComputeClusterDBSCAN(x, eps, MinPts, isseed)
        Dim clusterData As EntityClusterModel() = result _
            .Select(Function(c)
                        Return c _
                            .Select(Function(r)
                                        Return New EntityClusterModel With {
                                            .Cluster = c.name,
                                            .ID = r.ID,
                                            .Properties = r.Properties
                                        }
                                    End Function)
                    End Function) _
            .IteratesALL _
            .ToArray

        Return New dbscanResult With {
            .cluster = clusterData,
            .eps = eps,
            .MinPts = MinPts,
            .isseed = isseed.Select(Function(i) x(i).ID).ToArray
        }
    End Function
End Module