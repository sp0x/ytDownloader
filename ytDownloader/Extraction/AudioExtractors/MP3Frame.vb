Namespace Extraction.AudioExtractors
    Public Class Mp3BasicFrame
        Inherits NAudio.Wave.Mp3Frame
        Public Property ChannelMode As Integer
        Public Property BitRate As Integer
        Public Property FirstFrameHeader As UInteger
        Public Property MpegVersion As Integer
        Public Property SampleRate As Integer

        Public Sub New()
        End Sub
    End Class
End Namespace