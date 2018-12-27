﻿Imports System.IO

Public Class SerializationExample
    Class MyDeltaEntry
        Implements IDeltaEntry

        ' XmlSerializer cannot serialize the type SavedDeltaEntry, so let's create our own
        ' Note that the type names do not have to be the same when implementing the interface

        Sub New()

        End Sub

        Sub New(copyFrom As IDeltaEntry)
            I = copyFrom.Itemid
            S = copyFrom.Stackid
            M = copyFrom.Metadata
            P = copyFrom.Position
        End Sub

        Public Property I As Long? Implements IDeltaEntry.Itemid

        Public Property S As Long? Implements IDeltaEntry.Stackid

        Public Property M As String Implements IDeltaEntry.Metadata

        Public Property P As Integer? Implements IDeltaEntry.Position
    End Class

    Public Shared Sub Save(source As IEnumerable(Of IDeltaEntry))
        Using f As New SaveFileDialog
            f.DefaultExt = "xml"
            If f.ShowDialog() = DialogResult.OK Then
                Using fs As New FileStream(f.FileName, FileMode.Create, FileAccess.Write)
                    Dim list = source.Select(Function(d) New MyDeltaEntry(d)).ToList()
                    Dim x As New Xml.Serialization.XmlSerializer(list.GetType())
                    x.Serialize(fs, list)
                End Using
            End If
        End Using
    End Sub

    Public Shared Function Load() As IEnumerable(Of IDeltaEntry)
        Using f As New OpenFileDialog
            f.DefaultExt = "xml"
            If f.ShowDialog() = DialogResult.OK Then
                Using fs As New FileStream(f.FileName, FileMode.Open, FileAccess.Read)
                    Dim x As New Xml.Serialization.XmlSerializer((New List(Of MyDeltaEntry)).GetType())
                    Dim list As List(Of MyDeltaEntry) = x.Deserialize(fs)
                    Return list
                End Using
            Else
                Return Nothing
            End If
        End Using
    End Function
End Class
