Namespace Extraction
    Public Class YtPlaylist
        Implements IEnumerable(Of YtVideo)
        Public Property Url As String
        Public Sub New(plUrl As String)
            Url = plUrl
        End Sub
        Public Function GetEnumerator() As IEnumerator(Of YtVideo) Implements IEnumerable(Of YtVideo).GetEnumerator
            Return New YtPlaylistEnumerator(Url)
        End Function

        Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return New YtPlaylistEnumerator(Url)
        End Function
    End Class
End NameSpace