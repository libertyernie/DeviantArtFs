﻿Imports System.IO
Imports DeviantArtFs

Public Class AccessToken
    Implements IDeviantArtAutomaticRefreshToken

    Private ReadOnly Property Path As String

    Public Property A As String Implements IDeviantArtAccessToken.AccessToken

    Public Property R As String Implements IDeviantArtAutomaticRefreshToken.RefreshToken

    Public ReadOnly Property DeviantArtAuth As IDeviantArtAuth Implements IDeviantArtAutomaticRefreshToken.DeviantArtAuth
        Get
            Return New DeviantArtAuth(DeviantArtClientId, DeviantArtClientSecret)
        End Get
    End Property

    Public Sub New(path As String)
        Me.Path = path
    End Sub

    Public Sub Write()
        Using fs As New FileStream(Path, FileMode.Create, FileAccess.Write)
            Using sw As New StreamWriter(fs)
                sw.WriteLine(A)
                sw.WriteLine(R)
            End Using
        End Using
    End Sub

    Public Shared Function ReadFrom(path As String) As AccessToken
        Using fs As New FileStream(path, FileMode.Open, FileAccess.Read)
            Using sr As New StreamReader(fs)
                Dim a = sr.ReadLine()
                Dim r = sr.ReadLine()
                Return New AccessToken(path) With {.A = a, .R = r}
            End Using
        End Using
    End Function

    Public Function UpdateTokenAsync(newToken As IDeviantArtRefreshToken) As Task Implements IDeviantArtAutomaticRefreshToken.UpdateTokenAsync
        A = newToken.AccessToken
        R = newToken.RefreshToken
        Write()
        Return Task.CompletedTask
    End Function
End Class