Namespace Extraction
    ''' <summary>
    ''' This class holds progress information and completion percentage.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ProgressEventArgs
        Inherits EventArgs
        Public Property IsReady As Boolean
        ''' <summary>
        ''' Gets or sets a token whether the operation that reports the progress should be canceled.
        ''' </summary>
        Public Property Cancel() As Boolean

        ''' <summary>
        ''' Gets the progress percentage in a range from 0.0 to 100.0.
        ''' </summary>
        Public Property ProgressPercentage() As Double

        ''' <summary>
        ''' This holds an additional identification value.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Flag As ProgressFlags
        Public Function GetStatusString(title As String)
            Return String.Format("%{0:F2} {1}", ProgressPercentage, title)
        End Function

        Public Sub New(progressPercentage As Double)
            Me.ProgressPercentage = progressPercentage
        End Sub

    End Class


End Namespace