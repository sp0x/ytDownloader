Imports System.IO

Namespace Extraction.VideoParsers
    Public Class StreamOutput
        Implements IDisposable

        Protected Property Output As Stream
        Public Property Buffer As MemoryStream
        Public Sub New()
        End Sub
        Public Sub New(ByVal path As String)
            Output = New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024)
            Buffer = New MemoryStream
        End Sub
        Public Sub New(stream As Stream)
            If Not stream.CanWrite Then
                Throw New InvalidOperationException("Stream needs to be writable!")
            End If
            Output = stream
            Buffer = New MemoryStream
        End Sub

#Region "Writing"
        ''' <summary>
        ''' Flushes the buffer
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Flush()
            'Dim tmpBuff As Byte() = Buffer.ToArray()
            Buffer.CopyTo(Output)
            'OutputStream.Write(tmpBuff, 0, tmpBuff.Length)
            Buffer.Dispose()
            Buffer = New MemoryStream() 'Me.chunkBuffer.Clear()
        End Sub
#End Region


        Public Sub Dispose() Implements IDisposable.Dispose
            If Output IsNot Nothing Then Output.Dispose()
        End Sub
    End Class
End Namespace