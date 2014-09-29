Imports System.IO

Namespace Extraction
    Friend Class AacAudioExtractor
        Implements IAudioExtractor
        Private fileStream As FileStream
        Private aacProfile As Integer
        Private channelConfig As Integer
        Private sampleRateIndex As Integer

        Public Sub New(path As String)
            'Me.VideoPath = path
            fileStream = New FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 64 * 1024)
        End Sub

        'Public Property VideoPath() As String Implements IAudioExtractor.VideoPath

        Public Property VideoStream As FileStream Implements IAudioExtractor.VideoStream
            Get
                Return fileStream
            End Get
            Set(value As FileStream)
                fileStream = value
            End Set
        End Property
        Public Sub Dispose() Implements IAudioExtractor.Dispose
            Me.fileStream.Dispose()
        End Sub

        Public Sub WriteChunk(chunk As Byte(), timeStamp As UInteger) Implements IAudioExtractor.WriteChunk
            If chunk.Length < 1 Then
                Return
            End If

            If chunk(0) = 0 Then
                ' Header
                If chunk.Length < 3 Then
                    Return
                End If

                Dim bits As ULong = CULng(BigEndianBitConverter.ToUInt16(chunk, 1)) << 48

                aacProfile = BitHelper.Read(bits, 5) - 1
                sampleRateIndex = BitHelper.Read(bits, 4)
                channelConfig = BitHelper.Read(bits, 4)

                If aacProfile < 0 OrElse aacProfile > 3 Then
                    Throw New AudioExtractionException("Unsupported AAC profile.")
                End If
                If sampleRateIndex > 12 Then
                    Throw New AudioExtractionException("Invalid AAC sample rate index.")
                End If
                If channelConfig > 6 Then
                    Throw New AudioExtractionException("Invalid AAC channel configuration.")
                End If
            Else
                ' Audio data
                Dim dataSize As Integer = chunk.Length - 1
                Dim bits As ULong = 0

                ' Reference: WriteADTSHeader from FAAC's bitstream.c

                BitHelper.Write(bits, 12, &HFFF)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 2, 0)
                BitHelper.Write(bits, 1, 1)
                BitHelper.Write(bits, 2, aacProfile)
                BitHelper.Write(bits, 4, sampleRateIndex)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 3, channelConfig)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 1, 0)
                BitHelper.Write(bits, 13, 7 + dataSize)
                BitHelper.Write(bits, 11, &H7FF)
                BitHelper.Write(bits, 2, 0)

                fileStream.Write(BigEndianBitConverter.GetBytes(bits), 1, 7)
                fileStream.Write(chunk, 1, dataSize)
            End If
        End Sub
    End Class


End Namespace