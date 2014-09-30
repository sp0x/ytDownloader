Imports System.IO

Public Class IoFinishedEventArgs
    Inherits EventArgs
    Public Property Path As String
    Public Property Stream As Stream
    Public Property Mode As IOMode = IOMode.File
End Class