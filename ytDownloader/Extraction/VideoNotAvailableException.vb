Namespace Extraction
    ''' <summary>
    ''' The exception that is thrown when the video is not available for viewing.
    ''' This can happen when the uploader restricts the video to specific countries.
    ''' </summary>
    Public Class VideoNotAvailableException
        Inherits Exception
        Public Sub New()
        End Sub

        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class
End Namespace