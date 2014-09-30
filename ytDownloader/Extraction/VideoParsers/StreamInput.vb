Imports System.IO

Namespace Extraction.VideoParsers

    Public MustInherit Class StreamInput
        Implements IDisposable
        Public Property Input As Stream
        Public Property CustomOffset As UInt32
        Public ReadOnly Property Path As String
            Get
                If TypeOf Input Is FileStream Then
                    Return DirectCast(Input, FileStream).Name
                Else
                    Throw New NotImplementedException("Only file parsing is implemented in this version!")
                End If
            End Get
        End Property
        Public Function GetLength() As Long
          If TypeOf Input Is FileStream Then
                Return Input.Length
            Else
                Throw New NotImplementedException("Only file parsing is implemented in this version!")
                Return ""
            End If
        End Function


        Public Sub New(path As String)
            Input = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024)
        End Sub

#Region "Readers"
        Protected Function ReadBytes(len As Integer) As Byte()
            Dim buff As Byte() = New Byte(len - 1) {}
            Input.Read(buff, 0, len)
            CustomOffset += len
            Return buff
        End Function

        Protected Function ReadUInt24() As UInteger
            Dim x As Byte() = New Byte(4) {}
            Me.Input.Read(x, 1, 3)
            CustomOffset += 3
            Return BigEndianBitConverter.ToUInt32(x, 0)
        End Function

        Protected Function ReadUInt32() As UInteger
            Dim x As Byte() = New Byte(4) {}
            Me.Input.Read(x, 0, 4)
            CustomOffset += 4
            Return BigEndianBitConverter.ToUInt32(x, 0)
        End Function

        Protected Function ReadUInt8() As UInteger
            CustomOffset += 1
            Return Me.Input.ReadByte()
        End Function
        Protected Sub Seek(offset As Long)
            Me.Input.Seek(offset, SeekOrigin.Begin)
            Me.CustomOffset = offset
        End Sub
#End Region

        Protected Sub Dispose() Implements IDisposable.Dispose
            If Input IsNot Nothing Then
                Me.Input.Close()
                Me.Input = Nothing
            End If
        End Sub
    End Class

End Namespace