Namespace Extraction
    ''' <summary>
    ''' <para>
    ''' The exception that is thrown when the YouTube page could not be parsed.
    ''' This happens, when YouTube changes the structure of their page.
    ''' </para>
    ''' Please report when this exception happens at www.github.com/flagbug/YoutubeExtractor/issues
    ''' </summary>
    Public Class YoutubeParseException
        Inherits Exception
        Public Sub New(message As String, innerException As Exception)
            MyBase.New(message, innerException)
        End Sub
    End Class
End Namespace