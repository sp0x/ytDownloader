Namespace Extraction
    ''' <summary>
    ''' The exception that is thrown when an error occurs durin audio extraction.
    ''' </summary>
    Public Class AudioExtractionException
        Inherits Exception
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class


End Namespace